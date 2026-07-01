namespace Core.Client.Interfaces;

/// <summary>
///     Defines the contract for an installable port or tool.
/// </summary>
public interface IInstallable
{
    /// <summary>
    ///     Path to install folder.
    /// </summary>
    string InstallFolderPath { get; }

    /// <summary>
    ///     Is installed.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    ///     Currently installed version.
    /// </summary>
    string? InstalledVersion { get; }
}
