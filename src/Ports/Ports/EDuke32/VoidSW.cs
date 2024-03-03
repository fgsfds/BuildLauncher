using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;

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
        public override string GetStartCampaignArgs(IGame game, IMod mod)
        {
            var args = $@" -usecwd -nosetup";

            AddMusicFolder(game, ref args);

            if (mod is not WangCampaign dukeCamp ||
                game is not WangGame dukeGame)
            {
                ThrowHelper.ArgumentException();
                return null;
            }

            if (dukeCamp.FileName is null)
            {
                return args += $@" -j""{dukeGame.GameInstallFolder}"" -j""E:\Steam\steamapps\common\Shadow Warrior Classic\gameroot\classic\MUSIC"" -addon{(byte)dukeCamp.AddonEnum}";
            }
            else
            {
                return args += $@" -j""{dukeGame.GameInstallFolder}"" -j""{dukeGame.CampaignsFolderPath}"" -g""{dukeCamp.FileName}"" -addon{(byte)dukeCamp.AddonEnum}";
            }
        }

        /// <summary>
        /// Add music folders to the search list if music files don't exist in the game directory
        /// </summary>
        private static void AddMusicFolder(IGame game, ref string args)
        {
            if (File.Exists(Path.Combine(game.GameInstallFolder, "track02.ogg")))
            {
                return;
            }

            var folder = Path.Combine(game.GameInstallFolder, "MUSIC");
            if (Directory.Exists(folder))
            {
                args += @$" -j""{folder}""";
                return;
            }

            folder = Path.Combine(game.GameInstallFolder, "classic", "MUSIC");
            if (Directory.Exists(folder))
            {
                args += @$" -j""{folder}""";
                return;
            }
        }
    }
}
