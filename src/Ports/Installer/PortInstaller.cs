using System.Diagnostics;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

/// <summary>
///     Installs a port by downloading, extracting, and performing post-install setup.
/// </summary>
public sealed class PortInstaller : InstallerBase<BasePort>
{
    private readonly IApiInterface _apiInterface;

    /// <summary>
    ///     Initializes a new instance of <see cref="PortInstaller" />.
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

    /// <inheritdoc />
    protected override void Backup() { }

    /// <inheritdoc />
    protected override void PostInstall(string filePath)
    {
        if (_instance.PortEnum is PortEnum.DosBox)
        {
            var subFolder = Directory.GetDirectories(_instance.InstallFolderPath).FirstOrDefault(x => x.Contains("dosbox-staging", StringComparison.InvariantCultureIgnoreCase));
            FlattenSubfolder(_instance.InstallFolderPath, subFolder);
        }
        else if (_instance.PortEnum is PortEnum.ZeroRecomp)
        {
            try
            {
                var portable = Path.Combine(_instance.InstallFolderPath, "portable.txt");
                File.WriteAllText(portable, string.Empty);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error creating portable.txt: {ex.Message}");
            }

            var subFolder = Directory.GetDirectories(_instance.InstallFolderPath).FirstOrDefault(x => x.Contains("dnzh-", StringComparison.InvariantCultureIgnoreCase));
            FlattenSubfolder(_instance.InstallFolderPath, subFolder);
        }
    }

    /// <summary>
    ///     Moves all files from a nested subfolder up to the install folder and removes the subfolder.
    /// </summary>
    /// <param name="installFolderPath">Target install folder path.</param>
    /// <param name="subFolder">Path to the nested subfolder to flatten.</param>
    private static void FlattenSubfolder(string installFolderPath, string? subFolder)
    {
        if (string.IsNullOrWhiteSpace(subFolder))
        {
            throw new InvalidOperationException("Subfolder not found after unpacking.");
        }

        var files = Directory.EnumerateFiles(subFolder, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var fileName = Path.GetRelativePath(subFolder, file);
            var destFile = Path.Combine(installFolderPath, fileName);

            var destFolder = Path.GetDirectoryName(destFile) ?? throw new InvalidOperationException($"Could not determine directory for {destFile}");
            Directory.CreateDirectory(destFolder);

            File.Move(file, destFile, true);
        }

        Directory.Delete(subFolder, true);
    }

    /// <inheritdoc />
    public override void Uninstall()
    {
        if (Directory.Exists(_instance.InstallFolderPath))
        {
            Directory.Delete(_instance.InstallFolderPath, true);
        }
    }

    /// <inheritdoc />
    public override Task<GeneralReleaseJsonModel?> GetRelease(CancellationToken cancellationToken = default) => _apiInterface.GetLatestPortReleaseAsync(_instance.PortEnum, cancellationToken);
}
