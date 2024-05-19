using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Common.Releases;
using Games.Games;
using System.Text;

namespace Ports.Ports
{
    /// <summary>
    /// BuildGDX port
    /// </summary>
    public sealed class BuildGDX : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.BuildGDX;

        /// <inheritdoc/>
        public override string Exe => Path.Combine("jre", "bin", "javaw.exe");

        /// <inheritdoc/>
        public override string Name => "BuildGDX";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood,
            GameEnum.Duke3D,
            GameEnum.ShadowWarrior,
            GameEnum.Exhumed,
            GameEnum.Redneck,
            GameEnum.RidesAgain,
            GameEnum.NAM,
            GameEnum.WWIIGI,
            GameEnum.Witchaven,
            GameEnum.Witchaven2,
            GameEnum.TekWar
            ];

        /// <inheritdoc/>
        public override string? InstalledVersion => IsInstalled ? "1.16" : null;

        /// <inheritdoc/>
        public override bool IsInstalled => File.Exists(Path.Combine(PathToExecutableFolder, "BuildGDX.jar"));


        /// <inheritdoc/>
        protected override string ConfigFile => string.Empty;

        /// <inheritdoc/>
        protected override string AddDirectoryParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddFileParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddDefParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddConParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string MainDefParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string MainConParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddGrpParam => throw new NotImplementedException();


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
        {
            sb.Append(@" -jar ..\..\BuildGDX.jar");

            if (game is BloodGame bGame)
            {
                GetBloodArgs(sb, bGame, addon);
            }
            else if (game is DukeGame dGame)
            {
                GetDukeArgs(sb, dGame, addon);
            }
            else if (game is WangGame wGame)
            {
                GetWangArgs(sb, wGame, addon);
            }
            else if (game is SlaveGame sGame)
            {
                GetSlaveArgs(sb, sGame, addon);
            }
            else if (game is RedneckGame rGame)
            {
                GetRedneckArgs(sb, rGame, addon);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {addon} for game {game} is not supported");
            }
        }

        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame _, IAddon campaign) { }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) { }

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -silent \"true\"");


        private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon camp)
        {
            if (camp.Id.Equals(nameof(DukeAddonEnum.DukeWT), StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($@" -path ""{game.DukeWTInstallPath}""");
            }
            else
            {
                sb.Append($@" -path ""{game.GameInstallFolder}""");
            }
        }

        private void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        private static void GetWangArgs(StringBuilder sb, WangGame game, IAddon camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon camp)
        {
            if (camp.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($@" -path ""{game.AgainInstallPath}""");
            }
            else
            {
                sb.Append($@" -path ""{game.GameInstallFolder}""");
            }
        }

        private static void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }
    }
}
