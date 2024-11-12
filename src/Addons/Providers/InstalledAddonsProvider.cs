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

    private readonly Dictionary<AddonTypeEnum, Dictionary<AddonVersion, IAddon>> _cache;

    private static readonly SemaphoreSlim _semaphore = new(1);

    private bool _isCacheUpdating = false;

    public event AddonChanged? AddonsChangedEvent;

    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProvider)}.")]
    public InstalledAddonsProvider(
        IGame game,
        IConfigProvider config
        )
    {
        _game = game;
        _config = config;
        _cache = [];
    }


    /// <summary>
    /// Try to parse and copy addon into suitable folder
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
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
                        if (File.Exists(Path.Combine(dir, "addon.json")))
                        {
                            filesTcs.Add(Path.Combine(dir, "addon.json"));
                        }
                    }

                    var tcs = await GetAddonsFromFilesAsync(filesTcs).ConfigureAwait(false);
                    _cache.Add(AddonTypeEnum.TC, tcs);


                    //grpinfo
                    List<string> foldersGrpInfos = [];

                    var dirs2 = Directory.GetDirectories(_game.CampaignsFolderPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var dir in dirs2)
                    {
                        if (File.Exists(Path.Combine(dir, "addons.grpinfo")))
                        {
                            foldersGrpInfos.Add(dir);
                        }
                    }

                    if (foldersGrpInfos.Count > 0)
                    {
                        var grpInfoAddons = GrpInfoProvider.GetAddonsFromGrpInfo(foldersGrpInfos);

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

                }).WaitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            AddonsChangedEvent?.Invoke(_game, null);
        }
        catch
        {
            //do nothing
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

        var addon = await GetAddonFromFileAsync(pathToFile).ConfigureAwait(false);

        if (addon is null)
        {
            await CreateCache(true).ConfigureAwait(false);
            return;
        }

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

    /// <inheritdoc/>
    public void DeleteAddon(IAddon addon)
    {
        Guard.IsNotNull(_cache);
        Guard.IsNotNull(addon.PathToFile);

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
    public void EnableAddon(AddonVersion addonId)
    {
        ((AutoloadMod)_cache[AddonTypeEnum.Mod][addonId]).IsEnabled = true;

        _config.ChangeModState(addonId, true);
    }

    /// <inheritdoc/>
    public void DisableAddon(AddonVersion addonId)
    {
        ((AutoloadMod)_cache[AddonTypeEnum.Mod][addonId]).IsEnabled = false;

        _config.ChangeModState(addonId, false);
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
            foreach (var customCamp in result
                    //hack so SW addons end up at the beginning of the list
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

        return result is null ? [] : result;
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

        return result is null ? [] : result;
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
                var newAddon = await GetAddonFromFileAsync(file).ConfigureAwait(false);

                if (newAddon is null)
                {
                    continue;
                }

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
                    addedAddons.Add(new(newAddon.Id, newAddon.Version), newAddon);
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
    private async Task<BaseAddon?> GetAddonFromFileAsync(string pathToFile)
    {
        var supportedGame = _game.GameEnum;

        string id;
        string title;
        AddonTypeEnum type;
        string version;
        bool isUnpacked;

        string? author;
        string? description;
        Stream? image;
        Stream? preview;
        string? gameVersion;
        string? gameCrc;
        HashSet<FeatureEnum>? requiredFeatures;
        string? mainCon;
        HashSet<string>? addCons;
        string? mainDef;
        HashSet<string>? addDefs;
        string? rts;
        string? ini;
        string? rff;
        string? snd;
        Dictionary<string, string?>? dependencies;
        Dictionary<string, string?>? incompatibles;
        IStartMap? startMap;
        Dictionary<OSEnum, string>? executables;

        if (pathToFile.EndsWith(".json"))
        {
            isUnpacked = true;

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

            type = manifest.AddonType;
            id = manifest.Id;
            title = manifest.Title;
            author = manifest.Author;
            image = image ?? preview;
            version = manifest.Version;
            description = manifest.Description;
            supportedGame = manifest.SupportedGame.Game;
            gameVersion = manifest.SupportedGame.Version;
            gameCrc = manifest.SupportedGame.Crc;

            rts = manifest.Rts;
            ini = manifest.Ini;
            rff = manifest.MainRff;
            snd = manifest.SoundRff;

            startMap = manifest.StartMap;

            requiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToHashSet();

            mainCon = manifest.MainCon;
            addCons = manifest.AdditionalCons?.ToHashSet();

            mainDef = manifest.MainDef;
            addDefs = manifest.AdditionalDefs?.ToHashSet();

            dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
            incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

            if (manifest.Executables is not null)
            {
                executables = [];

                foreach (var exe in manifest.Executables)
                {
                    executables.Add(exe.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, exe.Value));
                }
            }
            else
            {
                executables = null;
            }
        }
        else if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.Open(pathToFile);
            var unpackedTo = UnpackIfNeededAndGetAddonDto(pathToFile, archive, out var manifest);

            if (manifest is null)
            {
                return null;
            }

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

            type = manifest.AddonType;
            id = manifest.Id;
            title = manifest.Title;
            author = manifest.Author;
            image = image ?? preview;
            version = manifest.Version;
            description = manifest.Description;
            supportedGame = manifest.SupportedGame.Game;
            gameVersion = manifest.SupportedGame.Version;
            gameCrc = manifest.SupportedGame.Crc;

            rts = manifest.Rts;
            ini = manifest.Ini;
            rff = manifest.MainRff;
            snd = manifest.SoundRff;

            startMap = manifest.StartMap;

            requiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToHashSet();

            mainCon = manifest.MainCon;
            addCons = manifest.AdditionalCons?.ToHashSet();

            mainDef = manifest.MainDef;
            addDefs = manifest.AdditionalDefs?.ToHashSet();

            dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
            incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

            if (manifest.Executables is not null)
            {
                executables = [];

                foreach (var exe in manifest.Executables)
                {
                    executables.Add(exe.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, exe.Value));
                }
            }
            else
            {
                executables = null;
            }
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
                SupportedGame = new(supportedGame, null, null),
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

            return addon;
        }
        else if (pathToFile.EndsWith(".grp", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        else
        {
            return null;
        }


        if (type is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(id);

            if (mainDef is not null)
            {
                ThrowHelper.ThrowArgumentException("Autoload mod can't have Main DEF");
            }

            var addon = new AutoloadMod()
            {
                Id = id,
                Type = AddonTypeEnum.Mod,
                Title = title,
                GridImage = image,
                Description = description,
                Version = version,
                Author = author,
                IsEnabled = isEnabled,
                PathToFile = pathToFile,
                MainDef = null,
                AdditionalDefs = addDefs,
                AdditionalCons = addCons,
                SupportedGame = new(supportedGame, gameVersion, gameCrc),
                DependentAddons = dependencies,
                IncompatibleAddons = incompatibles,
                StartMap = startMap,
                RequiredFeatures = requiredFeatures,
                PreviewImage = preview,
                IsFolder = isUnpacked,
                Executables = null
            };

            return addon;
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
                    Id = id,
                    Type = type,
                    SupportedGame = new(supportedGame, gameVersion, gameCrc),
                    Title = title,
                    GridImage = image,
                    Description = description,
                    Version = version,
                    Author = author,
                    PathToFile = pathToFile,
                    DependentAddons = dependencies,
                    IncompatibleAddons = incompatibles,
                    StartMap = startMap,
                    MainCon = mainCon,
                    AdditionalCons = addCons,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    RTS = rts,
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    Executables = executables
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.ShadowWarrior)
            {
                var addon = new WangCampaign()
                {
                    Id = id,
                    Type = type,
                    SupportedGame = new(supportedGame, gameVersion, gameCrc),
                    Title = title,
                    GridImage = image,
                    Description = description,
                    Version = version,
                    Author = author,
                    PathToFile = pathToFile,
                    DependentAddons = dependencies,
                    IncompatibleAddons = incompatibles,
                    StartMap = startMap,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    Executables = executables
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.Blood)
            {
                var addon = new BloodCampaign()
                {
                    Id = id,
                    Type = type,
                    SupportedGame = new(supportedGame, gameVersion, gameCrc),
                    Title = title,
                    GridImage = image,
                    Description = description,
                    Version = version,
                    Author = author,
                    PathToFile = pathToFile,
                    DependentAddons = dependencies,
                    IncompatibleAddons = incompatibles,
                    StartMap = startMap,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    INI = ini,
                    RFF = rff,
                    SND = snd,
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    Executables = executables
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.Exhumed)
            {
                var addon = new SlaveCampaign()
                {
                    Id = id,
                    Type = type,
                    SupportedGame = new(supportedGame, gameVersion, gameCrc),
                    Title = title,
                    GridImage = image,
                    Description = description,
                    Version = version,
                    Author = author,
                    PathToFile = pathToFile,
                    DependentAddons = dependencies,
                    IncompatibleAddons = incompatibles,
                    StartMap = startMap,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    Executables = executables
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.Standalone)
            {
                var addon = new StandaloneAddon()
                {
                    Id = id,
                    Type = type,
                    SupportedGame = new(supportedGame, gameVersion, gameCrc),
                    Title = title,
                    GridImage = image,
                    Description = description,
                    Version = version,
                    Author = author,
                    PathToFile = pathToFile,
                    DependentAddons = dependencies,
                    IncompatibleAddons = incompatibles,
                    StartMap = startMap,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    Executables = executables
                };

                return addon;
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
                return null;
            }
        }
    }

    /// <summary>
    /// Unpack archive if needed and return path to folder
    /// </summary>
    /// <param name="pathToFile">Path to archive</param>
    /// <param name="archive">Archive</param>
    /// <param name="addonDto">AddonDto</param>
    private string? UnpackIfNeededAndGetAddonDto(string pathToFile, IArchive archive, out AddonDto? addonDto)
    {
        IArchiveEntry? addonJson = null;

        foreach (var file in archive.Entries)
        {
            if (file.Key!.Equals("addons.grpinfo"))
            {
                //need to unpack archive with grpinfo
                var unpackedTo = Unpack(pathToFile, archive);
                addonDto = null;
                return unpackedTo;
            }

            if (file.Key!.Equals("addon.json"))
            {
                addonJson = file;
            }
        }

        if (addonJson is not null)
        {
            try
            {
                using var stream = addonJson.OpenEntryStream();

                addonDto = JsonSerializer.Deserialize(
                    stream,
                    AddonManifestContext.Default.AddonDto
                    )!;

                if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
                {
                    //need to unpack addons that contain custom RFF files
                    var unpackedTo = Unpack(pathToFile, archive);
                    return unpackedTo;
                }

                if (addonDto.Executables is not null)
                {
                    //need to unpack addons with custom executables
                    var unpackedTo = Unpack(pathToFile, archive);
                    return unpackedTo;
                }

                return null;
            }
            catch
            {
                addonDto = null;
                return null;
            }
        }

        addonDto = null;
        return null;
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

        File.Delete(pathToFile);

        return unpackTo;
    }
}
