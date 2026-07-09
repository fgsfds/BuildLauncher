using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Games.Games;

namespace Ports.Ports;

/// <summary>
///     Stub port with no functionality.
/// </summary>
public sealed class StubPort : BasePort
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.Stub;

    /// <inheritdoc />
    protected override string WinExe => "stub.exe";

    /// <inheritdoc />
    protected override string LinExe => string.Empty;

    /// <inheritdoc />
    public override string Name => "Stub";

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
}
