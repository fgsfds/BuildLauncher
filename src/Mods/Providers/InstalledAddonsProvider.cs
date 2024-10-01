﻿using Common;
using Common.Client.Config;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using Mods.Serializable;
using Mods.Serializable.Addon;
using SharpCompress.Archives;
using System.Text.Json;

namespace Mods.Providers;

/// <summary>
/// Class that provides lists of installed mods
/// </summary>
public sealed class InstalledAddonsProvider : IInstalledAddonsProvider
{
    private readonly IGame _game;
    private readonly IConfigProvider _config;

    private readonly Dictionary<AddonTypeEnum, Dictionary<AddonVersion, IAddon>> _cache;
    private readonly IEnumerable<string> _portExes = ["nblood.exe", "notblood.exe", "eduke32.exe"];

    private static readonly SemaphoreSlim _semaphore = new(1);

    private bool _isCacheUpdating = false;

    public event AddonChanged AddonsChangedEvent;

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

                    var tcs = await GetAddonsFromFilesAsync(AddonTypeEnum.TC, filesTcs).ConfigureAwait(false);
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
                    var maps = await GetAddonsFromFilesAsync(AddonTypeEnum.Map, filesMaps).ConfigureAwait(false);
                    _cache.Add(AddonTypeEnum.Map, maps);


                    //mods
                    var filesMods = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = await GetAddonsFromFilesAsync(AddonTypeEnum.Mod, filesMods).ConfigureAwait(false);
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
            _cache.ThrowIfNull();
        }
    }

    /// <inheritdoc/>
    public async Task AddAddonAsync(AddonTypeEnum addonType, string pathToFile)
    {
        _cache.ThrowIfNull();

        var addon = await GetAddonFromFileAsync(addonType, pathToFile).ConfigureAwait(false);

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
    }

    /// <inheritdoc/>
    public void DeleteAddon(IAddon addon)
    {
        _cache.ThrowIfNull();
        addon.PathToFile.ThrowIfNull();

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
            _ => throw new NotImplementedException()
        };
    }

    /// <inheritdoc/>
    public Dictionary<AddonVersion, IAddon> GetInstalledCampaigns()
    {
        var campaigns = _game.GetOriginalCampaigns();

        if (_isCacheUpdating)
        {
            return campaigns;
        }

        _cache.ThrowIfNull();

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

        _cache.ThrowIfNull();

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

        _cache.ThrowIfNull();

        _ = _cache.TryGetValue(AddonTypeEnum.Mod, out var result);

        return result is null ? [] : result;
    }

    /// <summary>
    /// Get addons from list of files
    /// </summary>
    /// <param name="addonType">Addon type</param>
    /// <param name="files">Paths to addon files</param>
    private async Task<Dictionary<AddonVersion, IAddon>> GetAddonsFromFilesAsync(AddonTypeEnum addonType, IEnumerable<string> files)
    {
        Dictionary<AddonVersion, IAddon> addedAddons = [];

        foreach (var file in files)
        {
            try
            {
                var newAddon = await GetAddonFromFileAsync(addonType, file).ConfigureAwait(false);

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
    /// Get addon from a file
    /// </summary>
    /// <param name="addonType">Addon type</param>
    /// <param name="pathToFile">Path to addon file</param>
    private async Task<BaseAddon?> GetAddonFromFileAsync(AddonTypeEnum addonType, string pathToFile)
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
        string? portOverride;

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

            var portExe = from file in Directory.GetFiles(addonDir)
                          from exe in _portExes
                          where file.EndsWith(exe)
                          select file;

            portOverride = portExe.Any() ? portExe.First() : null;
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

                var portExe = from file in Directory.GetFiles(addonDir)
                              from exe in _portExes
                              where file.EndsWith(exe)
                              select file;

                portOverride = portExe.Any() ? portExe.First() : null;
            }
            else
            {
                isUnpacked = false;
                portOverride = null;

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
                PortExeOverride = null
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


        if (addonType is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(id);

            if (mainDef is not null)
            {
                ThrowHelper.ArgumentException("Autoload mod can't have Main DEF");
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
                PortExeOverride = null
            };

            return addon;
        }
        else
        {
            if (_game.GameEnum is GameEnum.Duke3D)
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
                    PortExeOverride = portOverride
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.Fury)
            {
                var addon = new FuryCampaign()
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
                    RequiredFeatures = requiredFeatures,
                    PreviewImage = preview,
                    IsFolder = isUnpacked,
                    PortExeOverride = portOverride
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
                    PortExeOverride = portOverride
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
                    PortExeOverride = portOverride
                };

                return addon;
            }
            else if (_game.GameEnum is GameEnum.Redneck)
            {
                var addon = new RedneckCampaign()
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
                    PortExeOverride = portOverride
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
                    PortExeOverride = portOverride
                };

                return addon;
            }
            else
            {
                ThrowHelper.NotImplementedException();
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
                var unpackedTo = Unpack(pathToFile, archive);
                addonDto = null;
                return unpackedTo;
            }

            if (file.Key!.Equals("addon.json"))
            {
                addonJson = file;
            }
            else if (_portExes.Any(x => file.Key!.EndsWith(x)))
            {
                var unpackedTo = Unpack(pathToFile, archive);
                var addonJsonStr = Path.Combine(unpackedTo, "addon.json");

                if (File.Exists(addonJsonStr))
                {
                    try
                    {
                        addonDto = JsonSerializer.Deserialize(
                            File.ReadAllText(addonJsonStr),
                            AddonManifestContext.Default.AddonDto
                            )!;
                        return unpackedTo;
                    }
                    catch
                    {
                        addonDto = null;
                        return null;
                    }
                }
            }
        }

        if (addonJson is not null)
        {
            try
            {
                addonDto = JsonSerializer.Deserialize(
                    addonJson.OpenEntryStream(),
                    AddonManifestContext.Default.AddonDto
                    )!;

                if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
                {
                    //need to unpack addons that contain custom RFF files
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
        archive.Dispose();

        File.Delete(pathToFile);

        return unpackTo;
    }
}
