using System.Text;
using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Games.Games;

namespace Ports.Ports;

public sealed class StubPort : BasePort
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

    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
    }

    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
    }

    protected override void GetAutoloadModsArgs(StringBuilder sb, BaseGame game, BaseAddon addon, IEnumerable<KeyValuePair<AddonId, BaseAddon>> mods)
    {
    }

    protected override void GetSkipIntroParameter(StringBuilder sb)
    {
    }

    protected override void GetSkipStartupParameter(StringBuilder sb)
    {
    }

    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
    }
}
