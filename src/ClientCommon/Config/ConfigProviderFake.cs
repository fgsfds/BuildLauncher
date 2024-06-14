using Common;
using Common.Enums;

namespace ClientCommon.Config;

public sealed class ConfigProviderFake : IConfigProvider
{
    public string ApiPassword { get; set; } = string.Empty;
    public string? PathBlood { get; set; } = null;
    public string? PathDuke3D { get; set; } = null;
    public string? PathDuke64 { get; set; } = null;
    public string? PathDukeWT { get; set; } = null;
    public string? PathFury { get; set; } = null;
    public string? PathRedneck { get; set; } = null;
    public string? PathRidesAgain { get; set; } = null;
    public string? PathSlave { get; set; } = null;
    public string? PathWang { get; set; } = null;

    public Dictionary<string, TimeSpan> Playtimes => [];
    public Dictionary<string, bool> Scores => [];
    public HashSet<string> DisabledAutoloadMods => [];

    public ThemeEnum Theme { get; set; } = ThemeEnum.System;
    public bool SkipIntro { get; set; } = false;
    public bool SkipStartup { get; set; } = false;
    public bool UseLocalApi { get; set; } = true;

    public event ConfigProvider.ParameterChanged ParameterChangedEvent;

    public void AddPlaytime(string addonId, TimeSpan playTime) => throw new NotImplementedException();

    public void AddScore(string addonId, bool isUpvote) => throw new NotImplementedException();

    public void ChangeModState(AddonVersion addonId, bool isEnabled) => throw new NotImplementedException();
}
