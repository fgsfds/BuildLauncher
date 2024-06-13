using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
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
        protected override string ConfigFile => "eduke32.cfg";

        /// <inheritdoc/>
        protected override string AddGrpParam => "-grp ";

        /// <inheritdoc/>
        protected override string AddDirectoryParam => "-j ";

        /// <inheritdoc/>
        protected override string AddFileParam => "-g ";

        /// <inheritdoc/>
        protected override string AddDefParam => "-mh ";

        /// <inheritdoc/>
        protected override string AddConParam => "-mx ";

        /// <inheritdoc/>
        protected override string MainDefParam => "-h ";

        /// <inheritdoc/>
        protected override string MainConParam => "-x ";

        /// <inheritdoc/>
        protected override string SkillParam => "-s";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override List<string> SupportedGamesVersions =>
            [
            nameof(DukeVersionEnum.Duke3D_13D),
            nameof(DukeVersionEnum.Duke3D_Atomic),
            nameof(DukeVersionEnum.Duke3D_WT)
            ];

        /// <inheritdoc/>
        public override List<FeatureEnum> SupportedFeatures => [FeatureEnum.EDuke32_CON];

        /// <inheritdoc/>
        public override string? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToExecutableFolder, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return File.ReadAllText(versionFile);
            }
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
        {
            sb.Append($@" -usecwd"); //don't search for steam/gog installs
            sb.Append($@" -cachesize 262144"); //set cache to 256MiB

            if (addon.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{addon.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }

            if (addon.AdditionalDefs is not null)
            {
                foreach (var def in addon.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }


            if (game is DukeGame dGame)
            {
                GetDukeArgs(sb, dGame, addon);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
            }
        }


        /// <summary>
        /// Get startup agrs for Duke
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">DukeGame</param>
        /// <param name="camp">DukeCampaign</param>
        protected void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon addon)
        {
            if (addon.SupportedGame.GameEnum is GameEnum.Duke64)
            {
                sb.Append(@$" {AddDirectoryParam}""{Path.GetDirectoryName(game.Duke64RomPath)}"" -gamegrp ""{Path.GetFileName(game.Duke64RomPath)}""");
                return;
            }

            if (addon.SupportedGame.GameVersion is not null &&
                addon.SupportedGame.GameVersion.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.InvariantCultureIgnoreCase))
            {
                sb.Append($@" {AddDirectoryParam}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Base} {AddDirectoryParam}""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
            }
            else
            {
                byte dukeAddon = (byte)DukeAddonEnum.Base;

                if (addon.DependentAddons is null)
                {
                    dukeAddon = (byte)DukeAddonEnum.Base;
                }
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeDC)))
                {
                    dukeAddon = (byte)DukeAddonEnum.DukeDC;
                }
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeNW)))
                {
                    dukeAddon = (byte)DukeAddonEnum.DukeNW;
                }
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
                {
                    dukeAddon = (byte)DukeAddonEnum.DukeVaca;
                }

                sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -addon {dukeAddon}");
            }


            if (addon.FileName is null)
            {
                return;
            }

            if (addon is LooseMap)
            {
                GetLooseMapArgs(sb, game, addon);
                return;
            }

            if (addon is not DukeCampaign dCamp)
            {
                ThrowHelper.ArgumentException(nameof(addon));
                return;
            }

            if (dCamp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, dCamp.FileName)}""");
            }

            if (dCamp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, dCamp);
            }

            if (dCamp.MainCon is not null)
            {
                sb.Append($@" {MainConParam}""{dCamp.MainCon}""");
            }

            if (dCamp.AdditionalCons?.Count > 0)
            {
                foreach (var con in dCamp.AdditionalCons)
                {
                    sb.Append($@" {AddConParam}""{con}""");
                }
            }
        }


        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon addon, Dictionary<AddonVersion, IAddon> mods)
        {
            if (mods.Count == 0)
            {
                return;
            }

            sb.Append($@" {AddDirectoryParam}""{game.ModsFolderPath}""");

            foreach (var mod in mods)
            {
                if (mod.Value is not AutoloadMod aMod)
                {
                    continue;
                }

                if (!ValidateAutoloadMod(aMod, addon, mods))
                {
                    continue;
                }

                sb.Append($@" {AddFileParam}""{aMod.FileName}""");

                if (aMod.AdditionalDefs is not null)
                {
                    foreach (var def in aMod.AdditionalDefs)
                    {
                        sb.Append($@" {AddDefParam}""{def}""");
                    }
                }

                if (aMod.AdditionalCons is not null)
                {
                    foreach (var con in aMod.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
        }


        /// <summary>
        /// Remove GRP files from the config
        /// </summary>
        protected void FixGrpInConfig()
        {
            var config = Path.Combine(PathToExecutableFolder, ConfigFile);

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
    }
}
