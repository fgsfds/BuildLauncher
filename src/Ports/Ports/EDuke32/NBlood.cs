using Common.Enums;
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
            //nothing to do
        }


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            //don't search for steam/gog installs
            sb.Append($@" -usecwd");

            if (game is BloodGame bGame && mod is BloodCampaign bMod)
            {
                GetBloodArgs(sb, bGame, bMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }


        protected void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign bMod)
        {
            sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");


            if (bMod.INI is not null)
            {
                sb.Append($@" -ini ""{bMod.INI}""");
            }
            else if (bMod.RequiredAddonEnum is BloodAddonEnum.BloodCP)
            {
                sb.Append($@" -ini ""{Consts.CrypticIni}""");
            }


            if (bMod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{bMod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (bMod.FileName is null)
            {
                return;
            }


            if (bMod.RFF is not null)
            {
                sb.Append($@" -rff {bMod.RFF}");
            }


            if (bMod.SND is not null)
            {
                sb.Append($@" -snd {bMod.SND}");
            }


            if (bMod.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, bMod.FileName)}""");
            }
            else if (bMod.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, bMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bMod.Type} is not supported");
                return;
            }
        }
    }
}
