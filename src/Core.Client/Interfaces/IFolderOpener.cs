namespace Core.Client.Interfaces;

/// <summary>
///     Provides the ability to open a file system folder in the operating system's file explorer.
/// </summary>
public interface IFolderOpener
{
    /// <summary>
    ///     Opens the specified folder path in the operating system's default file explorer.
    /// </summary>
    /// <param name="path">
    ///     The absolute path to the folder to open.
    /// </param>
    void OpenFolder(string path);
}
