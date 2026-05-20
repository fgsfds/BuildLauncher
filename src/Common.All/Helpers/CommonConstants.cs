namespace Common.All.Helpers;

public static class CommonConstants
{
    public const string S3Endpoint = "https://s3-nl.hostkey.com";

    public const string S3Bucket = "b8743306-fgsfds";

    public const string S3SubFolder = "buildlauncher";

    /// <summary>
    /// GirtHub releases Url
    /// </summary>
    public static Uri GitHubReleases => new("https://api.github.com/repos/fgsfds/BuildLauncher/releases");

    /// <summary>
    /// Link to addons.json file
    /// </summary>
    public static Uri AddonsJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/addons.json");

    /// <summary>
    /// Link to addons.json file
    /// </summary>
    public static Uri DataJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/data.json");

    /// <summary>
    /// Link to manifests.json file
    /// </summary>
    public static Uri ManifestsJsonUrl => new("https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/manifests.json");
}
