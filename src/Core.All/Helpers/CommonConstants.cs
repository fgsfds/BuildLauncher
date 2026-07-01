namespace Core.All.Helpers;

/// <summary>
///     Provides common constant values used across the application.
/// </summary>
public static class CommonConstants
{
    /// <summary>
    ///     File name of the addon manifest.
    /// </summary>
    public const string AddonManifestName = "addon.json";

    /// <summary>
    ///     GitHub releases API URL.
    /// </summary>
    public static Uri GitHubReleases => new("https://api.github.com/repos/fgsfds/BuildLauncher/releases");

    /// <summary>
    ///     Link to addons.json file.
    /// </summary>
    public static Uri AddonsJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/addons.json");

    /// <summary>
    ///     Link to data.json file.
    /// </summary>
    public static Uri DataJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/data.json");

    /// <summary>
    ///     Link to manifests.json file.
    /// </summary>
    public static Uri ManifestsJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/manifests.json");
}
