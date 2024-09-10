using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Text;

namespace Ports.Ports.EDuke32;

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
    public override List<GameEnum> SupportedGames => [GameEnum.ShadowWarrior];

    /// <inheritdoc/>
    public override string PathToExecutableFolder => Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Hightile,
        FeatureEnum.Models
        ];

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
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        //don't search for steam/gog installs
        sb.Append($@" -usecwd {AddDirectoryParam}""{game.GameInstallFolder}""");

        if (addon.MainDef is not null)
        {
            sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                sb.Append($@" {AddDefParam}""{def}""");
            }
        }



        if (game is WangGame wGame)
        {
            GetWangArgs(sb, wGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }


    private void GetWangArgs(StringBuilder sb, WangGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not WangCampaign wCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (wCamp.DependentAddons is not null &&
            wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.Wanton)))
        {
            sb.Append($" -addon{(byte)WangAddonEnum.Wanton}");
        }
        else if (wCamp.DependentAddons is not null &&
                 wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.TwinDragon)))
        {
            sb.Append($" -addon{(byte)WangAddonEnum.TwinDragon}");
        }
        else
        {
            sb.Append($" -addon{(byte)WangAddonEnum.Base}");
        }


        AddWangMusicFolder(sb, game);


        if (wCamp.FileName is null)
        {
            return;
        }


        if (wCamp.Type is AddonTypeEnum.TC)
        {
            sb.Append($@" {AddDirectoryParam}""{game.CampaignsFolderPath}"" {AddFileParam}""{wCamp.FileName}""");
        }
        else if (wCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, wCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {wCamp.Type} is not supported");
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
