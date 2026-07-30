using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;

namespace Ports.Ports;

/// <summary>
///     Zero Hour Overclocked recompilation port.
/// </summary>
public sealed class ZHRecomp : BasePort
{
    /// <summary>
    ///     Name of the Duke Nukem Zero Hour ROM file.
    /// </summary>
    private const string RomName = "dnzh.us.z64";

    private readonly IConfigProvider _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ZHRecomp" /> class.
    /// </summary>
    /// <param name="config">Configuration provider.</param>
    public ZHRecomp(IConfigProvider config)
    {
        _config = config;
    }

    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.ZeroRecomp;

    /// <inheritdoc />
    protected override string WinExe => "DNZHRecompiled.exe";

    /// <inheritdoc />
    protected override string LinExe => "DNZHRecompiled";

    /// <inheritdoc />
    public override string Name => "Zero Hour Overclocked";

    /// <inheritdoc />
    public override string ShortName => "ZHRecomp";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [GameEnum.DukeZeroHour];

    /// <inheritdoc />
    public override bool IsSkillSelectionAvailable => false;

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [];

    /// <inheritdoc />
    protected override string ConfigFile => throw new NotImplementedException();

    /// <inheritdoc />
    protected override PortCmdArguments CmdArguments => new()
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
        AddSnd = null
    };

    /// <inheritdoc />
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon) { }

    /// <inheritdoc />
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc />
    protected override void GetSkipStartupParameter(StringBuilder sb) { }

    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        var pathToRom = Path.Combine(InstallFolderPath, RomName);

        if (!File.Exists(pathToRom))
        {
            if (!File.Exists(_config.PathDukeZH))
            {
                throw new InvalidOperationException($"Can't find {_config.PathDukeZH}.");
            }

            File.Copy(_config.PathDukeZH, pathToRom, true);
        }
    }

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }
}
