using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
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
    public override List<GameEnum> SupportedGames => [];

    /// <inheritdoc />
    public override List<FeatureEnum> SupportedFeatures => [];

    /// <inheritdoc />
    public override string? InstalledVersion => string.Empty;

    /// <inheritdoc />
    public override bool IsSkillSelectionAvailable => false;

    /// <inheritdoc />
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc />
    protected override string AddDirectoryParam => string.Empty;

    /// <inheritdoc />
    protected override string MainGrpParam => string.Empty;

    /// <inheritdoc />
    protected override string AddGrpParam => string.Empty;

    /// <inheritdoc />
    protected override string AddFileParam => string.Empty;

    /// <inheritdoc />
    protected override string AddDefParam => string.Empty;

    /// <inheritdoc />
    protected override string AddConParam => string.Empty;

    /// <inheritdoc />
    protected override string MainDefParam => string.Empty;

    /// <inheritdoc />
    protected override string MainConParam => string.Empty;

    /// <inheritdoc />
    protected override string SkillParam => string.Empty;

    /// <inheritdoc />
    protected override string AddGameDirParam => string.Empty;

    /// <inheritdoc />
    protected override string AddRffParam => string.Empty;

    /// <inheritdoc />
    protected override string AddSndParam => string.Empty;

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }

    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign) { }

    /// <inheritdoc />
    protected override void GetAutoloadModsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IReadOnlyList<BaseAddon> mods) { }

    /// <inheritdoc />
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc />
    protected override void GetSkipStartupParameter(StringBuilder sb) { }

    /// <inheritdoc />
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon) { }

    /// <inheritdoc />
    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign) => MoveSaveFilesFromStorage(game, campaign);

    /// <inheritdoc />
    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign) => MoveSaveFilesToStorage(game, campaign);

    /// <inheritdoc />
    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId) => GetPathToAddonSavedGamesFolder(subFolder, addonId);

    /// <inheritdoc />
    public void CallGetMapArgs(StringBuilder sb, BaseAddon camp) => GetMapArgs(sb, camp);

    /// <inheritdoc />
    public void CallGetOptionsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IReadOnlyList<string> enabledOptions) => GetOptionsArgs(sb, game, addon, enabledOptions);
}


/// <summary>
///     Test proxy for <see cref="EDuke32" /> that exposes protected members for testing.
/// </summary>
internal sealed class EDuke32TestProxy : EDuke32
{
    /// <inheritdoc />
    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign) => MoveSaveFilesFromStorage(game, campaign);

    /// <inheritdoc />
    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign) => MoveSaveFilesToStorage(game, campaign);

    /// <inheritdoc />
    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId) => GetPathToAddonSavedGamesFolder(subFolder, addonId);
}
