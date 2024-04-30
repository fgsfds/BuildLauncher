using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// RedNukem port
    /// </summary>
    public sealed class Fury(ConfigEntity config) : EDuke32
    {
        private readonly ConfigEntity _config = config;

        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Fury;

        /// <inheritdoc/>
        public override string Exe => "fury.exe";

        /// <inheritdoc/>
        public override string Name => "Fury";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Fury];

        /// <inheritdoc/>
        public override string PathToPortFolder => _config.GamePathFury ?? string.Empty;

        /// <inheritdoc/>
        public override bool IsInstalled => File.Exists(FullPathToExe);

        /// <inheritdoc/>
        protected override string ConfigFile => "fury.cfg";

        /// <inheritdoc/>
        public override Uri RepoUrl => ThrowHelper.NotImplementedException<Uri>();

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => ThrowHelper.NotImplementedException<Func<GitHubReleaseAsset, bool>>();


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (mod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{mod.MainDef}""");
            }
            //no need to override main def

            if (mod.AdditionalDefs is not null)
            {
                foreach (var def in mod.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }


            if (game is FuryGame fGame && mod is FuryCampaign fCamp)
            {
                GetFuryArgs(sb, fGame, fCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }

        private void GetFuryArgs(StringBuilder sb, FuryGame game, FuryCampaign camp)
        {
            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

                if (camp.MainCon is not null)
                {
                    sb.Append($@" {MainConParam}""{camp.MainCon}""");
                }

                if (camp.AdditionalCons?.Count > 0)
                {
                    foreach (var con in camp.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
            else if (camp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }
    }
}
