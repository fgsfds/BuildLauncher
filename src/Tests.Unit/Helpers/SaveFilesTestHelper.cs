using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

internal sealed class BasePortTestProxy : BasePort
{
    public override PortEnum PortEnum => PortEnum.Stub;
    protected override string WinExe => string.Empty;
    protected override string LinExe => string.Empty;
    public override string Name => string.Empty;
    public override List<GameEnum> SupportedGames => [];
    public override List<FeatureEnum> SupportedFeatures => [];
    public override string? InstalledVersion => string.Empty;
    public override bool IsSkillSelectionAvailable => false;
    protected override string ConfigFile => string.Empty;
    protected override string AddDirectoryParam => string.Empty;
    protected override string MainGrpParam => string.Empty;
    protected override string AddGrpParam => string.Empty;
    protected override string AddFileParam => string.Empty;
    protected override string AddDefParam => string.Empty;
    protected override string AddConParam => string.Empty;
    protected override string MainDefParam => string.Empty;
    protected override string MainConParam => string.Empty;
    protected override string SkillParam => string.Empty;
    protected override string AddGameDirParam => string.Empty;
    protected override string AddRffParam => string.Empty;
    protected override string AddSndParam => string.Empty;
    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }
    public override void BeforeStart(BaseGame game, BaseAddon campaign) { }
    protected override void GetAutoloadModsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IReadOnlyList<BaseAddon> mods) { }
    protected override void GetSkipIntroParameter(StringBuilder sb) { }
    protected override void GetSkipStartupParameter(StringBuilder sb) { }
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon) { }

    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign)
        => MoveSaveFilesFromStorage(game, campaign);

    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign)
        => MoveSaveFilesToStorage(game, campaign);

    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId)
        => GetPathToAddonSavedGamesFolder(subFolder, addonId);

    public void CallGetMapArgs(StringBuilder sb, BaseAddon camp)
        => GetMapArgs(sb, camp);

    public void CallGetOptionsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IReadOnlyList<string> enabledOptions)
        => GetOptionsArgs(sb, game, addon, enabledOptions);
}


internal sealed class EDuke32TestProxy : EDuke32
{
    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign)
        => MoveSaveFilesFromStorage(game, campaign);

    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign)
        => MoveSaveFilesToStorage(game, campaign);

    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId)
        => GetPathToAddonSavedGamesFolder(subFolder, addonId);
}
