using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Diagnostics;
using System.Text;

namespace Ports.Ports
{
    /// <summary>
    /// Raze port
    /// </summary>
    public sealed class Raze : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Raze;

        /// <inheritdoc/>
        public override string Exe => "raze.exe";

        /// <inheritdoc/>
        public override string Name => "Raze";

        /// <inheritdoc/>
        public override string ConfigFile => "raze_portable.ini";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood,
            GameEnum.Duke3D,
            GameEnum.Wang,
            GameEnum.Slave,
            GameEnum.Redneck,
            GameEnum.Again,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/ZDoom/Raze/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith(".zip") && !x.FileName.Contains("linux") && !x.FileName.Contains("macos");

        /// <inheritdoc/>
        public override int? InstalledVersion =>
            File.Exists(FullPathToExe)
            ? int.Parse(new string(FileVersionInfo.GetVersionInfo(FullPathToExe).FileVersion!.Where(static x => char.IsDigit(x)).ToArray()))
            : null;

        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IMod campaign)
        {
            var config = Path.Combine(FolderPath, ConfigFile);

            if (!File.Exists(config))
            {
                //creating default config if it doesn't exist
                var text = """
                    [GameSearch.Directories]
                    Path=.

                    [FileSearch.Directories]
                    Path=.
                    Path=.

                    [SoundfontSearch.Directories]
                    Path=$PROGDIR/soundfonts

                    [Exhumed.AutoExec]
                    Path=$PROGDIR/autoexec.cfg

                    [GlobalSettings]
                    gl_texture_filter=6
                    snd_alresampler=Nearest
                    gl_tonemap=5
                    hw_useindexedcolortextures=true
                    """;

                if (!Directory.Exists(Path.GetDirectoryName(config)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(config));
                }

                File.WriteAllText(config, text);
            }

            AddGamePathsToConfig(game.GameInstallFolder, game.ModsFolderPath, config);

            FixRoute66Files(game, campaign);
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            sb.Append($@" -nosetup -savedir ""{Path.Combine(FolderPath, "Save", mod.DisplayName.Replace(' ', '_'))}""");

            if (game is BloodGame bGame && mod is BloodCampaign bMod)
            {
                GetBloodArgs(sb, bGame, bMod);
            }
            else if (game is DukeGame dGame && mod is DukeCampaign dMod)
            {
                GetDukeArgs(sb, dGame, dMod);
            }
            else if (game is WangGame wGame && mod is WangCampaign wMod)
            {
                GetWangArgs(sb, wGame, wMod);
            }
            else if (game is SlaveGame sGame && mod is SlaveCampaign sMod)
            {
                GetSlaveArgs(sb, sGame, sMod);
            }
            else if (game is RedneckGame rGame && mod is RedneckCampaign rMod)
            {
                GetRedneckArgs(sb, rGame, rMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
            }
        }

        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame _, IMod campaign, IEnumerable<IMod> mods)
        {
            if (!mods.Any())
            {
                return;
            }

            foreach (var mod in mods)
            {
                mod.ThrowIfNotType<AutoloadMod>(out var autoloadMod);

                if (!autoloadMod.IsEnabled)
                {
                    //skipping disabled mods
                    continue;
                }

                if (autoloadMod.SupportedPorts is not null &&
                    !autoloadMod.SupportedPorts.Contains(PortEnum))
                {
                    //skipping mods not supported by the current port
                    continue;
                }

                if (campaign.Addon is not null &&
                    autoloadMod.SupportedAddons is not null &&
                    !autoloadMod.SupportedAddons.Contains(campaign.Addon))
                {
                    //skipping mods not supported by the current addon
                    continue;
                }

                sb.Append($@" -file ""{mod.FileName}""");
            }
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        private void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign camp)
        {
            sb.Append($" -addon {(byte)camp.AddonEnum}");

            if (camp.AddonEnum is DukeAddonEnum.WorldTour)
            {
                var config = Path.Combine(FolderPath, ConfigFile);
                AddGamePathsToConfig(game.DukeWTInstallPath, game.ModsFolderPath, config);

                return;
            }

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}"" -con ""{camp.StartupFile}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }

        private void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign camp)
        {
            var ini = camp.StartupFile;

            if (camp.ModType is ModTypeEnum.Map)
            {
                if (camp.AddonEnum is BloodAddonEnum.Blood)
                {
                    ini = Consts.BloodIni;
                }
                else if (camp.AddonEnum is BloodAddonEnum.Cryptic)
                {
                    ini = Consts.CrypticIni;
                }
            }

            //sb.Append(@$" -iwad BLOOD.RFF -ini ""{ini}""");
            sb.Append(@$" -ini ""{ini}""");

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }

        private static void GetWangArgs(StringBuilder sb, WangGame game, WangCampaign camp)
        {
            //sb.Append(@" -iwad SW.GRP");

            if (camp.FileName is null)
            {
                if (camp.AddonEnum is WangAddonEnum.Wanton)
                {
                    sb.Append(@" -file WT.GRP");
                }
                else if (camp.AddonEnum is WangAddonEnum.TwinDragon)
                {
                    sb.Append(@" -file TD.GRP");
                }

                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            if (camp.AddonEnum is RedneckAddonEnum.Route66)
            {
                sb.Append(" -route66");
                return;
            }

            if (camp.AddonEnum is RedneckAddonEnum.Again)
            {
                var config = Path.Combine(FolderPath, ConfigFile);
                AddGamePathsToConfig(game.AgainInstallPath, game.ModsFolderPath, config);
            }

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }

        private static void GetSlaveArgs(StringBuilder sb, SlaveGame game, SlaveCampaign camp)
        {
            if (camp.FileName is null)
            {
                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }

        /// <summary>
        /// Remove route 66 art files overrides
        /// </summary>
        private static void FixRoute66Files(IGame game, IMod campaign)
        {
            if (game is RedneckGame && campaign is RedneckCampaign rCamp)
            {
                var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");
                var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");
                var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");
                var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");
                var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");
                var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");

                if (File.Exists(tilesA2))
                {
                    File.Delete(tilesA2);
                }

                if (File.Exists(tilesB2))
                {
                    File.Delete(tilesB2);
                }

                if (File.Exists(turdMovAnm2))
                {
                    File.Delete(turdMovAnm2);
                }

                if (File.Exists(turdMovVoc2))
                {
                    File.Delete(turdMovVoc2);
                }

                if (File.Exists(endMovAnm2))
                {
                    File.Delete(endMovAnm2);
                }

                if (File.Exists(endMovVoc2))
                {
                    File.Delete(endMovVoc2);
                }
            }
        }

        /// <summary>
        /// Add paths to game and mods folder to the config
        /// </summary>
        [Obsolete("Remove if this ever implemented https://github.com/ZDoom/Raze/issues/1060")]
        private static void AddGamePathsToConfig(string gameFolder, string modsFolder, string config)
        {
            var contents = File.ReadAllLines(config);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].Equals("[GameSearch.Directories]"))
                {
                    var path = gameFolder.Replace('\\', '/');

                    contents[i + 1] = $"Path={path}";
                    continue;
                }
                if (contents[i].Equals("[FileSearch.Directories]"))
                {
                    var path = modsFolder.Replace('\\', '/');

                    contents[i + 1] = $"Path={path}";

                    path = gameFolder.Replace('\\', '/');

                    contents[i + 2] = $"Path={path}";

                    break;
                }
            }

            File.WriteAllLines(config, contents);
        }
    }
}
