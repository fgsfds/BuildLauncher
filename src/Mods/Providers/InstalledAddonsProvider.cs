using ClientCommon.Config;
using ClientCommon.Providers;
using Common.Enums;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using Mods.Serializable;
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
    private readonly PlaytimeProvider _playtimeProvider;

    private readonly Dictionary<AddonTypeEnum, Dictionary<string, IAddon>> _cache;
    private static readonly SemaphoreSlim _semaphore = new(1);

    private bool _isCacheUpdating = false;

    public event AddonChanged AddonsChangedEvent;

    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProvider)}.")]
    public InstalledAddonsProvider(
        IGame game,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider
        )
    {
        _game = game;
        _config = config;
        _playtimeProvider = playtimeProvider;
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
            }).WaitAsync(CancellationToken.None);
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

        if (dict.TryGetValue(addon.Id, out _))
        {
            dict[addon.Id] = addon;
        }
        else
        {
            dict.Add(addon.Id, addon);
        }
    }

    /// <inheritdoc/>
    public void DeleteAddon(IAddon addon)
    {
        _cache.ThrowIfNull();
        addon.PathToFile.ThrowIfNull();

        File.Delete(addon.PathToFile);

        _cache[addon.Type].Remove(addon.Id);

        AddonsChangedEvent?.Invoke(_game, addon.Type);
    }

    /// <inheritdoc/>
    public void EnableAddon(string addonId)
    {
        ((AutoloadMod)_cache[AddonTypeEnum.Mod][addonId]).IsEnabled = true;

        _config.ChangeModState(addonId, true);
    }

    /// <inheritdoc/>
    public void DisableAddon(string addonId)
    {
        ((AutoloadMod)_cache[AddonTypeEnum.Mod][addonId]).IsEnabled = false;

        _config.ChangeModState(addonId, false);
    }

    /// <inheritdoc/>
    public Dictionary<string, IAddon> GetInstalledAddons(AddonTypeEnum addonType)
    {
        if (_isCacheUpdating)
        {
            return [];
        }

        _cache.ThrowIfNull();

        _cache.TryGetValue(addonType, out var result);

        if (result is not null)
        {
            return result;
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Get addons from list of files
    /// </summary>
    /// <param name="addonType">Addon type</param>
    /// <param name="files">Paths to addon files</param>
    private Dictionary<string, IAddon> GetAddonsFromFiles(AddonTypeEnum addonType, IEnumerable<string> files)
    {
        Dictionary<string, IAddon> addedAddons = new(files.Count(), StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                var newAddon = GetAddonFromFile(addonType, file);

                if (newAddon is null)
                {
                    continue;
                }

                if (addedAddons.TryGetValue(newAddon.Id, out var existingMod))
                {
                    if (existingMod.Version is null &&
                        newAddon.Version is not null)
                    {
                        //replacing with addon that have version
                        addedAddons[newAddon.Id] = newAddon;
                    }
                    else if (existingMod.Version is not null &&
                             newAddon.Version is not null &&
                             VersionComparer.Compare(newAddon.Version, existingMod.Version, ">"))
                    {
                        //replacing with addon that have higher version
                        addedAddons[newAddon.Id] = newAddon;
                    }
                }
                else
                {
                    addedAddons.Add(newAddon.Id, newAddon);
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

        if (ArchiveFactory.IsArchive(pathToFile, out var _))
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
            if (manifest.Description is string desc)
            {
                description = desc;
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

            preview = ImageHelper.GetImageFromArchive(archive, "eduke32_preview.png");
            image = ImageHelper.GetCoverFromArchive(archive) ?? preview;

            dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version);
            incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version);

            if (manifest.SupportedGame.Version is not null)
            {
                if (manifest.SupportedGame.Version == DukeVersionEnum.Duke3D_13D.ToString())
                {
                }
                else if (manifest.SupportedGame.Version == DukeVersionEnum.Duke3D_Atomic.ToString())
                {
                }
                else if (manifest.SupportedGame.Version == DukeVersionEnum.Duke3D_WT.ToString())
                {
                }
            }
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            //TODO loose maps
        }
        else if (pathToFile.EndsWith(".grp", StringComparison.OrdinalIgnoreCase))
        {
            //"real" grps are not supported
            return null;
        }

        Addon? addon = null;

        if (addonType is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(id);

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
                MainDef = mainDef,
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
                    Title = title!,
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
        var oldManifest = archive.Entries.FirstOrDefault(static x => x.Key.Equals("manifest.json", StringComparison.OrdinalIgnoreCase));

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
