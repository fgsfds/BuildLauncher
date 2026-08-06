using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.Client.Helpers;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports.EDuke32;

/// <summary>
///     VoidSW port
/// </summary>
public sealed class VoidSW : EDuke32
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.VoidSW;

    /// <inheritdoc />
    protected override string WinExe => "voidsw.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "VoidSW";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [GameEnum.Wang];

    /// <inheritdoc />
    public override string InstallFolderPath => Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    protected override string ConfigFile => "voidsw.cfg";

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => base.CmdArguments with
    {
        AddDirectory = "-j",
        AddFile = "-g",
        MainDef = "-h",
        AddDef = "-mh",
        AddCon = null,
        MainCon = null
    };

    /// <inheritdoc />
    public override bool IsDownloadable => false;


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FixConfig();
    }


    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.AppendSkipSteam();
    }
}
