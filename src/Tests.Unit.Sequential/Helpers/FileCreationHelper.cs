using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

/// <summary>
///     Helper for creating test files and folders.
/// </summary>
public static class FileCreationHelper
{
    /// <summary>
    ///     Creates a temporary folder on disk and returns an <see cref="AddonFilePathWrapper" /> pointing to it.
    ///     The folder is not cleaned up automatically.
    /// </summary>
    /// <param name="pathToFolder">Receives the created folder path.</param>
    public static AddonFilePathWrapper CreateFileInTempDir()
    {
        var pathToFolder = PathHelper.GetFakePath();
        Directory.CreateDirectory(pathToFolder);

        return new(pathToFolder, "addon.json");
    }

    /// <summary>
    ///     Creates a temporary folder with an addon.json manifest file on disk.
    /// </summary>
    /// <returns>A tuple of (folderPath, jsonPath).</returns>
    public static AddonFilePathWrapper CreateAddonManifestInTempFolder(string id, string addonType, string game, string title, string version)
    {
        var folderPath = PathHelper.GetFakePath();
        Directory.CreateDirectory(folderPath);
        var jsonPath = Path.Combine(folderPath, "addon.json");

        File.WriteAllText(
            jsonPath, $$"""
            {
                "id": "{{id}}",
                "type": "{{addonType}}",
                "game": { "name": "{{game}}" },
                "title": "{{title}}",
                "version": "{{version}}"
            }
            """
            );

        return new(folderPath, "addon.json");
    }
}
