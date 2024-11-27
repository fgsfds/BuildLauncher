using Addons.Addons;
using Common;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Common.Serializable.Addon;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using System.Text.Json;

namespace Addons.Providers;

/// <summary>
/// Class that provides lists of installed mods
/// </summary>
public sealed class InstalledAddonsProvider : IInstalledAddonsProvider
{
    private readonly IGame _game;
    private readonly IConfigProvider _config;
    private readonly ILogger _logger;

    private readonly Dictionary<AddonTypeEnum, Dictionary<AddonVersion, IAddon>> _cache;

    private static readonly SemaphoreSlim _semaphore = new(1);

    private bool _isCacheUpdating = false;

    public event AddonChanged? AddonsChangedEvent;

    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProvider)}.")]
    public InstalledAddonsProvider(
        IGame game,
        IConfigProvider config,
        ILogger logger
        )
    {
        _game = game;
        _config = config;
        _logger = logger;
        _cache = [];
    }


    /// <summary>
    /// Try to parse and copy addon into suitable folder
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    /// <returns>Copied successfully</returns>
    public async Task<bool> CopyAddonIntoFolder(string pathToFile)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            !pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var addon = GetGameAndTypeFromFile(pathToFile);

        if (addon is null)
        {
            return false;
        }

        if (addon.Item1 != _game.GameEnum)
        {
            return false;
        }

        string folderToPutFile;

        if (addon.Item2 is AddonTypeEnum.TC)
        {
            folderToPutFile = _game.CampaignsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Map)
        {
            folderToPutFile = _game.MapsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Mod)
        {
            folderToPutFile = _game.ModsFolderPath;
        }
        else
        {
            return false;
        }

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        await AddAddonAsync(newPathToFile).ConfigureAwait(false);

        return true;
    }


    /// <inheritdoc/>
    public async Task CreateCache(bool createNew)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        _isCacheUpdating = true;

        if (createNew)
        {
            _cache.Clear();
        }

        try
        {
            if (_cache.Count == 0)
            {
                await Task.Run(async () =>
                {
                    //campaigns
                    List<string> filesTcs = [.. Directory.GetFiles(_game.CampaignsFolderPath, "*.zip")];

                    var dirs = Directory.GetDirectories(_game.CampaignsFolderPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var dir in dirs)
                    {
                        var addonJsons = Directory.GetFiles(dir, "addon*.json");

                        if (addonJsons.Length > 0)
                        {
                            filesTcs.AddRange(addonJsons);
                        }
                    }

                    var tcs = await GetAddonsFromFilesAsync(filesTcs).ConfigureAwait(false);
                    _cache.Add(AddonTypeEnum.TC, tcs);


                    //grpinfo
                    List<string> foldersWithGrpInfos = [];
                    dirs = Directory.GetDirectories(_game.CampaignsFolderPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var dir in dirs)
                    {
                        if (File.Exists(Path.Combine(dir, "addons.grpinfo")))
                        {
                            foldersWithGrpInfos.Add(dir);
                        }
                    }

                    if (foldersWithGrpInfos.Count > 0)
                    {
                        var grpInfoAddons = GrpInfoProvider.GetAddonsFromGrpInfo(foldersWithGrpInfos);

                        if (grpInfoAddons.Count > 0)
                        {
                            foreach (var addon in grpInfoAddons)
                            {
                                _cache[AddonTypeEnum.TC].Add(new(addon.Id, null), addon);
                            }
                        }
                    }


                    //maps
                    var filesMaps = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
                    var maps = await GetAddonsFromFilesAsync(filesMaps).ConfigureAwait(false);
                    _cache.Add(AddonTypeEnum.Map, maps);


                    //mods
                    var filesMods = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = await GetAddonsFromFilesAsync(filesMods).ConfigureAwait(false);

                    _cache.Add(AddonTypeEnum.Mod, mods);

                    //enabling/disabling addons
                    foreach (var mod in mods)
                    {
                        if (mod.Value is not AutoloadMod autoloadMod)
                        {
                            _logger.LogError($"=== Error while enabling/disabling addon {mod.Key.Id}");
                            continue;
                        }

                        if (autoloadMod.IsEnabled)
                        {
                            EnableAddon(mod.Key);
                        }
                        else if (!autoloadMod.IsEnabled)
                        {
                            DisableAddon(mod.Key);
                        }
                    }

                }).WaitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            AddonsChangedEvent?.Invoke(_game, null);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while creating installed addons cache ===");
        }
        finally
        {
            _isCacheUpdating = false;
            _ = _semaphore.Release();
            Guard.IsNotNull(_cache);
        }
    }

    /// <inheritdoc/>
    public async Task AddAddonAsync(string pathToFile)
    {
        Guard.IsNotNull(_cache);

        var addons = await GetAddonFromFileAsync(pathToFile).ConfigureAwait(false);

        if (addons is null or [])
        {
            await CreateCache(true).ConfigureAwait(false);
            return;
        }

        foreach (var addon in addons)
        {
            if (!_cache.TryGetValue(addon.Type, out _))
            {
                _cache.Add(addon.Type, []);
            }

            var dict = _cache[addon.Type];

            if (dict.TryGetValue(new(addon.Id, addon.Version), out _))
            {
                dict[new(addon.Id, addon.Version)] = addon;
            }
            else
            {
                dict.Add(new(addon.Id, addon.Version), addon);
            }

            AddonsChangedEvent?.Invoke(_game, addon.Type);
        }
    }

    /// <inheritdoc/>
    public void DeleteAddon(IAddon addon)
    {
        Guard.IsNotNull(_cache);
        Guard.IsNotNull(addon.PathToFile);

        addon.GridImage?.Dispose();
        addon.PreviewImage?.Dispose();

        if (addon.IsFolder)
        {
            Directory.Delete(Path.GetDirectoryName(addon.PathToFile)!, true);
        }
        else
        {
            File.Delete(addon.PathToFile);
        }

        if (addon is LooseMap lMap)
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(addon.PathToFile)!, $"{Path.GetFileNameWithoutExtension(lMap.FileName)!}.*");

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        _ = _cache[addon.Type].Remove(new(addon.Id, addon.Version));

        AddonsChangedEvent?.Invoke(_game, addon.Type);
    }

    /// <inheritdoc/>
    public void EnableAddon(AddonVersion addon)
    {
        if (!_cache[AddonTypeEnum.Mod].TryGetValue(addon, out var mod) ||
            mod is not AutoloadMod autoloadMod)
        {
            return;
        }

        autoloadMod.IsEnabled = true;

        if (autoloadMod.DependentAddons is not null)
        {
            foreach (var dep in autoloadMod.DependentAddons)
            {
                EnableAddon(new() { Id = dep.Key, Version = dep.Value });
            }
        }

        if (autoloadMod.IncompatibleAddons is not null)
        {
            foreach (var inc in autoloadMod.IncompatibleAddons)
            {
                DisableAddon(new() { Id = inc.Key, Version = inc.Value });
            }
        }

        _config.ChangeModState(addon, true);
    }

    /// <inheritdoc/>
    public void DisableAddon(AddonVersion addon)
    {
        if (!_cache[AddonTypeEnum.Mod].TryGetValue(addon, out var mod) ||
            mod is not AutoloadMod autoloadMod)
        {
            return;
        }

        autoloadMod.IsEnabled = false;

        var deps = _cache[AddonTypeEnum.Mod].Where(x => x.Value.DependentAddons?.ContainsKey(autoloadMod.Id) ?? false);

        foreach (var dep in deps)
        {
            DisableAddon(dep.Key);
        }

        _config.ChangeModState(addon, false);
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetInstalledAddonsByType(AddonTypeEnum addonType)
    {
        return addonType switch
        {
            AddonTypeEnum.TC => GetInstalledCampaigns(),
            AddonTypeEnum.Map => GetInstalledMaps(),
            AddonTypeEnum.Mod => GetInstalledMods(),
            _ => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonVersion, IAddon>>()
        };
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetInstalledCampaigns()
    {
        var campaigns = OriginalCampaignsProvider.GetOriginalCampaigns(_game);

        if (_isCacheUpdating)
        {
            return campaigns;
        }

        Guard.IsNotNull(_cache);

        _ = _cache.TryGetValue(AddonTypeEnum.TC, out var result);

        if (result is not null)
        {
            //hack to make SW addons appear at the top of the list
            foreach (var customCamp in result
                .OrderByDescending(static x => x.Key.Id.Equals(nameof(WangAddonEnum.TwinDragon), StringComparison.InvariantCultureIgnoreCase))
                .ThenByDescending(static x => x.Key.Id.Equals(nameof(WangAddonEnum.Wanton), StringComparison.InvariantCultureIgnoreCase))
                .ThenBy(static x => x.Value.Title))
            {
                campaigns.Add(customCamp.Key, customCamp.Value);
            }
        }

        return campaigns;
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetInstalledMaps()
    {
        if (_isCacheUpdating)
        {
            return [];
        }

        Guard.IsNotNull(_cache);

        _ = _cache.TryGetValue(AddonTypeEnum.Map, out var result);

        return result ?? [];
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetInstalledMods()
    {
        if (_isCacheUpdating)
        {
            return [];
        }

        Guard.IsNotNull(_cache);

        _ = _cache.TryGetValue(AddonTypeEnum.Mod, out var result);

        return result ?? [];
    }

    /// <summary>
    /// Get addons from list of files
    /// </summary>
    /// <param name="files">Paths to addon files</param>
    private async Task<Dictionary<AddonVersion, IAddon>> GetAddonsFromFilesAsync(IEnumerable<string> files)
    {
        Dictionary<AddonVersion, IAddon> addedAddons = [];

        foreach (var file in files)
        {
            try
            {
                var newAddons = await GetAddonFromFileAsync(file).ConfigureAwait(false);

                if (newAddons is null or [])
                {
                    continue;
                }

                foreach (var newAddon in newAddons)
                {
                    try
                    {
                        if (newAddon is AutoloadMod &&
                            addedAddons.TryGetValue(new(newAddon.Id, newAddon.Version), out var existingMod))
                        {
                            if (existingMod.Version is null &&
                                newAddon.Version is not null)
                            {
                                //replacing with addon that have version
                                addedAddons[new(newAddon.Id, newAddon.Version)] = newAddon;
                            }
                            else if (existingMod.Version is not null &&
                                     newAddon.Version is not null &&
                                     VersionComparer.Compare(newAddon.Version, existingMod.Version, ">"))
                            {
                                //replacing with addon that have higher version
                                addedAddons[new(newAddon.Id, newAddon.Version)] = newAddon;
                            }
                        }
                        else
                        {
                            _ = addedAddons.TryAdd(new(newAddon.Id, newAddon.Version), newAddon);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                continue;
            }
        }

        return addedAddons;
    }

    /// <summary>
    /// Get game enum and addon type enum from a file
    /// </summary>
    /// <param name="pathToFile">Path to file</param>
    private Tuple<GameEnum, AddonTypeEnum>? GetGameAndTypeFromFile(string pathToFile)
    {
        if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.Open(pathToFile);
            var manifestFile = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

            if (manifestFile is null)
            {
                return null;
            }

            using var stream = manifestFile.OpenEntryStream();

            var manifest = JsonSerializer.Deserialize(
                stream,
                AddonManifestContext.Default.AddonDto
                );

            if (manifest is null)
            {
                return null;
            }

            var supportedGame = manifest.SupportedGame.Game;
            var type = manifest.AddonType;

            return new(supportedGame, type);
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return new(_game.GameEnum, AddonTypeEnum.Map);
        }

        return null;
    }

    /// <summary>
    /// Get addon from a file
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    private async Task<List<BaseAddon>?> GetAddonFromFileAsync(string pathToFile)
    {
        List<AddonCarcass> carcasses = [];

        if (pathToFile.EndsWith(".json"))
        {
            AddonCarcass carcass = new();

            carcass.IsUnpacked = true;

            var jsonText = File.ReadAllText(pathToFile);

            var manifest = JsonSerializer.Deserialize(
                jsonText,
                AddonManifestContext.Default.AddonDto
                );

            if (manifest is null)
            {
                return null;
            }

            var addonDir = Path.GetDirectoryName(pathToFile)!;

            var gridFile = Directory.GetFiles(addonDir, "grid.*");
            var previewFile = Directory.GetFiles(addonDir, "preview.*");

            if (gridFile.Length > 0)
            {
                var stream = await File.ReadAllBytesAsync(gridFile[0]).ConfigureAwait(false);
                carcass.Image = new MemoryStream(stream);
            }
            else
            {
                carcass.Image = null;
            }

            if (previewFile.Length > 0)
            {
                var stream = await File.ReadAllBytesAsync(previewFile[0]).ConfigureAwait(false);
                carcass.Preview = new MemoryStream(stream);
            }
            else
            {
                carcass.Preview = null;
            }

            carcass.Type = manifest.AddonType;
            carcass.Id = manifest.Id;
            carcass.Title = manifest.Title;
            carcass.Author = manifest.Author;
            carcass.Image ??= carcass.Preview;
            carcass.Version = manifest.Version;
            carcass.Description = manifest.Description;
            carcass.SupportedGame = manifest.SupportedGame.Game;
            carcass.GameVersion = manifest.SupportedGame.Version;
            carcass.GameCrc = manifest.SupportedGame.Crc;

            carcass.Rts = manifest.Rts;
            carcass.Ini = manifest.Ini;
            carcass.Rff = manifest.MainRff;
            carcass.Snd = manifest.SoundRff;

            carcass.StartMap = manifest.StartMap;

            carcass.RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToHashSet();

            carcass.MainCon = manifest.MainCon;
            carcass.AddCons = manifest.AdditionalCons?.ToHashSet();

            carcass.MainDef = manifest.MainDef;
            carcass.AddDefs = manifest.AdditionalDefs?.ToHashSet();

            carcass.Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
            carcass.Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

            if (manifest.Executables is not null)
            {
                carcass.Executables = [];

                foreach (var exe in manifest.Executables)
                {
                    carcass.Executables.Add(exe.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, exe.Value));
                }
            }
            else
            {
                carcass.Executables = null;
            }

            carcasses.Add(carcass);
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            var bloodIni = pathToFile.Replace(".map", ".ini", StringComparison.InvariantCultureIgnoreCase);
            var iniExists = File.Exists(bloodIni);

            var addon = new LooseMap()
            {
                Id = Path.GetFileName(pathToFile),
                Type = AddonTypeEnum.Map,
                Title = Path.GetFileName(pathToFile),
                SupportedGame = new(_game.GameEnum, null, null),
                PathToFile = pathToFile,
                StartMap = new MapFileDto() { File = Path.GetFileName(pathToFile) },
                BloodIni = iniExists ? bloodIni : null,
                GridImage = null,
                Description = null,
                Version = null,
                Author = null,
                MainDef = null,
                AdditionalDefs = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            };

            return [addon];
        }
        else if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.Open(pathToFile);
            var unpackedTo = UnpackIfNeededAndGetAddonDto(pathToFile, archive, out var manifests);

            if (manifests is null or [])
            {
                return null;
            }

            bool isUnpacked = false;
            Stream? image;
            Stream? preview;

            if (unpackedTo is not null)
            {
                pathToFile = Path.Combine(unpackedTo, "addon.json");
                isUnpacked = true;

                var addonDir = Path.GetDirectoryName(pathToFile)!;

                var gridFile = Directory.GetFiles(addonDir, "grid.*");
                var previewFile = Directory.GetFiles(addonDir, "preview.*");

                if (gridFile.Length > 0)
                {
                    var stream = await File.ReadAllBytesAsync(gridFile[0]).ConfigureAwait(false);
                    image = new MemoryStream(stream);
                }
                else
                {
                    image = null;
                }

                if (previewFile.Length > 0)
                {
                    var stream = await File.ReadAllBytesAsync(previewFile[0]).ConfigureAwait(false);
                    preview = new MemoryStream(stream);
                }
                else
                {
                    preview = null;
                }
            }
            else
            {
                isUnpacked = false;

                preview = ImageHelper.GetImageFromArchive(archive, "preview.png");
                image = ImageHelper.GetCoverFromArchive(archive) ?? preview;
            }

            foreach (var manifest in manifests)
            {
                AddonCarcass carcass = new();

                carcass.IsUnpacked = isUnpacked;
                carcass.Image = image;
                carcass.Preview = preview;

                carcass.Type = manifest.AddonType;
                carcass.Id = manifest.Id;
                carcass.Title = manifest.Title;
                carcass.Author = manifest.Author;
                carcass.Image ??= carcass.Preview;
                carcass.Version = manifest.Version;
                carcass.Description = manifest.Description;
                carcass.SupportedGame = manifest.SupportedGame.Game;
                carcass.GameVersion = manifest.SupportedGame.Version;
                carcass.GameCrc = manifest.SupportedGame.Crc;

                carcass.Rts = manifest.Rts;
                carcass.Ini = manifest.Ini;
                carcass.Rff = manifest.MainRff;
                carcass.Snd = manifest.SoundRff;

                carcass.StartMap = manifest.StartMap;

                carcass.RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToHashSet();

                carcass.MainCon = manifest.MainCon;
                carcass.AddCons = manifest.AdditionalCons?.ToHashSet();

                carcass.MainDef = manifest.MainDef;
                carcass.AddDefs = manifest.AdditionalDefs?.ToHashSet();

                carcass.Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
                carcass.Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

                if (manifest.Executables is not null)
                {
                    carcass.Executables = [];

                    foreach (var exe in manifest.Executables)
                    {
                        carcass.Executables.Add(exe.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, exe.Value));
                    }
                }
                else
                {
                    carcass.Executables = null;
                }

                carcasses.Add(carcass);
            }
        }
        else if (pathToFile.EndsWith(".grp", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        else
        {
            return null;
        }

        List<BaseAddon> addons = [];

        foreach (var carcass in carcasses)
        {
            if (carcass.Type is AddonTypeEnum.Mod)
            {
                var isEnabled = !_config.DisabledAutoloadMods.Contains(carcass.Id);

                if (carcass.MainDef is not null)
                {
                    ThrowHelper.ThrowArgumentException("Autoload mod can't have Main DEF");
                }

                var addon = new AutoloadMod()
                {
                    Id = carcass.Id,
                    Type = AddonTypeEnum.Mod,
                    Title = carcass.Title,
                    GridImage = carcass.Image,
                    Description = carcass.Description,
                    Version = carcass.Version,
                    Author = carcass.Author,
                    IsEnabled = isEnabled,
                    PathToFile = pathToFile,
                    MainDef = null,
                    AdditionalDefs = carcass.AddDefs,
                    AdditionalCons = carcass.AddCons,
                    SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    RequiredFeatures = carcass.RequiredFeatures,
                    PreviewImage = carcass.Preview,
                    IsFolder = carcass.IsUnpacked,
                    Executables = null
                };

                addons.Add(addon);
            }
            else
            {
                if (_game.GameEnum
                    is GameEnum.Duke3D
                    or GameEnum.Fury
                    or GameEnum.Redneck
                    or GameEnum.NAM
                    or GameEnum.WW2GI)
                {
                    var addon = new DukeCampaign()
                    {
                        Id = carcass.Id,
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImage = carcass.Image,
                        Description = carcass.Description,
                        Version = carcass.Version,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainCon = carcass.MainCon,
                        AdditionalCons = carcass.AddCons,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RTS = carcass.Rts,
                        RequiredFeatures = carcass.RequiredFeatures,
                        PreviewImage = carcass.Preview,
                        IsFolder = carcass.IsUnpacked,
                        Executables = carcass.Executables
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.ShadowWarrior)
                {
                    var addon = new WangCampaign()
                    {
                        Id = carcass.Id,
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImage = carcass.Image,
                        Description = carcass.Description,
                        Version = carcass.Version,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        PreviewImage = carcass.Preview,
                        IsFolder = carcass.IsUnpacked,
                        Executables = carcass.Executables
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Blood)
                {
                    var addon = new BloodCampaign()
                    {
                        Id = carcass.Id,
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImage = carcass.Image,
                        Description = carcass.Description,
                        Version = carcass.Version,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        INI = carcass.Ini,
                        RFF = carcass.Rff,
                        SND = carcass.Snd,
                        RequiredFeatures = carcass.RequiredFeatures,
                        PreviewImage = carcass.Preview,
                        IsFolder = carcass.IsUnpacked,
                        Executables = carcass.Executables
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Exhumed)
                {
                    var addon = new SlaveCampaign()
                    {
                        Id = carcass.Id,
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImage = carcass.Image,
                        Description = carcass.Description,
                        Version = carcass.Version,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        PreviewImage = carcass.Preview,
                        IsFolder = carcass.IsUnpacked,
                        Executables = carcass.Executables
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Standalone)
                {
                    var addon = new StandaloneAddon()
                    {
                        Id = carcass.Id,
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImage = carcass.Image,
                        Description = carcass.Description,
                        Version = carcass.Version,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        PreviewImage = carcass.Preview,
                        IsFolder = carcass.IsUnpacked,
                        Executables = carcass.Executables
                    };

                    addons.Add(addon);
                }
                else
                {
                    ThrowHelper.ThrowNotSupportedException();
                }
            }
        }

        return addons;
    }

    /// <summary>
    /// Unpack archive if needed and return path to folder
    /// </summary>
    /// <param name="pathToFile">Path to archive</param>
    /// <param name="archive">Archive</param>
    /// <param name="addonDtos">AddonDto</param>
    private string? UnpackIfNeededAndGetAddonDto(string pathToFile, IArchive archive, out List<AddonDto>? addonDtos)
    {
        try
        {
            string? unpackedTo = null;

            if (archive.Entries.Any(static x => x.Key!.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase)))
            {
                //need to unpack archive with grpinfo
                unpackedTo = Unpack(pathToFile, archive);
                addonDtos = null;
                return unpackedTo;
            }

            var addonJsonsInsideArchive = archive.Entries.Where(static x => x.Key!.StartsWith("addon") && x.Key!.EndsWith(".json"));

            if (addonJsonsInsideArchive?.Any() is not true)
            {
                addonDtos = null;
                return null;
            }

            using var addonJsonStream = addonJsonsInsideArchive.First().OpenEntryStream();

            var addonDto = JsonSerializer.Deserialize(
                addonJsonStream,
                AddonManifestContext.Default.AddonDto
                )!;

            if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
            {
                //need to unpack addons that contain custom RFF files
                unpackedTo = Unpack(pathToFile, archive);
            }
            else if (addonDto.Executables is not null)
            {
                //need to unpack addons with custom executables
                unpackedTo = Unpack(pathToFile, archive);
            }

            List<AddonDto> result = [];

            if (unpackedTo is not null)
            {
                var unpackedAddonJsons = Directory.GetFiles(unpackedTo, "addon*.json");

                foreach (var addonJson in unpackedAddonJsons)
                {
                    var text = File.ReadAllText(addonJson);

                    var addonDto2 = JsonSerializer.Deserialize(
                        text,
                        AddonManifestContext.Default.AddonDto
                        )!;

                    result.Add(addonDto2);
                }
            }
            else
            {
                foreach (var addonJson in addonJsonsInsideArchive)
                {
                    using var addonJsonStream2 = addonJson.OpenEntryStream();

                    var addonDto2 = JsonSerializer.Deserialize(
                        addonJsonStream2,
                        AddonManifestContext.Default.AddonDto
                        )!;

                    result.Add(addonDto2);
                }
            }

            addonDtos = result.Count > 0 ? result : null;
            return unpackedTo;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while unpacking archive ===");

            addonDtos = null;
            return null;
        }
    }

    /// <summary>
    /// Unpack archive and return path to folder
    /// </summary>
    /// <param name="pathToFile">Path to archive</param>
    /// <param name="archive">Archive</param>
    private string Unpack(string pathToFile, IArchive archive)
    {
        var fileFolder = Path.GetDirectoryName(pathToFile)!;
        var unpackTo = Path.Combine(fileFolder, Path.GetFileNameWithoutExtension(pathToFile));

        if (Directory.Exists(unpackTo))
        {
            Directory.Delete(unpackTo, true);
        }

        archive.ExtractToDirectory(unpackTo);

        archive.Dispose();
        File.Delete(pathToFile);

        return unpackTo;
    }
}

internal struct AddonCarcass
{
    public GameEnum SupportedGame { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public AddonTypeEnum Type { get; set; }
    public string Version { get; set; }
    public bool IsUnpacked { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public Stream? Image { get; set; }
    public Stream? Preview { get; set; }
    public string? GameVersion { get; set; }
    public string? GameCrc { get; set; }
    public HashSet<FeatureEnum>? RequiredFeatures { get; set; }
    public string? MainCon { get; set; }
    public HashSet<string>? AddCons { get; set; }
    public string? MainDef { get; set; }
    public HashSet<string>? AddDefs { get; set; }
    public string? Rts { get; set; }
    public string? Ini { get; set; }
    public string? Rff { get; set; }
    public string? Snd { get; set; }
    public Dictionary<string, string?>? Dependencies { get; set; }
    public Dictionary<string, string?>? Incompatibles { get; set; }
    public IStartMap? StartMap { get; set; }
    public Dictionary<OSEnum, string>? Executables { get; set; }
}
