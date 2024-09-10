using Common;
using Common.Client.Config;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
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
        await _semaphore.WaitAsync();
        _isCacheUpdating = true;

        if (createNew)
        {
            _cache.Clear();
        }

        if (_cache.Count == 0)
        {
            await Task.Run(() =>
            {
                IEnumerable<string> files;

                files = Directory.GetFiles(_game.CampaignsFolderPath, "*.zip");
                var camps = GetAddonsFromFiles(AddonTypeEnum.TC, files);
                _cache.Add(AddonTypeEnum.TC, camps);

                files = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
                var maps = GetAddonsFromFiles(AddonTypeEnum.Map, files);
                _cache.Add(AddonTypeEnum.Map, maps);

                files = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                var mods = GetAddonsFromFiles(AddonTypeEnum.Mod, files);
                _cache.Add(AddonTypeEnum.Mod, mods);
            }).WaitAsync(CancellationToken.None).ConfigureAwait(false);
        }

        _isCacheUpdating = false;
        AddonsChangedEvent?.Invoke(_game, null);
        _semaphore.Release();

        _cache.ThrowIfNull();
    }

    /// <inheritdoc/>
    public void AddAddon(AddonTypeEnum addonType, string pathToFile)
    {
        _cache.ThrowIfNull();

        var addon = GetAddonFromFile(addonType, pathToFile);

        addon.ThrowIfNull();

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

        File.Delete(addon.PathToFile);

        if (addon is LooseMap lMap)
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(addon.PathToFile)!, $"{Path.GetFileNameWithoutExtension(lMap.FileName)!}.*");

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        _cache[addon.Type].Remove(new(addon.Id, addon.Version));

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

        _cache.TryGetValue(AddonTypeEnum.TC, out var result);

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

        _cache.TryGetValue(AddonTypeEnum.Map, out var result);

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

        _cache.TryGetValue(AddonTypeEnum.Mod, out var result);

        return result is null ? [] : result;
    }

    /// <summary>
    /// Get addons from list of files
    /// </summary>
    /// <param name="addonType">Addon type</param>
    /// <param name="files">Paths to addon files</param>
    private Dictionary<AddonVersion, IAddon> GetAddonsFromFiles(AddonTypeEnum addonType, IEnumerable<string> files)
    {
        Dictionary<AddonVersion, IAddon> addedAddons = [];

        foreach (var file in files)
        {
            try
            {
                var newAddon = GetAddonFromFile(addonType, file);

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
    private Addon? GetAddonFromFile(AddonTypeEnum addonType, string pathToFile)
    {
        var type = addonType;
        var id = Path.GetFileName(pathToFile);
        var title = Path.GetFileName(pathToFile);
        string version = new("1.0");

        string? author = null;
        string? description = null;
        Stream? image = null;
        Stream? preview = null;

        var supportedGame = _game.GameEnum;
        string? gameVersion = null;
        string? gameCrc = null;

        HashSet<FeatureEnum>? requiredFeatures = null;

        string? mainCon = null;
        HashSet<string>? addCons = null;
        string? mainDef = null;
        HashSet<string>? addDefs = null;

        string? rts = null;
        string? ini = null;
        string? rff = null;
        string? snd = null;

        Dictionary<string, string?>? dependencies = null;
        Dictionary<string, string?>? incompatibles = null;
        IStartMap? startMap = null;

        Addon? addon;

        if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.Open(pathToFile);

            if (DeleteOld(pathToFile, archive))
            {
                return null;
            }

            var entry = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("addon.json"));

            if (entry is null)
            {
                //TODO add support for non-manifested mods
                return null;
            }

            var manifest = JsonSerializer.Deserialize(
                entry.OpenEntryStream(),
                AddonManifestContext.Default.AddonDto
                );

            if (manifest is null)
            {
                return null;
            }

            type = manifest.AddonType;
            id = manifest.Id;
            title = manifest.Title;
            author = manifest.Author;
            version = manifest.Version;

            //TODO description
            if (manifest.Description is not null)
            {
                description = manifest.Description;
            }

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

            preview = ImageHelper.GetImageFromArchive(archive, "preview.png");
            image = ImageHelper.GetCoverFromArchive(archive) ?? preview;

            dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
            incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

            //TODO Duke versions
            if (manifest.SupportedGame.Version is not null)
            {
                if (manifest.SupportedGame.Version == nameof(DukeVersionEnum.Duke3D_13D))
                {
                }
                else if (manifest.SupportedGame.Version == nameof(DukeVersionEnum.Duke3D_Atomic))
                {
                }
                else if (manifest.SupportedGame.Version == nameof(DukeVersionEnum.Duke3D_WT))
                {
                }
            }
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            var bloodIni = pathToFile.Replace(".map", ".ini", StringComparison.InvariantCultureIgnoreCase);
            var iniExists = File.Exists(bloodIni);

            addon = new LooseMap()
            {
                Id = Path.GetFileName(pathToFile),
                Type = AddonTypeEnum.Map,
                Title = Path.GetFileName(pathToFile),
                SupportedGame = new(supportedGame, gameVersion, gameCrc),
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
            };

            return addon;
        }
        else if (pathToFile.EndsWith(".grp", StringComparison.OrdinalIgnoreCase))
        {
            //"real" grps are not supported
        }

        if (addonType is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(id);

            if (mainDef is not null)
            {
                ThrowHelper.ArgumentException("Autoload mod can't have Main DEF");
            }

            addon = new AutoloadMod()
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
            };
        }
        else
        {
            if (_game.GameEnum is GameEnum.Duke3D)
            {
                addon = new DukeCampaign()
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
                    PreviewImage = preview
                };
            }
            else if (_game.GameEnum is GameEnum.Fury)
            {
                addon = new FuryCampaign()
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
                };
            }
            else if (_game.GameEnum is GameEnum.ShadowWarrior)
            {
                addon = new WangCampaign()
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
                };
            }
            else if (_game.GameEnum is GameEnum.Blood)
            {
                addon = new BloodCampaign()
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
                };
            }
            else if (_game.GameEnum is GameEnum.Redneck)
            {
                addon = new RedneckCampaign()
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
                };
            }
            else if (_game.GameEnum is GameEnum.Exhumed)
            {
                addon = new SlaveCampaign()
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
                };
            }
            else
            {
                ThrowHelper.NotImplementedException();
                return null;
            }
        }

        return addon;
    }

    [Obsolete("delete")]
    private bool DeleteOld(string pathToFile, IArchive archive)
    {
        var oldManifest = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("manifest.json", StringComparison.OrdinalIgnoreCase));

        if (oldManifest is not null)
        {
            //deleting old versions of the mods
            archive.Dispose();
            File.Delete(pathToFile);
            return true;
        }

        return false;
    }
}
