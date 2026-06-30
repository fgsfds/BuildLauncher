using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Core.Client.Interfaces;
using Games.Games;

namespace Ports.Ports;

/// <summary>
/// Raze port
/// </summary>
public sealed class ZHRecomp : BasePort
{
    private const string RomName = "dnzh.us.z64";

    private readonly IConfigProvider _config;

    public ZHRecomp(IConfigProvider config)
    {
        _config = config;
    }

    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.ZeroRecomp;

    /// <inheritdoc/>
    protected override string WinExe => "DNZHRecompiled.exe";

    /// <inheritdoc/>
    protected override string LinExe => "DNZHRecompiled";

    /// <inheritdoc/>
    public override string Name => "Zero Hour Overclocked";

    public override string ShortName => "ZHRecomp";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames => [GameEnum.DukeZeroHour];

    /// <inheritdoc/>
    public override string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(InstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            return File.ReadAllText(versionFile);
        }
    }

    /// <inheritdoc/>
    public override bool IsSkillSelectionAvailable => false;

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures => [];

    /// <inheritdoc/>
    protected override string ConfigFile => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddDirectoryParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string MainGrpParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddGrpParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddFileParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddDefParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddConParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string MainDefParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string MainConParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string SkillParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddGameDirParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddRffParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddSndParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon) { }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) { }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void AfterEnd(BaseGame game, BaseAddon campaign) { }
}
