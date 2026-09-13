using Core.All;
using Core.All.Enums;
using Core.Client.Enums;

namespace Core.Client.Interfaces;

/// <summary>
///     Defines the contract for accessing and modifying application configuration, settings, and game paths.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    ///     Represents a method that handles configuration parameter change events.
    /// </summary>
    /// <param name="parameterName">The name of the changed parameter.</param>
    delegate void ParameterChanged(string? parameterName);


    /// <summary>
    ///     Gets the set of addon identifiers that are disabled from autoloading.
    /// </summary>
    IReadOnlySet<string> DisabledAutoloadMods { get; }

    /// <summary>
    ///     Gets the set of favorite addon identifiers with optional version.
    /// </summary>
    IReadOnlySet<AddonId> FavoriteAddons { get; }

    /// <summary>
    ///     Gets or sets whether the user has consented to data collection.
    /// </summary>
    bool IsConsented { get; set; }

    /// <summary>
    ///     Gets or sets whether to use the local offline API instead of the remote one.
    /// </summary>
    bool UseLocalApi { get; set; }

    /// <summary>
    ///     Gets the installation path for the specified game.
    /// </summary>
    /// <param name="game">The game to get the installation path for.</param>
    /// <returns>The game path, or null if not set.</returns>
    string? GetGamePath(GameEnum game);

    /// <summary>
    ///     Sets the installation path for the specified game.
    /// </summary>
    /// <param name="game">The game to set the installation path for.</param>
    /// <param name="value">The path to store.</param>
    void SetGamePath(GameEnum game, string? value);

    /// <summary>
    ///     Gets or sets the installation path for Duke WT, which has no dedicated game enum.
    /// </summary>
    string? PathDukeWT { get; set; }

    /// <summary>
    ///     Gets or sets the API password for local API authentication.
    /// </summary>
    string? ApiPassword { get; set; }

    /// <summary>
    ///     Gets or sets the GitHub authentication token.
    /// </summary>
    string? GitHubToken { get; set; }

    /// <summary>
    ///     Gets or sets the S3 secret key for upload storage.
    /// </summary>
    string? S3SecretKey { get; set; }

    /// <summary>
    ///     Gets the dictionary of playtimes per addon.
    /// </summary>
    IReadOnlyDictionary<string, TimeSpan> Playtimes { get; }

    /// <summary>
    ///     Gets the dictionary of ratings per addon.
    /// </summary>
    IReadOnlyDictionary<string, byte> Rating { get; }

    /// <summary>
    ///     Gets or sets whether to skip the intro video.
    /// </summary>
    bool SkipIntro { get; set; }

    /// <summary>
    ///     Gets or sets whether to skip the startup screen.
    /// </summary>
    bool SkipStartup { get; set; }

    /// <summary>
    ///     Gets or sets the application UI theme.
    /// </summary>
    ThemeEnum Theme { get; set; }

    /// <summary>
    ///     Occurs when a configuration parameter changes.
    /// </summary>
    event ParameterChanged ParameterChangedEvent;

    /// <summary>
    ///     Adds playtime for the specified addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="playTime">The playtime duration to add.</param>
    void AddPlaytime(string addonId, TimeSpan playTime);

    /// <summary>
    ///     Adds or updates a rating score for the specified addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="rating">The rating value.</param>
    void AddScore(string addonId, byte rating);

    /// <summary>
    ///     Enables or disables autoloading for the specified addon mod.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="isEnabled">Whether the mod should be enabled.</param>
    void ChangeModState(in AddonId addonId, bool isEnabled);

    /// <summary>
    ///     Adds or removes the specified addon from favorites.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="isEnabled">Whether to favorite the addon.</param>
    void ChangeFavoriteState(in AddonId addonId, bool isEnabled);

    /// <summary>
    ///     Enables or disables an option for the specified addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <param name="option">The option name.</param>
    /// <param name="isEnabled">Whether the option is enabled.</param>
    void ChangeAddonOptionState(string addonId, string option, bool isEnabled);

    /// <summary>
    ///     Gets the set of enabled options for the specified addon.
    /// </summary>
    /// <param name="addonId">The addon identifier.</param>
    /// <returns>A set of enabled option names.</returns>
    IReadOnlySet<string> GetEnabledOptions(string addonId);
}
