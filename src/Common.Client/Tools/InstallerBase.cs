using System.Security.Cryptography;
using Common.All.Serializable.Downloadable;
using Ports.Ports;

namespace Common.Client.Tools;

public abstract class InstallerBase<T> where T : IInstallable
{
    protected readonly T _instance;
    private readonly FilesDownloader _filesDownloader;
    private readonly ArchiveTools _archiveTools;

    /// <summary>
    /// Installation progress
    /// </summary>
    public Progress<float> Progress { get; init; } = new();

    protected InstallerBase(
        T instance,
        FilesDownloader filesDownloader,
         ArchiveTools archiveTools
        )
    {
        _instance = instance;
        _filesDownloader = filesDownloader;
        _archiveTools = archiveTools;
    }

    /// <summary>
    /// Check hash of the local file
    /// </summary>
    /// <param name="filePath">Full path to the file</param>
    /// <param name="hashStr">Hash that the file's hash will be compared to</param>
    /// <returns>true if check is passed</returns>
    protected static async Task<bool> CheckFileHashAsync(
        string filePath,
        string? hashStr
        )
    {
        if (hashStr is null)
        {
            return true;
        }

        const string Sha = "sha256:";
        if (hashStr.StartsWith(Sha))
        {
            hashStr = hashStr[Sha.Length..];
        }

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        using var sha256 = SHA256.Create();

        var hashBytes = sha256.ComputeHash(fileStream);
        var sha = Convert.ToHexString(hashBytes);

        return hashStr.Equals(hashStr, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Install tool
    /// </summary>
    protected abstract void InstallInternal();

    public abstract void Uninstall();

    public abstract Task<GeneralReleaseJsonModel?> GetRelease();

    public async Task<bool> InstallAsync()
    {
        try
        {
            _filesDownloader.ProgressChanged += OnProgressChanged;
            _archiveTools.ProgressChanged += OnProgressChanged;

            var release = await GetRelease().ConfigureAwait(false);

            if (release?.DownloadUrl is null)
            {
                return false;
            }

            var filePath = Path.GetFileName(release.DownloadUrl.ToString());

            var isDownloaded = await _filesDownloader.DownloadFileAsync(release.DownloadUrl, filePath, CancellationToken.None).ConfigureAwait(false);

            if (!isDownloaded)
            {
                return false;
            }

            var isHashCorrect = await CheckFileHashAsync(filePath, release.Hash).ConfigureAwait(false);

            if (!isHashCorrect)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return false;
            }

            InstallInternal();

            await _archiveTools.UnpackArchiveAsync(filePath, _instance.InstallFolderPath).ConfigureAwait(false);

            File.Delete(filePath);

            File.WriteAllText(Path.Combine(_instance.InstallFolderPath, "version"), release.Version);

            return true;
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
        }
    }

    protected void OnProgressChanged(object? _, float e)
    {
        ((IProgress<float>)Progress).Report(e);
    }
}