﻿using Common.Config;
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

        /// <inheritdoc/>
        public override string ConfigFile => "fury.cfg";

        public override string FolderPath => _config.GamePathFury ?? string.Empty;

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
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            mod.ThrowIfNotType<FuryCampaign>(out var dukeCamp);
            game.ThrowIfNotType<FuryGame>(out var dukeGame);

            sb.Append($@" -nosetup");

            if (dukeCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, dukeCamp.FileName)}"" -map ""{dukeCamp.StartupFile}""");
            }
        }
    }
}