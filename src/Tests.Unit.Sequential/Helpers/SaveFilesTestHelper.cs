using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports;
using Ports.Builders;
using Ports.Helpers;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

/// <summary>
///     Test proxy for <see cref="BasePort" /> that exposes protected members for testing.
/// </summary>
internal sealed class BasePortTestProxy : BasePort
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.Stub;

    /// <inheritdoc />
    protected override string WinExe => string.Empty;

    /// <inheritdoc />
    protected override string LinExe => string.Empty;
    /// <inheritdoc />
    public override string Name => string.Empty;

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [];

    /// <inheritdoc />
    public override string? InstalledVersion => string.Empty;

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
    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }

    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign) { }

    /// <inheritdoc />
    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesFromStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    /// <inheritdoc />
    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesToStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    /// <inheritdoc />
    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId) => GetPathToAddonSavedGamesFolder(subFolder, addonId);

    /// <inheritdoc />
    public string CallGetMapArgs(BaseGame game, BaseAddon camp)
    {
        CmdParametersBuilder sb = CmdParametersBuilderFactory.Create(game, camp, this);
        sb.AppendTcOrMapArgs(camp);

        return sb.ToString();
    }

    /// <inheritdoc />
    public string CallGetOptionsArgs(BaseGame game, BaseAddon addon, IReadOnlyList<string> enabledOptions)
    {
        CmdParametersBuilder sb = CmdParametersBuilderFactory.Create(game, addon, this);
        sb.AppendOptionsArgs(enabledOptions);

        return sb.ToString();
    }
}


/// <summary>
///     Test proxy for <see cref="EDuke32" /> that exposes protected members for testing.
/// </summary>
internal sealed class EDuke32TestProxy : EDuke32
{
    /// <inheritdoc />
    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesFromStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    /// <inheritdoc />
    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesToStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    /// <inheritdoc />
    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId) => GetPathToAddonSavedGamesFolder(subFolder, addonId);
}
