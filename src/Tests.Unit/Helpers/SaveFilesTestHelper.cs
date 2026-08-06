using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;
using Ports;
using Ports.Builders;
using Ports.Helpers;
using Ports.Ports;

namespace Tests.Unit;

internal sealed class BasePortTestProxy : BasePort
{
    public override PortEnum PortEnum => PortEnum.Stub;

    protected override string WinExe => string.Empty;

    protected override string LinExe => string.Empty;
    public override string Name => string.Empty;

    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [];

    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [];

    public override string? InstalledVersion => string.Empty;

    protected override string ConfigFile => string.Empty;

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

    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }

    public override void BeforeStart(BaseGame game, BaseAddon campaign) { }

    public void CallMoveSaveFilesFromStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesFromStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    public void CallMoveSaveFilesToStorage(BaseGame game, BaseAddon campaign) => SaveFilesHelper.MoveSaveFilesToStorage(
        GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
        GetGameSaveFilesFolder(game, campaign));

    public string CallGetPathToAddonSavedGamesFolder(string subFolder, string addonId) => GetPathToAddonSavedGamesFolder(subFolder, addonId);

    public string CallGetMapArgs(BaseGame game, BaseAddon camp)
    {
        CmdParametersBuilder sb = CmdParametersBuilderFactory.Create(game, camp, this);
        sb.AppendTcOrMapArgs(camp);

        return sb.ToString();
    }

    public string CallGetOptionsArgs(BaseGame game, BaseAddon addon, IReadOnlyList<string> enabledOptions)
    {
        CmdParametersBuilder sb = CmdParametersBuilderFactory.Create(game, addon, this);
        sb.AppendOptionsArgs(enabledOptions);

        return sb.ToString();
    }
}
