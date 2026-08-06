using System.Collections.Immutable;
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
}
