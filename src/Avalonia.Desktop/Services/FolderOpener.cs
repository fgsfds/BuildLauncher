using System.Diagnostics;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop.Services;

/// <summary>
///     Opens a file system folder using the operating system's default file explorer.
/// </summary>
public sealed class FolderOpener : IFolderOpener
{
    private readonly ILogger<FolderOpener> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FolderOpener" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger instance.
    /// </param>
    public FolderOpener(ILogger<FolderOpener> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Opens the specified folder in the OS file explorer.
    /// </summary>
    /// <param name="path">
    ///     The absolute path to the folder to open.
    /// </param>
    public void OpenFolder(string path)
    {
        try
        {
            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                }
                );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open folder: {Path}", path);
        }
    }
}
