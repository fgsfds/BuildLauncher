using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// VoidSW port
    /// </summary>
    public sealed class VoidSW : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.VoidSW;

        /// <inheritdoc/>
        public override string Exe => "voidsw.exe";

        /// <inheritdoc/>
        public override string Name => "VoidSW";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Wang];

        /// <inheritdoc/>
        public override string FolderPath => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");

        /// <inheritdoc/>
        public override string ConfigFile => "voidsw.cfg";

        /// <inheritdoc/>
        public override string AddDirectoryParam => "-j";

        /// <inheritdoc/>
        public override string AddFileParam => "-g";

        /// <inheritdoc/>
        public override string AddDefParam => "-mh";


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (game is not WangGame wGame || mod is not WangCampaign wCamp)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            sb.Append($@" -usecwd -nosetup");

            AddMusicFolder(sb, wGame);

            sb.Append($@" -j""{wGame.GameInstallFolder}"" -addon{(byte)wCamp.AddonEnum}");

            if (wCamp.FileName is null)
            {
                return;
            }

            if (wCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -j""{wGame.CampaignsFolderPath}"" -g""{wCamp.FileName}""");
            }
            else if (wCamp.ModType is ModTypeEnum.Map)
            {
                if (wCamp.IsLoose)
                {
                    sb.Append($@" -j""{Path.Combine(wGame.MapsFolderPath)}""");
                }
                else
                {
                    sb.Append($@" -g""{Path.Combine(wGame.MapsFolderPath, wCamp.FileName)}""");
                }

                sb.Append($@" -map ""{wCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {wCamp.ModType} is not supported");
                return;
            }
        }


        /// <summary>
        /// Add music folders to the search list if music files don't exist in the game directory
        /// </summary>
        private static void AddMusicFolder(StringBuilder sb, IGame game)
        {
            if (File.Exists(Path.Combine(game.GameInstallFolder, "track02.ogg")))
            {
                return;
            }

            var folder = Path.Combine(game.GameInstallFolder, "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }

            folder = Path.Combine(game.GameInstallFolder, "classic", "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }
        }
    }
}
