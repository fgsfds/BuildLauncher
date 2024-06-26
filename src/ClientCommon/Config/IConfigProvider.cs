﻿using Common;
using Common.Enums;
using static ClientCommon.Config.ConfigProvider;

namespace ClientCommon.Config;
public interface IConfigProvider
{
    string ApiPassword { get; set; }
    HashSet<string> DisabledAutoloadMods { get; }
    string? PathBlood { get; set; }
    string? PathDuke3D { get; set; }
    string? PathDuke64 { get; set; }
    string? PathDukeWT { get; set; }
    string? PathFury { get; set; }
    string? PathRedneck { get; set; }
    string? PathRidesAgain { get; set; }
    string? PathSlave { get; set; }
    string? PathWang { get; set; }
    Dictionary<string, TimeSpan> Playtimes { get; }
    Dictionary<string, byte> Rating { get; }
    bool SkipIntro { get; set; }
    bool SkipStartup { get; set; }
    ThemeEnum Theme { get; set; }
    bool UseLocalApi { get; set; }

    event ParameterChanged ParameterChangedEvent;

    void AddPlaytime(string addonId, TimeSpan playTime);
    void AddScore(string addonId, byte rating);
    void ChangeModState(AddonVersion addonId, bool isEnabled);
}