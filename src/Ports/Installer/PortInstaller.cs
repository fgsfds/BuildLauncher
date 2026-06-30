using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortInstaller : InstallerBase<BasePort>
{
    private readonly IApiInterface _apiInterface;

    /// <summary>
    /// Initializes a new instance of <see cref="PortInstaller"/>.
    /// </summary>
    /// <param name="port">The port to install.</param>
    /// <param name="apiInterface">API interface for fetching release info.</param>
    /// <param name="filesDownloader">File downloader service.</param>
    /// <param name="archiveTools">Archive extraction service.</param>
    /// <param name="logger">Logger instance.</param>
    public PortInstaller(
        BasePort port,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger<PortInstaller> logger
        ) : base(port, filesDownloader, archiveTools, logger)
    {
        _apiInterface = apiInterface;
    }

    /// <inheritdoc/>
    protected override void Backup() { }

    /// <inheritdoc/>
    protected override void PostInstall(string filePath)
    {
        if (_instance.PortEnum is PortEnum.DosBox)
        {
            var subFolder = Directory.GetDirectories(_instance.InstallFolderPath).FirstOrDefault(x => x.Contains("dosbox-staging", StringComparison.InvariantCultureIgnoreCase));
            FlattenSubfolder(_instance.InstallFolderPath, subFolder);
        }
        else if (_instance.PortEnum is PortEnum.ZeroRecomp)
        {
            var portable = Path.Combine(_instance.InstallFolderPath, "portable.txt");
            File.WriteAllText(portable, string.Empty);

            var subFolder = Directory.GetDirectories(_instance.InstallFolderPath).FirstOrDefault(x => x.Contains("dnzh-", StringComparison.InvariantCultureIgnoreCase));
            FlattenSubfolder(_instance.InstallFolderPath, subFolder);
        }
    }

    /// <summary>
    /// Moves all files from a nested subfolder into the install folder, then removes the subfolder.
    /// </summary>
    /// <param name="installFolderPath">Target installation folder.</param>
    /// <param name="subFolder">The nested subfolder to flatten. If <c>null</c> or empty, a <see cref="NullReferenceException"/> is thrown.</param>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="subFolder"/> is <c>null</c> or empty.</exception>
    private static void FlattenSubfolder(string installFolderPath, string? subFolder)
    {
        if (string.IsNullOrWhiteSpace(subFolder))
        {
            throw new InvalidOperationException("Subfolder not found after unpacking.");
        }

        var files = Directory.EnumerateFiles(subFolder, "*.*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string fileName = Path.GetRelativePath(subFolder, file);
            string destFile = Path.Combine(installFolderPath, fileName);

            var destFolder = Path.GetDirectoryName(destFile) ?? throw new InvalidOperationException($"Could not determine directory for {destFile}");
            Directory.CreateDirectory(destFolder);

            File.Move(file, destFile, true);
        }

        Directory.Delete(subFolder, true);
    }

    /// <inheritdoc/>
    public override void Uninstall() => Directory.Delete(_instance.InstallFolderPath, true);

    /// <inheritdoc/>
    public override Task<GeneralReleaseJsonModel?> GetRelease() => _apiInterface.GetLatestPortReleaseAsync(_instance.PortEnum);
}
