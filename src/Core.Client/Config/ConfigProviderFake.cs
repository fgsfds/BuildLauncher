using Core.All;
using Core.All.Enums;
using Core.Client.Enums;
using Core.Client.Interfaces;

namespace Core.Client.Config;

/// <summary>
///     A fake configuration provider for testing that returns default values.
/// </summary>
public sealed class ConfigProviderFake : IConfigProvider
{
    /// <inheritdoc />
    public string WindowPlacement { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ActiveTab { get; set; } = string.Empty;

    /// <summary> Stores the selected sub-tab for each game tab. </summary>
    private readonly Dictionary<string, string> _gameSubTabs = [];

    /// <inheritdoc />
    public string GetGameSubTab(string gameTabName) => _gameSubTabs.GetValueOrDefault(gameTabName, string.Empty);

    /// <inheritdoc />
    public void SetGameSubTab(string gameTabName, string subTabName) => _gameSubTabs[gameTabName] = subTabName;

    /// <summary>
    ///     Gets a dictionary of addon scores.
    /// </summary>
    public Dictionary<string, bool> Scores { get; } = [];
    /// <inheritdoc />
    public string? ApiPassword { get; set; } = null;
    /// <inheritdoc />
    public bool IsConsented { get; set; } = true;
    /// <inheritdoc />
    public string? PathDukeWT { get; set; } = null;
    /// <inheritdoc />
    public string? GitHubToken { get; set; } = null;
    /// <inheritdoc />
    public string? S3SecretKey { get; set; } = null;
    /// <inheritdoc />
    public IReadOnlyDictionary<string, byte> Rating { get; set; } = new Dictionary<string, byte>();

    private readonly Dictionary<GameEnum, string?> _gamePaths = [];

    /// <inheritdoc />
    public string? GetGamePath(GameEnum game) => _gamePaths.TryGetValue(game, out var path) ? path : null;

    /// <inheritdoc />
    public void SetGamePath(GameEnum game, string? value) => _gamePaths[game] = value;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, TimeSpan> Playtimes { get; } = new Dictionary<string, TimeSpan>();
    /// <inheritdoc />
    public IReadOnlySet<string> DisabledAutoloadMods { get; } = new HashSet<string>();

    /// <inheritdoc />
    public IReadOnlySet<string> GetEnabledOptions(string addonId) => new HashSet<string>();

    /// <inheritdoc />
    public ThemeEnum Theme { get; set; } = ThemeEnum.System;
    /// <inheritdoc />
    public bool SkipIntro { get; set; } = false;
    /// <inheritdoc />
    public bool SkipStartup { get; set; } = false;
    /// <inheritdoc />
    public bool UseMica { get; set; } = true;
    /// <inheritdoc />
    public bool UseLocalApi { get; set; } = true;

    /// <inheritdoc />
    public IReadOnlySet<AddonId> FavoriteAddons { get; } = new HashSet<AddonId>();


    /// <inheritdoc />
    public event IConfigProvider.ParameterChanged? ParameterChangedEvent;

    /// <inheritdoc />
    public void AddPlaytime(string addonId, TimeSpan playTime) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(AddPlaytime)}.");

    /// <inheritdoc />
    public void AddScore(string addonId, byte rating) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(AddScore)}.");

    /// <inheritdoc />
    public void ChangeAddonOptionState(string addonId, string option, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeAddonOptionState)}.");

    /// <inheritdoc />
    public void ChangeFavoriteState(in AddonId addonId, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeFavoriteState)}.");

    /// <inheritdoc />
    public void ChangeModState(in AddonId addonId, bool isEnabled) => throw new NotSupportedException($"{nameof(ConfigProviderFake)} does not support {nameof(ChangeModState)}.");
}
