using Common.All.Enums;
using Common.All.Serializable.Downloadable;
using Common.Client;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortInstaller : InstallerBase<BasePort>
{
    private readonly IApiInterface _apiInterface;

    public PortInstaller(
        BasePort port,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger logger
        ) : base(port, filesDownloader, archiveTools)
    {
        _apiInterface = apiInterface;
    }

    /// <inheritdoc/>
    protected override void InstallInternal(string filePath)
    {
        if (_instance.PortEnum is PortEnum.DosBox)
        {
            var subFolder = Directory.GetDirectories(_instance.InstallFolderPath).FirstOrDefault(x => x.Contains("dosbox-staging"));

            if (string.IsNullOrWhiteSpace(subFolder))
            {
                throw new NullReferenceException(nameof(subFolder));
            }

            var files = Directory.EnumerateFiles(subFolder, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string fileName = Path.GetRelativePath(subFolder, file);
                string destFile = Path.Combine(_instance.InstallFolderPath, fileName);

                var destFolder = Path.GetDirectoryName(destFile)!;
                Directory.CreateDirectory(destFolder);

                File.Move(file, destFile, true);
            }
        }
    }

    /// <inheritdoc/>
    public override void Uninstall() => Directory.Delete(_instance.InstallFolderPath, true);

    /// <inheritdoc/>
    public override Task<GeneralReleaseJsonModel?> GetRelease() => _apiInterface.GetLatestPortReleaseAsync(_instance.PortEnum);
}
