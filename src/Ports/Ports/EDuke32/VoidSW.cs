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
        public override string PathToPortFolder => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");


        /// <inheritdoc/>
        protected override string ConfigFile => "voidsw.cfg";

        /// <inheritdoc/>
        protected override string AddDirectoryParam => "-j";

        /// <inheritdoc/>
        protected override string AddFileParam => "-g";

        /// <inheritdoc/>
        protected override string AddDefParam => "-mh";


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (game is not WangGame wGame ||
                mod is not WangCampaign wMod)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            sb.Append($@" -usecwd -nosetup");

            AddMusicFolder(sb, wGame);

            sb.Append($@" -j""{wGame.GameInstallFolder}"" -addon{wMod.AddonEnum}");

            if (mod.FileName is null)
            {
                return;
            }

            if (mod.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -j""{wGame.CampaignsFolderPath}"" -g""{mod.FileName}""");
            }
            else if (mod.Type is ModTypeEnum.Map)
            {
                //TODO restore loose maps
                //if (mod.IsLoose)
                //{
                //    sb.Append($@" -j""{Path.Combine(wGame.MapsFolderPath)}""");
                //}
                //else
                //{
                //    sb.Append($@" -g""{Path.Combine(wGame.MapsFolderPath, mod.FileName)}""");
                //}

                //TODO
                //sb.Append($@" -map ""{mod.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} is not supported");
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
