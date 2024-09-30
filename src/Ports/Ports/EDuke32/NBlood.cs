using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Text;

namespace Ports.Ports.EDuke32;

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
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Modern_Types,
        FeatureEnum.Hightile,
        FeatureEnum.Models
        ];

    /// <inheritdoc/>
    protected override string ConfigFile => "nblood.cfg";

    /// <inheritdoc/>
    protected override string SkillParam => "-s ";

    /// <inheritdoc/>
    protected override string AddRffParam => "-rff ";

    /// <inheritdoc/>
    protected override string AddSndParam => "-snd ";


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append(" -usecwd");

        _ = sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");

        if (addon.MainDef is not null)
        {
            _ = sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            _ = sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                _ = sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }

    protected void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon addon)
    {
        if (addon is LooseMap lMap)
        {
            if (lMap.BloodIni is null)
            {
                _ = sb.Append($@" -ini ""{Consts.BloodIni}""");
            }
            else
            {
                _ = sb.Append($@" -ini ""{Path.GetFileName(lMap.BloodIni)}""");
            }

            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not BloodCampaign bCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (bCamp.INI is not null)
        {
            _ = sb.Append($@" -ini ""{bCamp.INI}""");
        }
        else if (bCamp.DependentAddons is not null && bCamp.DependentAddons.ContainsKey(nameof(BloodAddonEnum.BloodCP)))
        {
            _ = sb.Append($@" -ini ""{Consts.CrypticIni}""");
        }


        if (bCamp.FileName is null)
        {
            return;
        }


        if (bCamp.Type is AddonTypeEnum.TC)
        {
            if (bCamp.FileName.Equals("addon.json"))
            {
                var pathToAddonFolder = Path.GetDirectoryName(bCamp.PathToFile)!;
                var addonFolderName = Path.GetFileName(pathToAddonFolder);
                var pathToAddonInPortFolder = Path.Combine(PortInstallFolderPath, addonFolderName);

                _ = sb.Append($@" {AddGameDirParam}""{addonFolderName}""");

                if (Directory.Exists(pathToAddonInPortFolder))
                {
                    Directory.Delete(pathToAddonInPortFolder);
                }

                _ = Directory.CreateSymbolicLink(pathToAddonInPortFolder, pathToAddonFolder);
            }
            else
            {
                _ = sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, bCamp.FileName)}""");
            }
        }
        else if (bCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, bCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {bCamp.Type} is not supported");
            return;
        }


        if (bCamp.RFF is not null)
        {
            _ = sb.Append($@" {AddRffParam}""{bCamp.RFF}""");
        }


        if (bCamp.SND is not null)
        {
            _ = sb.Append($@" {AddSndParam}""{bCamp.SND}""");
        }
    }
}
