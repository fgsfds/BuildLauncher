using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports;

/// <summary>
///     BuildGDX port.
/// </summary>
public sealed class BuildGDX : BasePort
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.BuildGDX;

    /// <inheritdoc />
    protected override string WinExe => Path.Combine("jre", "bin", "javaw.exe");

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "BuildGDX";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } =
    [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.Wang,
        GameEnum.Slave,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        GameEnum.NAM,
        GameEnum.Witchaven,
        GameEnum.Witchaven2,
        GameEnum.TekWar
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<string> SupportedGamesVersions { get; } =
    [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic),
        nameof(DukeVersionEnum.Duke3D_WT)
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.TROR,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => new()
    {
        AddDirectory = null,
        MainGrp = null,
        AddGrp = null,
        AddFile = null,
        AddDef = null,
        AddCon = null,
        MainDef = null,
        MainCon = null,
        SkillLevel = null,
        AddGameDir = null,
        AddRff = null,
        AddSnd = null,
        SkipIntro = null,
        SkipStartup = null,
        SkipSteam = null
    };


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
        FilesHelpers.RestoreRoute66Files(game);
        FilesHelpers.RestoreWtFiles(game);
    }

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesToStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
    }

    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.AppendStart(@" -jar ..\..\BuildGDX.jar");
    }
}
