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
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            mod.ThrowIfNotType<WangCampaign>(out var wangCamp);
            game.ThrowIfNotType<WangGame>(out var wangGame);

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
                ThrowHelper.NotImplementedException($"Mod type {wangCamp.ModType} is not supported");
                return;
            }
        }

        // <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IMod campaign, Dictionary<Guid, IMod> mods)
        {
            if (!mods.Any())
            {
                return;
            }

            sb.Append($@" -j""{game.ModsFolderPath}""");

            foreach (var mod in mods)
            {
                mod.Value.ThrowIfNotType<AutoloadMod>(out var autoloadMod);

                if (!autoloadMod.IsEnabled)
                {
                    //skipping disabled mods
                    continue;
                }

                if (!autoloadMod.SupportedPorts?.Contains(PortEnum) ?? false)
                {
                    //skipping mods not supported by the current port
                    continue;
                }

                if (campaign.Addon is not null &&
                    autoloadMod.SupportedAddons is not null &&
                    autoloadMod.SupportedAddons.Contains(campaign.Addon))
                {
                    //skipping mods not supported by the current addon
                    continue;
                }

                sb.Append($@" -g""{mod.Value.FileName}""");
            }

            sb.Append($@" -j""{Path.Combine(game.SpecialFolderPath, Consts.CombinedModFolder)}"" -mh""{Consts.CombinedDef}""");
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
