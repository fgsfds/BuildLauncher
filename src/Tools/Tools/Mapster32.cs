using Common.Enums;
using Common.Helpers;
using Common.Releases;
using Games.Providers;

namespace Tools.Tools
{
    public sealed class Mapster32 : BaseTool
    {
        private readonly GamesProvider _gamesProvider;

        public override string Exe => "mapster32.exe";

        public override string Name => "Mapster32";

        public override Uri RepoUrl => null;

        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => null;

        public override string PathToExecutableFolder => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");

        public override bool CanBeInstalled => false;

        public override bool CanBeLaunched => _gamesProvider.GetGame(GameEnum.Duke3D).IsBaseGameInstalled;

        public Mapster32(GamesProvider gamesProvider)
        {
            _gamesProvider = gamesProvider;
        }

        /// <summary>
        /// Get command line parameters to start the game with selected campaign and autoload mods
        /// </summary>
        /// <param name="game">Game<param>
        public override string GetStartToolArgs()
        {
            var game = _gamesProvider.GetGame(Common.Enums.GameEnum.Duke3D);
            if (!game.IsBaseGameInstalled)
            {
                ThrowHelper.Exception();
            }

            return $@"-game_dir ""{game.GameInstallFolder}""";
        }
    }
}
