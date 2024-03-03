using Common.Helpers;
using Common.Enums;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Collections.Immutable;
using Common.Enums.Addons;
using Common.Interfaces;

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
            GameEnum.IonFury,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://dukeworld.com/eduke32/synthesis/latest/");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void BeforeStart(IGame game)
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

        /// <inheritdoc/>
        public override string GetStartCampaignArgs(IGame game, IMod mod)
        {
            var args = $@" -usecwd -nosetup";

            if (mod is not DukeCampaign dukeCamp ||
                game is not DukeGame dukeGame)
            {
                ThrowHelper.ArgumentException();
                return null;
            }

            if (dukeCamp.AddonEnum is DukeAddonEnum.WorldTour)
            {
                return args += $@" -addon {(byte)DukeAddonEnum.Duke3D} -j""{dukeGame.DukeWTInstallPath}"" -j ""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp";
            }

            if (dukeCamp.FileName is null)
            {
                return args += $@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum}";
            }
            else
            {
                return args += $@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum} -g ""{Path.Combine(game.CampaignsFolderPath, dukeCamp.FileName)}""";
            }
        }

        /// <inheritdoc/>
        public override string GetAutoloadModsArgs(IGame game, ImmutableList<IMod> mods)
        {
            if (mods.Count == 0)
            {
                return string.Empty;
            }

            var args = $@" -j ""{game.ModsFolderPath}""";

            foreach (var mod in mods)
            {
                args += $@" -g ""{mod.FileName}""";
            }

            args += $@" -g ""{Path.Combine(game.SpecialFolderPath, "z_combined.zip")}""";

            return args;
        }

        /// <inheritdoc/>
        public override string GetSkipIntroParameter() => " -quick";

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
    }
}
