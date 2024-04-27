using Common.Config;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Helpers;
using Mods.Mods;
using Mods.Serializable;
using SharpCompress.Archives;
using System.Text.Json;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of installed mods
    /// </summary>
    public sealed class InstalledModsProvider : IInstalledModsProvider
    {
        private readonly IGame _game;
        private readonly ConfigEntity _config;
        private readonly Dictionary<AddonTypeEnum, Dictionary<string, IAddon>> _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        public event ModChanged ModDeletedEvent;

        [Obsolete($"Don't create directly. Use {nameof(InstalledModsProvider)}.")]
        public InstalledModsProvider(
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
                    var camps = GetModsFromFiles(AddonTypeEnum.TC, files);
                    _cache.Add(AddonTypeEnum.TC, camps);

                    files = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
                    var maps = GetModsFromFiles(AddonTypeEnum.Map, files);
                    _cache.Add(AddonTypeEnum.Map, maps);

                    files = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                    var mods = GetModsFromFiles(AddonTypeEnum.Mod, files);
                    _cache.Add(AddonTypeEnum.Mod, mods);
                });
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }

        /// <inheritdoc/>
        public void AddMod(AddonTypeEnum modTypeEnum, string pathToFile)
        {
            _cache.ThrowIfNull();

            var mod = GetMod(modTypeEnum, pathToFile);

            mod.ThrowIfNull();

            if (!_cache.TryGetValue(mod.Type, out _))
            {
                _cache.Add(mod.Type, []);
            }

            var dict = _cache[mod.Type];

            if (dict.TryGetValue(mod.Id, out _))
            {
                dict[mod.Id] = mod;
            }
            else
            {
                dict.Add(mod.Id, mod);
            }
        }

        /// <inheritdoc/>
        public void DeleteMod(IAddon mod)
        {
            _cache.ThrowIfNull();

            File.Delete(mod.PathToFile);

            _cache[mod.Type].Remove(mod.Id);

            ModDeletedEvent?.Invoke(_game, mod.Type);
        }

        /// <inheritdoc/>
        public void EnableMod(string id) => ((AutoloadMod)_cache[AddonTypeEnum.Mod][id]).IsEnabled = true;

        /// <inheritdoc/>
        public void DisableMod(string id) => ((AutoloadMod)_cache[AddonTypeEnum.Mod][id]).IsEnabled = false;

        /// <inheritdoc/>
        public Dictionary<string, IAddon> GetInstalledMods(AddonTypeEnum modTypeEnum)
        {
            _cache.ThrowIfNull();

            _cache.TryGetValue(modTypeEnum, out var result);

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
        /// Get mods from list of files
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="files">Paths to mod files</param>
        private Dictionary<string, IAddon> GetModsFromFiles(AddonTypeEnum modTypeEnum, IEnumerable<string> files)
        {
            Dictionary<string, IAddon> addedMods = new(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                try
                {
                    var newMod = GetMod(modTypeEnum, file);

                    if (newMod is null)
                    {
                        continue;
                    }

                    if (addedMods.TryGetValue(newMod.Id, out var existingMod))
                    {
                        if (existingMod.Version is null &&
                            newMod.Version is not null)
                        {
                            //replacing with mod that have version
                            addedMods[newMod.Id] = newMod;
                        }
                        else if (existingMod.Version is not null &&
                                 newMod.Version is not null &&
                                 VersionComparer.Compare(newMod.Version, existingMod.Version, ">"))
                        {
                            //replacing with mod that have higher version
                            addedMods[newMod.Id] = newMod;
                        }
                    }
                    else
                    {
                        addedMods.Add(newMod.Id, newMod);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return addedMods;
        }

        /// <summary>
        /// Get mod from a file
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="pathToFile">Path to mod file</param>
        private Addon? GetMod(AddonTypeEnum modTypeEnum, string pathToFile)
        {
            var type = modTypeEnum;
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
            HashSet<FeaturesEnum>? requiredFeatures = null;

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
                            if (dep.Id == DukeAddonEnum.DukeDC.ToString())
                            {
                                dukeAddon = DukeAddonEnum.DukeDC;
                            }
                            else if (dep.Id == DukeAddonEnum.DukeNW.ToString())
                            {
                                dukeAddon = DukeAddonEnum.DukeNW;
                            }
                            else if (dep.Id == DukeAddonEnum.DukeVaca.ToString())
                            {
                                dukeAddon = DukeAddonEnum.DukeVaca;
                            }
                            else if (dep.Id == DukeAddonEnum.DukeWT.ToString())
                            {
                                dukeAddon = DukeAddonEnum.DukeWT;
                            }

                            else if (dep.Id == WangAddonEnum.WangTD.ToString())
                            {
                                wangAddon = WangAddonEnum.WangTD;
                            }
                            else if (dep.Id.Equals(WangAddonEnum.WangWD.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                wangAddon = WangAddonEnum.WangWD;
                            }

                            else if (dep.Id == RedneckAddonEnum.RedneckR66.ToString())
                            {
                                redneckAddon = RedneckAddonEnum.RedneckR66;
                            }

                            else if (dep.Id.Equals(BloodAddonEnum.BloodCP.ToString(), StringComparison.OrdinalIgnoreCase))
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

            if (modTypeEnum is AddonTypeEnum.Mod)
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
                    SupportedGamesCrcs = supportedGamesCrcs,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainCon = mainCon,
                        AdditionalCons = addCons,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
                        RTS = rts,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
                        Dependencies = dependencies,
                        Incompatibles = incompatibles,
                        StartMap = startMap,
                        MainCon = mainCon,
                        AdditionalCons = addCons,
                        MainDef = mainDef,
                        AdditionalDefs = addDefs,
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
                        SupportedGamesCrcs = supportedGamesCrcs,
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
