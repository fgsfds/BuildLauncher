using Common.Config;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Helpers;
using Mods.Addons;
using Mods.Serializable;
using SharpCompress.Archives;
using System.Text.Json;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of installed mods
    /// </summary>
    public sealed class InstalledAddonsProvider : IInstalledAddonsProvider
    {
        private readonly IGame _game;
        private readonly ConfigEntity _config;
        private readonly Dictionary<AddonTypeEnum, Dictionary<string, IAddon>> _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        public event AddonChanged AddonDeletedEvent;

        [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProvider)}.")]
        public InstalledAddonsProvider(
            IGame game,
            ConfigEntity config
            )
        {
            _game = game;
            _config = config;
            _cache = [];
        }


        /// <inheritdoc/>
        public async Task CreateCache(bool createNew)
        {
            _semaphore.Wait();

            if (createNew)
            {
                _cache.Clear();
            }

            if (_cache.Count == 0)
            {
                await Task.Run(() =>
                {
                    IEnumerable<string> files;

                    files = Directory.GetFiles(_game.CampaignsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".grp", StringComparison.OrdinalIgnoreCase));
                    var camps = GetAddonsFromFiles(AddonTypeEnum.TC, files);
                    _cache.Add(AddonTypeEnum.TC, camps);

                    files = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
                    var maps = GetAddonsFromFiles(AddonTypeEnum.Map, files);
                    _cache.Add(AddonTypeEnum.Map, maps);

                    files = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = GetAddonsFromFiles(AddonTypeEnum.Mod, files);
                    _cache.Add(AddonTypeEnum.Mod, mods);
                });
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }

        /// <inheritdoc/>
        public void AddAddon(AddonTypeEnum addonType, string pathToFile)
        {
            _cache.ThrowIfNull();

            var addon = GetAddon(addonType, pathToFile);

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

            File.Delete(addon.PathToFile);

            _cache[addon.Type].Remove(addon.Id);

            AddonDeletedEvent?.Invoke(_game, addon.Type);
        }

        /// <inheritdoc/>
        public void EnableAddon(string id) => ((AutoloadMod)_cache[AddonTypeEnum.Mod][id]).IsEnabled = true;

        /// <inheritdoc/>
        public void DisableAddon(string id) => ((AutoloadMod)_cache[AddonTypeEnum.Mod][id]).IsEnabled = false;

        /// <inheritdoc/>
        public Dictionary<string, IAddon> GetInstalledAddon(AddonTypeEnum addonType)
        {
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
                    var newAddon = GetAddon(addonType, file);

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
                catch (Exception ex)
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
        private Addon? GetAddon(AddonTypeEnum addonType, string pathToFile)
        {
            var type = addonType;
            var id = Path.GetFileName(pathToFile);
            var title = Path.GetFileName(pathToFile);
            string? author = null;
            string? version = null;
            string? description = null;
            Stream? image = null;
            Stream? preview = null;

            HashSet<GameEnum>? supportedGames = [_game.GameEnum];
            HashSet<int>? supportedGamesCrcs = null;
            HashSet<PortEnum>? supportedPorts = null;
            HashSet<string>? grps = null;
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

            var dukeAddon = DukeAddonEnum.Duke3D;
            var bloodAddon = BloodAddonEnum.Blood;
            var wangAddon = WangAddonEnum.Wang;
            var redneckAddon = RedneckAddonEnum.Redneck;

            if (ArchiveFactory.IsArchive(pathToFile, out var _))
            {
                using var archive = ArchiveFactory.Open(pathToFile);

                if (DeleteOld(pathToFile, archive))
                {
                    return null;
                }

                var entry = archive.Entries.FirstOrDefault(static x => x.Key.Equals("addon.json"));

                if (entry is null)
                {
                    //TODO add support for non-manifested mods
                    return null;
                }

                var manifest = JsonSerializer.Deserialize(
                    entry.OpenEntryStream(),
                    AddonManifestContext.Default.AddonDto
                    );

                if (manifest is not null)
                {
                    type = manifest.Type;
                    id = manifest.Id;
                    title = manifest.Title;
                    author = manifest.Author;
                    version = manifest.Version;

                    //TODO description
                    if (manifest.Description is string desc)
                    {
                        description = desc;
                    }

                    supportedGames = manifest.SupportedGames is null ? supportedGames : [.. manifest.SupportedGames];
                    supportedGamesCrcs = manifest.SupportedGamesCrcs is null ? null : [.. manifest.SupportedGamesCrcs];
                    supportedPorts = manifest.SupportedPorts is null ? null : [.. manifest.SupportedPorts];

                    grps = manifest.Grps?.ToHashSet();
                    rts = manifest.Rts;
                    ini = manifest.Ini;
                    rff = manifest.Rff;
                    snd = manifest.Snd;
                    startMap = manifest.StartMap;
                    requiredFeatures = manifest.RequiredFeatures?.ToHashSet();

                    if (manifest.Cons is not null)
                    {
                        addCons = new(manifest.Cons.Count);

                        foreach (var con in manifest.Cons)
                        {
                            if (con.Type is ScriptTypeEnum.Main)
                            {
                                mainCon = con.PathToFile;
                            }
                            else
                            {
                                addCons.Add(con.PathToFile);
                            }
                        }
                    }

                    if (manifest.Defs is not null)
                    {
                        addDefs = new(manifest.Defs.Count);

                        foreach (var def in manifest.Defs)
                        {
                            if (def.Type is ScriptTypeEnum.Main)
                            {
                                mainDef = def.PathToFile;
                            }
                            else
                            {
                                addDefs.Add(def.PathToFile);
                            }
                        }
                    }

                    if (manifest.Dependencies is not null)
                    {
                        dependencies = new(manifest.Dependencies.Count, StringComparer.OrdinalIgnoreCase);

                        //extracting official addons from dependencies
                        foreach (var dep in manifest.Dependencies)
                        {
                            if (dep.Id == nameof(DukeAddonEnum.DukeDC))
                            {
                                dukeAddon = DukeAddonEnum.DukeDC;
                            }
                            else if (dep.Id == nameof(DukeAddonEnum.DukeNW))
                            {
                                dukeAddon = DukeAddonEnum.DukeNW;
                            }
                            else if (dep.Id == nameof(DukeAddonEnum.DukeVaca))
                            {
                                dukeAddon = DukeAddonEnum.DukeVaca;
                            }
                            else if (dep.Id == nameof(DukeAddonEnum.DukeWT))
                            {
                                dukeAddon = DukeAddonEnum.DukeWT;
                            }

                            else if (dep.Id == nameof(WangAddonEnum.WangTD))
                            {
                                wangAddon = WangAddonEnum.WangTD;
                            }
                            else if (dep.Id.Equals(nameof(WangAddonEnum.WangWD), StringComparison.OrdinalIgnoreCase))
                            {
                                wangAddon = WangAddonEnum.WangWD;
                            }

                            else if (dep.Id == nameof(RedneckAddonEnum.RedneckR66))
                            {
                                redneckAddon = RedneckAddonEnum.RedneckR66;
                            }

                            else if (dep.Id.Equals(nameof(BloodAddonEnum.BloodCP), StringComparison.OrdinalIgnoreCase))
                            {
                                bloodAddon = BloodAddonEnum.BloodCP;
                            }

                            dependencies.Add(dep.Id, dep.Version);
                        }
                    }

                    if (manifest.Incompatibles is not null)
                    {
                        incompatibles = new(manifest.Incompatibles.Count, StringComparer.OrdinalIgnoreCase);

                        foreach (var dep in manifest.Incompatibles)
                        {
                            incompatibles.Add(dep.Id, dep.Version);
                        }
                    }

                    if (manifest.PreviewImage is not null)
                    {
                        preview = ImageHelper.GetImageFromArchive(archive, manifest.PreviewImage);
                    }

                    image = ImageHelper.GetCoverFromArchive(archive) ?? preview;
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
                string? requiredAddon = null;

                if (_game.GameEnum is GameEnum.Duke3D)
                {
                    requiredAddon = dukeAddon.ToString();
                }

                addon = new AutoloadMod()
                {
                    Id = id,
                    Type = AddonTypeEnum.Mod,
                    Title = title!,
                    Image = image,
                    SupportedPorts = supportedPorts,
                    Description = description,
                    Version = version,
                    Author = author,
                    IsEnabled = isEnabled,
                    PathToFile = pathToFile,
                    MainDef = mainDef,
                    AdditionalDefs = addDefs,
                    SupportedGames = supportedGames,
                    RequiredGamesCrcs = supportedGamesCrcs,
                    Dependencies = dependencies,
                    Incompatibles = incompatibles,
                    StartMap = startMap,
                    RequiredFeatures = requiredFeatures,
                    Preview = preview
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
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainCon = mainCon,
                        AdditionalCons = addCons,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        RTS = rts,
                        GRPs = grps,
                        RequiredAddonEnum = dukeAddon,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
                    };
                }
                else if (_game.GameEnum is GameEnum.Fury)
                {
                    addon = new FuryCampaign()
                    {
                        Id = id,
                        Type = type,
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainCon = mainCon,
                        AdditionalCons = addCons,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
                    };
                }
                else if (_game.GameEnum is GameEnum.Wang)
                {
                    addon = new WangCampaign()
                    {
                        Id = id,
                        Type = type,
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        RequiredAddonEnum = wangAddon,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
                    };
                }
                else if (_game.GameEnum is GameEnum.Blood)
                {
                    addon = new BloodCampaign()
                    {
                        Id = id,
                        Type = type,
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        INI = ini,
                        RFF = rff,
                        SND = snd,
                        RequiredAddonEnum = bloodAddon,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
                    };
                }
                else if (_game.GameEnum is GameEnum.Redneck)
                {
                    addon = new RedneckCampaign()
                    {
                        Id = id,
                        Type = type,
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainCon = mainCon,
                        AdditionalCons = addCons,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        GRPs = null,
                        RTS = rts,
                        RequiredAddonEnum = redneckAddon,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
                    };
                }
                else if (_game.GameEnum is GameEnum.Exhumed)
                {
                    addon = new SlaveCampaign()
                    {
                        Id = id,
                        Type = type,
                        Title = title!,
                        Image = image,
                        SupportedPorts = supportedPorts,
                        Description = description,
                        Version = version,
                        Author = author,
                        PathToFile = pathToFile,
                        SupportedGames = supportedGames,
                        RequiredGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        RequiredFeatures = requiredFeatures,
                        Preview = preview
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
            var oldManifest = archive.Entries.FirstOrDefault(static x => x.Key.Equals("manifest.json"));

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
}
