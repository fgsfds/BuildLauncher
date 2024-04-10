using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Ports.Providers;
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
            GameEnum.Wang,
            GameEnum.Slave,
            GameEnum.Redneck,
            GameEnum.RedneckRA,
            GameEnum.NAM,
            GameEnum.WWIIGI,
            GameEnum.Witchaven,
            GameEnum.Witchaven2,
            GameEnum.TekWar
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://github.com/fgsfds/Build-Mods-Repo/raw/master/Ports/BuildGDX_v116.zip");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => throw new NotImplementedException();

        /// <inheritdoc/>
        public override int? InstalledVersion => IsInstalled ? 116 : null;

        /// <inheritdoc/>
        public override bool IsInstalled => File.Exists(Path.Combine(PathToPortFolder, "BuildGDX.jar"));


        /// <inheritdoc/>
        protected override string ConfigFile => string.Empty;

        /// <inheritdoc/>
        protected override string AddDirectoryParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddFileParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddDefParam => throw new NotImplementedException();

        protected override string AddConParam => throw new NotImplementedException();

        protected override string MainDefParam => throw new NotImplementedException();

        protected override string MainConParam => throw new NotImplementedException();


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            sb.Append(@" -jar ..\..\BuildGDX.jar");

            if (game is BloodGame bGame)
            {
                GetBloodArgs(sb, bGame, mod);
            }
            else if (game is DukeGame dGame)
            {
                GetDukeArgs(sb, dGame, mod);
            }
            else if (game is WangGame wGame)
            {
                GetWangArgs(sb, wGame, mod);
            }
            else if (game is SlaveGame sGame)
            {
                GetSlaveArgs(sb, sGame, mod);
            }
            else if (game is RedneckGame rGame)
            {
                GetRedneckArgs(sb, rGame, mod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
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
            if (camp.Id == DukeAddonEnum.WorldTour.ToString())
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
            if (camp.Id == GameEnum.RedneckRA.ToString())
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
