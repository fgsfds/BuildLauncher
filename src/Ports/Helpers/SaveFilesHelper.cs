using Core.Client.Helpers;

namespace Ports.Helpers;

/// <summary>
///     Moves save files between the game install folder and the port's saved games storage.
/// </summary>
public static class SaveFilesHelper
{
    private static readonly HashSet<string> SaveFileExtensions =
    [
        ".sav",
        ".esv"
    ];

    /// <summary>
    ///     Moves save files from the addon's saved games storage folder to the given destination folder.
    /// </summary>
    /// <param name="saveFolder">
    ///     The addon's saved games storage folder.
    /// </param>
    /// <param name="destFolder">
    ///     The folder to move the save files into.
    /// </param>
    public static void MoveSaveFilesFromStorage(string saveFolder, string? destFolder)
    {
        if (destFolder is null)
        {
            return;
        }

        if (!Directory.Exists(saveFolder))
        {
            return;
        }

        var saves = Directory.GetFiles(saveFolder);

        foreach (var save in saves)
        {
            var destFileName = Path.Combine(destFolder, Path.GetFileName(save));
            File.Move(save, destFileName, true);
        }
    }

    /// <summary>
    ///     Moves save files from the source folder to the addon's saved games storage folder.
    /// </summary>
    /// <param name="saveFolder">
    ///     The addon's saved games storage folder.
    /// </param>
    /// <param name="sourceFolder">
    ///     The folder containing the save files to move.
    /// </param>
    public static void MoveSaveFilesToStorage(string saveFolder, string sourceFolder)
    {
        ArgumentNullException.ThrowIfNull(sourceFolder);

        var files = from file in Directory.GetFiles(sourceFolder)
                    from ext in SaveFileExtensions
                    where file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
                    select file;

        Ensure.DirectoryExists(saveFolder);

        foreach (var file in files)
        {
            var destFileName = Path.Combine(saveFolder, Path.GetFileName(file));
            File.Move(file, destFileName, true);
        }
    }
}
