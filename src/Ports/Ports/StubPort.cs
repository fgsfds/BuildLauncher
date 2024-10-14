﻿using Common;
using Common.Enums;
using Common.Interfaces;
using System.Text;

namespace Ports.Ports;

public sealed class StubPort : BasePort
{
    public override PortEnum PortEnum => PortEnum.Stub;

    public override string Exe => string.Empty;

    public override string Name => string.Empty;

    public override List<GameEnum> SupportedGames => [];

    public override List<FeatureEnum> SupportedFeatures => [];

    public override string? InstalledVersion => string.Empty;

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

    public override void AfterEnd(IGame game, IAddon campaign)
    {
    }

    public override void BeforeStart(IGame game, IAddon campaign)
    {
    }

    protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon addon, Dictionary<AddonVersion, IAddon> mods)
    {
    }

    protected override void GetSkipIntroParameter(StringBuilder sb)
    {
    }

    protected override void GetSkipStartupParameter(StringBuilder sb)
    {
    }

    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
    }
}