using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// EDuke32 port
    /// </summary>
    public class EDuke32 : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.EDuke32;

        /// <inheritdoc/>
        public override string Exe => "eduke32.exe";

        /// <inheritdoc/>
        public override string Name => "EDuke32";

        /// <inheritdoc/>
        public override string ConfigFile => "eduke32.cfg";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://dukeworld.com/eduke32/synthesis/latest/");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => ThrowHelper.NotImplementedException<Func<GitHubReleaseAsset, bool>>();

        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IMod campaign)
        {
            FixGrpInConfig();

            FixRoute66Files(game, campaign);
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            mod.ThrowIfNotType<DukeCampaign>(out var dukeCamp);
            game.ThrowIfNotType<DukeGame>(out var dukeGame);

            sb.Append($@" -usecwd -nosetup");

            if (dukeCamp.AddonEnum is DukeAddonEnum.WorldTour)
            {
                sb.Append($@" -addon {(byte)DukeAddonEnum.Duke3D} -j ""{dukeGame.DukeWTInstallPath}"" -j ""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
                return;
            }

            sb.Append($@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum}");

            if (dukeCamp.FileName is null)
            {
                return;
            }

            if (dukeCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, dukeCamp.FileName)}"" -con ""{dukeCamp.StartupFile}""");
            }
            else if (dukeCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, dukeCamp.FileName)}"" -map ""{dukeCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {dukeCamp.ModType} is not supported");
                return;
            }
        }

        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IMod campaign, Dictionary<Guid, IMod> mods)
        {
            if (mods.Count == 0)
            {
                return;
            }

            sb.Append($@" -j ""{game.ModsFolderPath}""");

            foreach (var mod in mods)
            {
                mod.Value.ThrowIfNotType<AutoloadMod>(out var autoloadMod);

                if (!autoloadMod.IsEnabled)
                {
                    //skipping disabled mods
                    continue;
                }

                if (!autoloadMod.SupportedPorts?.Contains(PortEnum) ?? false)
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

                sb.Append($@" -g ""{mod.Value.FileName}""");
            }

            sb.Append($@" -j ""{Path.Combine(game.SpecialFolderPath, Consts.CombinedModFolder)}"" -mh ""{Consts.CombinedDef}""");
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        public override int? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(FolderPath, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return int.Parse(File.ReadAllText(versionFile));
            }
        }

        /// <summary>
        /// Remove GRP files from the config
        /// </summary>
        private void FixGrpInConfig()
        {
            var config = Path.Combine(FolderPath, ConfigFile);

            if (!File.Exists(config))
            {
                return;
            }

            var contents = File.ReadAllLines(config);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].StartsWith("SelectedGRP"))
                {
                    contents[i] = @"SelectedGRP = """"";
                    break;
                }
            }

            File.WriteAllLines(config, contents);
        }

        /// <summary>
        /// Override original art files with route 66's ones or remove overrides
        /// </summary>
        private static void FixRoute66Files(IGame game, IMod campaign)
        {
            if (game is RedneckGame && campaign is RedneckCampaign rCamp)
            {
                var tilesA1 = Path.Combine(game.GameInstallFolder, "TILESA66.ART");
                var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");

                var tilesB1 = Path.Combine(game.GameInstallFolder, "TILESB66.ART");
                var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");

                var turdMovAnm1 = Path.Combine(game.GameInstallFolder, "TURD66.ANM");
                var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");

                var turdMovVoc1 = Path.Combine(game.GameInstallFolder, "TURD66.VOC");
                var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");

                var endMovAnm1 = Path.Combine(game.GameInstallFolder, "END66.ANM");
                var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");

                var endMovVoc1 = Path.Combine(game.GameInstallFolder, "END66.VOC");
                var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");


                if (rCamp.AddonEnum is RedneckAddonEnum.Route66)
                {
                    File.Copy(tilesA1, tilesA2, true);
                    File.Copy(tilesB1, tilesB2, true);
                    File.Copy(turdMovAnm1, turdMovAnm2, true);
                    File.Copy(turdMovVoc1, turdMovVoc2, true);
                    File.Copy(endMovAnm1, endMovAnm2, true);
                    File.Copy(endMovVoc1, endMovVoc2, true);
                }
                else
                {
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
        }
    }
}
