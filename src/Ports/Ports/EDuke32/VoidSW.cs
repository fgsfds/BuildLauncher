using System.Text;
using Addons.Addons;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Helpers;
using Common.Client.Helpers;
using CommunityToolkit.Diagnostics;
using Games.Games;

namespace Ports.Ports.EDuke32;

/// <summary>
/// VoidSW port
/// </summary>
public sealed class VoidSW : EDuke32
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.VoidSW;

    /// <inheritdoc/>
    protected override string WinExe => "voidsw.exe";

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "VoidSW";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.Wang];

    /// <inheritdoc/>
    public override string PortInstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
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
    protected override string AddConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override bool IsDownloadable => false;


    /// <inheritdoc/>
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);

        FixConfig();
    }


    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        //don't search for steam/gog installs
        _ = sb.Append($@" -usecwd {AddDirectoryParam}""{game.GameInstallFolder}""");

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



        if (game is WangGame wGame)
        {
            GetWangArgs(sb, wGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }


    private void GetWangArgs(StringBuilder sb, WangGame game, BaseAddon addon)
    {
        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        addon.ThrowIfNotType<GenericCampaignEntity>(out var wCamp);

        if (wCamp.DependentAddons?.ContainsKey(nameof(WangAddonEnum.Wanton)) == true)
        {
            _ = sb.Append($" -addon{(byte)WangAddonEnum.Wanton}");
        }
        else if (wCamp.DependentAddons?.ContainsKey(nameof(WangAddonEnum.TwinDragon)) == true)
        {
            _ = sb.Append($" -addon{(byte)WangAddonEnum.TwinDragon}");
        }
        else
        {
            _ = sb.Append($" -addon{(byte)WangAddonEnum.Base}");
        }


        AddWangMusicFolder(sb, game);


        if (wCamp.FileName is null)
        {
            return;
        }


        if (wCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.CampaignsFolderPath}"" {AddFileParam}""{wCamp.FileName}""");
        }
        else if (wCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, wCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {wCamp.Type} is not supported");
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
            _ = sb.Append(@$" -j""{folder}""");
            return;
        }

        folder = Path.Combine(game.GameInstallFolder!, "classic", "MUSIC");
        if (Directory.Exists(folder))
        {
            _ = sb.Append(@$" -j""{folder}""");
            return;
        }
    }
}
