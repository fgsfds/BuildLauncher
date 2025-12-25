namespace Common.Client.Interfaces;

public interface IInstallable
{
    /// <summary>
    /// Path to install folder.
    /// </summary>
    string InstallFolderPath { get; }

    /// <summary>
    /// Is installed.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    /// Currently installed version.
    /// </summary>
    string? InstalledVersion { get; }
}