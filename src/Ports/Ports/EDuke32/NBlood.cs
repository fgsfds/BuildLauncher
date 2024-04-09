﻿using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// NBlood port
    /// </summary>
    public class NBlood : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.NBlood;

        /// <inheritdoc/>
        public override string Exe => "nblood.exe";

        /// <inheritdoc/>
        public override string Name => "NBlood";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Blood];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("nblood_win64");


        /// <inheritdoc/>
        protected override string ConfigFile => "nblood.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            game.CreateCombinedMod();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (game is not BloodGame bGame ||
                mod is not BloodCampaign bMod)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            var ini = bMod.INI;

            if (bMod.Type is ModTypeEnum.Map)
            {
                if (bMod.Id == BloodAddonEnum.Blood.ToString())
                {
                    ini = Consts.BloodIni;
                }
                else if (bMod.Id == BloodAddonEnum.BloodCP.ToString())
                {
                    ini = Consts.CrypticIni;
                }
            }

            sb.Append($@" -usecwd -nosetup -j ""{bGame.GameInstallFolder}"" -g BLOOD.RFF -ini ""{ini}""");

            if (bMod.FileName is null)
            {
                return;
            }

            if (bMod.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -g ""{Path.Combine(bGame.CampaignsFolderPath, bMod.FileName)}""");
            }
            else if (bMod.Type is ModTypeEnum.Map)
            {
                //TODO loose maps
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, bMod.FileName)}""");

                //TODO maps
                sb.Append($@" -map ""{bMod.StartMap}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bMod.Type} is not supported");
                return;
            }
        }
    }
}
