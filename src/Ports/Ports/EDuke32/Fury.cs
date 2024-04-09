using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// RedNukem port
    /// </summary>
    public sealed class Fury : EDuke32
    {
        private readonly ConfigEntity _config;

        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Fury;

        /// <inheritdoc/>
        public override string Exe => "fury.exe";

        /// <inheritdoc/>
        public override string Name => "Fury";

        public override string PathToPortFolder => _config.GamePathFury ?? string.Empty;

        public override bool IsInstalled => File.Exists(FullPathToExe);

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Fury
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => ThrowHelper.NotImplementedException<Uri>();

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => ThrowHelper.NotImplementedException<Func<GitHubReleaseAsset, bool>>();

        public Fury(ConfigEntity config)
        {
            _config = config;
        }


        /// <inheritdoc/>
        protected override string ConfigFile => "fury.cfg";


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (game is not FuryGame)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            sb.Append($@" -nosetup");

            if (mod.Type is ModTypeEnum.Map)
            {
                //TODO restore loose maps
                //if (mod.IsLoose)
                //{
                //    sb.Append($@" -j ""{Path.Combine(game.MapsFolderPath)}""");
                //}
                //else
                //{
                //    sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, mod.FileName!)}""");
                //}

                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, mod.FileName!)}""");
                //TODO
                //sb.Append($@" -map ""{mod.StartupFile}""");
            }
        }
    }
}
