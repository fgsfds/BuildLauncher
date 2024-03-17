using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
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
            GameEnum.Again,
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


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            sb.Append(@" -jar ..\..\BuildGDX.jar");

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
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame _, IMod campaign, Dictionary<Guid, IMod> mods) { }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) { }

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -silent \"true\"");


        private void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign camp)
        {
            if (camp.AddonEnum is DukeAddonEnum.WorldTour)
            {
                sb.Append($@" -path ""{game.DukeWTInstallPath}""");
            }
            else
            {
                sb.Append($@" -path ""{game.GameInstallFolder}""");
            }
        }

        private void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        private static void GetWangArgs(StringBuilder sb, WangGame game, WangCampaign camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            if (camp.AddonEnum is RedneckAddonEnum.Again)
            {
                sb.Append($@" -path ""{game.AgainInstallPath}""");
            }
            else
            {
                sb.Append($@" -path ""{game.GameInstallFolder}""");
            }
        }

        private static void GetSlaveArgs(StringBuilder sb, SlaveGame game, SlaveCampaign camp)
        {
            sb.Append($@" -path ""{game.GameInstallFolder}""");
        }
    }
}
