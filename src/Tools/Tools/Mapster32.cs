using Common.Enums;
using Common.Helpers;
using Common.Releases;
using Games.Providers;

namespace Tools.Tools
{
    public sealed class Mapster32 : BaseTool
    {
        private readonly GamesProvider _gamesProvider;

        /// <inheritdoc/>
        public override string Exe => "mapster32.exe";

        /// <inheritdoc/>
        public override string Name => "Mapster32";

        /// <inheritdoc/>
        public override Uri RepoUrl => null;

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => null;

        /// <inheritdoc/>
        public override string PathToExecutableFolder => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");

        /// <inheritdoc/>
        public override bool CanBeInstalled => false;

        /// <inheritdoc/>
        public override bool CanBeLaunched => _gamesProvider.GetGame(GameEnum.Duke3D).IsBaseGameInstalled;


        public Mapster32(GamesProvider gamesProvider)
        {
            _gamesProvider = gamesProvider;
        }


        /// <inheritdoc/>
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
