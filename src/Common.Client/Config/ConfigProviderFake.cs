using Common.All;
using Common.Client.Enums;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using static Common.Client.Interfaces.IConfigProvider;

namespace Common.Client.Config;

public sealed class ConfigProviderFake : IConfigProvider
{
    public string ApiPassword { get; set; } = string.Empty;
    public bool IsConsented { get; set; } = true;
    public string? PathBlood { get; set; } = null;
    public string? PathDuke3D { get; set; } = null;
    public string? PathDuke64 { get; set; } = null;
    public string? PathDukeWT { get; set; } = null;
    public string? PathFury { get; set; } = null;
    public string? PathRedneck { get; set; } = null;
    public string? PathRidesAgain { get; set; } = null;
    public string? PathSlave { get; set; } = null;
    public string? PathWang { get; set; } = null;
    public string? PathNam { get; set; } = null;
    public string? PathWW2GI { get; set; } = null;
    public string? PathWitchaven { get; set; } = null;
    public string? PathWitchaven2 { get; set; } = null;
    public string? PathTekWar { get; set; } = null;
    public Dictionary<string, byte> Rating { get; set; } = [];

    public Dictionary<string, TimeSpan> Playtimes => [];
    public Dictionary<string, bool> Scores => [];
    public HashSet<string> DisabledAutoloadMods => [];
    public HashSet<string> GetEnabledOptions(string addonId) => [];

    public ThemeEnum Theme { get; set; } = ThemeEnum.System;
    public bool SkipIntro { get; set; } = false;
    public bool SkipStartup { get; set; } = false;
    public bool UseLocalApi { get; set; } = true;

    public HashSet<AddonId> FavoriteAddons => [];

    public event ParameterChanged? ParameterChangedEvent;

    public void AddPlaytime(string addonId, TimeSpan playTime) => ThrowHelper.ThrowNotSupportedException();

    public void AddScore(string addonId, byte rating) => ThrowHelper.ThrowNotSupportedException();

    public void ChangeAddonOptionState(string addonId, string option, bool isEnabled) => ThrowHelper.ThrowNotSupportedException();

    public void ChangeFavoriteState(AddonId addonVersion, bool isEnabled) => ThrowHelper.ThrowNotSupportedException();

    public void ChangeModState(AddonId addonId, bool isEnabled) => ThrowHelper.ThrowNotSupportedException();
}
