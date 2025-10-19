namespace Common.All.Helpers;

public static class Consts
{
    public const string S3Endpoint = "http://176.222.52.233:9000";

    /// <summary>
    /// Path to the files repository
    /// </summary>
    public const string FilesRepo = $"{S3Endpoint}/buildlauncher";

    /// <summary>
    /// Path to the uploads folder
    /// </summary>
    public const string UploadsFolder = $"{S3Endpoint}/uploads/buildlauncher";

    /// <summary>
    /// GirtHub releases Url
    /// </summary>
    public const string GitHubReleases = "https://api.github.com/repos/fgsfds/BuildLauncher/releases";

    /// <summary>
    /// Link to addons.json file
    /// </summary>
    public const string AddonsJsonUrl = "https://raw.githubusercontent.com/fgsfds/BuildLauncher/refs/heads/master/db/addons.json";

    /// <summary>
    /// Addon manifest file
    /// </summary>
    public const string AddonManifest = "addon.json";

    /// <summary>
    /// S3 bucket address
    /// </summary>
    public const string Bucket = "";
}
