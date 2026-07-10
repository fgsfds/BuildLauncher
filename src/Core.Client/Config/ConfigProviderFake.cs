using Core.All;
using Core.Client.Enums;
using Core.Client.Interfaces;

namespace Core.Client.Config;

/// <summary>
///     A fake configuration provider for testing that returns default values.
/// </summary>
public sealed class ConfigProviderFake : IConfigProvider
{
    /// <summary>
    ///     Gets a dictionary of addon scores.
    /// </summary>
    public Dictionary<string, bool> Scores => [];
    /// <inheritdoc />
    public string? ApiPassword { get; set; } = null;
    /// <inheritdoc />
    public bool IsConsented { get; set; } = true;
    /// <inheritdoc />
    public string? PathBlood { get; set; } = null;
    /// <inheritdoc />
    public string? PathDuke3D { get; set; } = null;
    /// <inheritdoc />
    public string? PathDuke64 { get; set; } = null;
    /// <inheritdoc />
    public string? PathDukeZH { get; set; } = null;
    /// <inheritdoc />
    public string? PathDukeWT { get; set; } = null;
    /// <inheritdoc />
    public string? PathFury { get; set; } = null;
    /// <inheritdoc />
    public string? PathRedneck { get; set; } = null;
    /// <inheritdoc />
    public string? PathRidesAgain { get; set; } = null;
    /// <inheritdoc />
    public string? PathSlave { get; set; } = null;
    /// <inheritdoc />
    public string? PathWang { get; set; } = null;
    /// <inheritdoc />
    public string? PathNam { get; set; } = null;
    /// <inheritdoc />
    public string? PathWW2GI { get; set; } = null;
    /// <inheritdoc />
    public string? PathWitchaven { get; set; } = null;
    /// <inheritdoc />
    public string? PathWitchaven2 { get; set; } = null;
    /// <inheritdoc />
    public string? PathTekWar { get; set; } = null;
    /// <inheritdoc />
    public string? GitHubToken { get; set; } = null;
    /// <inheritdoc />
    public string? S3SecretKey { get; set; } = null;
    /// <inheritdoc />
    public Dictionary<string, byte> Rating { get; set; } = [];

    /// <inheritdoc />
    public Dictionary<string, TimeSpan> Playtimes => [];
    /// <inheritdoc />
    public HashSet<string> DisabledAutoloadMods => [];

    /// <inheritdoc />
    public HashSet<string> GetEnabledOptions(string addonId) => [];

    /// <inheritdoc />
    public ThemeEnum Theme { get; set; } = ThemeEnum.System;
    /// <inheritdoc />
    public bool SkipIntro { get; set; } = false;
    /// <inheritdoc />
    public bool SkipStartup { get; set; } = false;
    /// <inheritdoc />
    public bool UseLocalApi { get; set; } = true;

    /// <inheritdoc />
    public HashSet<AddonId> FavoriteAddons => [];


    /// <inheritdoc />
    public event IConfigProvider.ParameterChanged? ParameterChangedEvent;

    /// <inheritdoc />
    public void AddPlaytime(string addonId, TimeSpan playTime) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(AddPlaytime)}.");

    /// <inheritdoc />
    public void AddScore(string addonId, byte rating) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(AddScore)}.");

    /// <inheritdoc />
    public void ChangeAddonOptionState(string addonId, string option, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeAddonOptionState)}.");

    /// <inheritdoc />
    public void ChangeFavoriteState(AddonId addonId, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeFavoriteState)}.");

    /// <inheritdoc />
    public void ChangeModState(AddonId addonId, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeModState)}.");
}
