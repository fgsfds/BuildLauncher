﻿namespace Common.Helpers;

public static class Consts
{
    /// <summary>
    /// Path to the files repository
    /// </summary>
    public const string FilesRepo = "http://files.fgsfds.link/buildlauncher";

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
    public const string Bucket = "b70f50a9-files";
}
