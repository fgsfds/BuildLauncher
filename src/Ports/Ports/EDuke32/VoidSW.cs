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
        protected override string MainDefParam => "-h";

        /// <inheritdoc/>
        protected override string AddDefParam => "-mh";

        /// <inheritdoc/>
        protected override string AddConParam => ThrowHelper.NotImplementedException<string>();

        /// <inheritdoc/>
        protected override string MainConParam => ThrowHelper.NotImplementedException<string>();


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            //don't search for steam/gog installs
            sb.Append($@" -usecwd");

            sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");


            if (game is WangGame wGame && mod is WangCampaign wMod)
            {
                GetWangArgs(sb, wGame, wMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }


        private void GetWangArgs(StringBuilder sb, WangGame wGame, WangCampaign wMod)
        {
            sb.Append($@" -addon{(byte)wMod.RequiredAddonEnum}");

            AddWangMusicFolder(sb, wGame);


            if (wMod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{wMod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (wMod.FileName is null)
            {
                return;
            }


            if (wMod.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddDirectoryParam}""{wGame.CampaignsFolderPath}"" {AddFileParam}""{wMod.FileName}""");
            }
            else if (wMod.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, wGame, wMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {wMod.Type} is not supported");
                return;
            }
        }


        /// <summary>
        /// Add music folders to the search list if music files don't exist in the game directory
        /// </summary>
        private static void AddWangMusicFolder(StringBuilder sb, WangGame game)
        {
            if (File.Exists(Path.Combine(game.GameInstallFolder!, "track02.ogg")))
            {
                return;
            }

            var folder = Path.Combine(game.GameInstallFolder!, "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }

            folder = Path.Combine(game.GameInstallFolder!, "classic", "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }
        }
    }
}
