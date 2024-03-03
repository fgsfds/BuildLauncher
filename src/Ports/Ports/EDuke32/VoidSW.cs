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
        public override string PortFolder => "EDuke32";

        /// <inheritdoc/>
        public override string ConfigFile => "voidsw.cfg";

        /// <inheritdoc/>
        public override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (mod is not WangCampaign wangCamp ||
                game is not WangGame wangGame)
            {
                ThrowHelper.ArgumentException();
                return;
            }

            sb.Append($@" -usecwd -nosetup");

            AddMusicFolder(sb, game);

            sb.Append($@" -j""{wangGame.GameInstallFolder}"" -addon{(byte)wangCamp.AddonEnum}");

            if (wangCamp.FileName is null)
            {
                return;
            }

            if (wangCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -j""{game.CampaignsFolderPath}"" -g""{wangCamp.FileName}""");
            }
            else if (wangCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -j""{game.CampaignsFolderPath}"" -g""{wangCamp.FileName}"" -map ""{wangCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException();
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
