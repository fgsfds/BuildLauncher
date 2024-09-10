using Common.Tools;
using Tools.Tools;

namespace Tools.Installer;

public sealed class ToolsInstallerFactory(ToolsReleasesProvider toolsReleasesProvider)
{
    /// <summary>
    /// Create <see cref="ToolsInstaller"/> instance
    /// </summary>
    /// <returns></returns>
    public ToolsInstaller Create() => new(toolsReleasesProvider);
}

public sealed class ToolsInstaller
{
    private readonly ArchiveTools _fileTools;
    private readonly ToolsReleasesProvider _toolsReleasesProvider;

    public ToolsInstaller(ToolsReleasesProvider toolsReleasesProvider)
    {
        _fileTools = new();
        _toolsReleasesProvider = toolsReleasesProvider;
        Progress = _fileTools.Progress;
    }

    /// <summary>
    /// Installation progress
    /// </summary>
    public Progress<float> Progress { get; init; }

    /// <summary>
    /// Install tool
    /// </summary>
    /// <param name="port">Port</param>
    public async Task InstallAsync(BaseTool port)
    {
        var release = await _toolsReleasesProvider.GetLatestReleaseAsync(port).ConfigureAwait(false);

        if (release is null)
        {
            return;
        }

        var filePath = Path.GetFileName(release.WindowsDownloadUrl.ToString());

        await _fileTools.DownloadFileAsync(release.WindowsDownloadUrl, filePath).ConfigureAwait(false);

        await _fileTools.UnpackArchiveAsync(filePath, port.PathToExecutableFolder).ConfigureAwait(false);

        File.Delete(filePath);

        File.WriteAllText(Path.Combine(port.PathToExecutableFolder, "version"), release.Version);
    }
}
