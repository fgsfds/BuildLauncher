using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using Common.Client.Interfaces;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives.Zip;

namespace Common.Client.Tools;

public sealed class FilesUploader
{
    private readonly IApiInterface _apiInterface;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;


    public FilesUploader(
        IApiInterface apiInterface,
        IHttpClientFactory httpClientFactory,
        ILogger logger
        )
    {
        _apiInterface = apiInterface;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    public async Task<bool> AddAddonToDatabaseAsync(string pathToFile)
    {
        var downloadAddonEntity = await GetDownloadableAddonDtoAsync(pathToFile).ConfigureAwait(false);

        if (downloadAddonEntity is null)
        {
            return false;
        }

        var result = await _apiInterface.AddAddonToDatabaseAsync(downloadAddonEntity).ConfigureAwait(false);

        return result;
    }


    /// <summary>
    /// Upload multiple files to S3
    /// </summary>
    /// <param name="folder">Destination folder in the bucket</param>
    /// <param name="files">List of paths to files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="remoteFileName">File name on the s3 server</param>
    /// <returns>True if successfully uploaded</returns>
    public async Task<Result> UploadFilesAsync(
        string folder,
        List<string> files,
        StrongBox<int> progress,
        CancellationToken cancellationToken,
        string? remoteFileName = null
        )
    {
        _logger.LogInformation($"Uploading {files.Count} file(s)");

        try
        {
            foreach (var file in files)
            {
                var fileName = remoteFileName ?? Path.GetFileName(file);
                var signedUrl = await _apiInterface.GetSignedUrlAsync(Path.Combine(folder, fileName)).ConfigureAwait(false);

                if (!signedUrl.IsSuccess)
                {
                    return new(signedUrl.ResultEnum, signedUrl.Message);
                }

                await using var fileStream = File.OpenRead(file);
                using StreamContent content = new(fileStream);

                _ = Task.Run(() => { TrackProgress(fileStream, progress); });

                using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.Upload.GetDescription());

                using var response = await httpClient.PutAsync(signedUrl.ResultObject, content, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return new(ResultEnum.Error, errorMessage);
                }

                using var check = await httpClient.GetAsync(signedUrl.ResultObject, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None).ConfigureAwait(false);

                FileInfo fileSize = new(file);

                if (!check.IsSuccessStatusCode ||
                    check.Content.Headers.ContentLength != fileSize.Length)
                {
                    return new(ResultEnum.Error, "Error while uploading file.");
                }
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Uploading cancelled");
            return new(ResultEnum.Error, "Uploading cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while uploading fix");
            return new(ResultEnum.Error, ex.Message);
        }

        return new(ResultEnum.Success, string.Empty);


        static void TrackProgress(FileStream streamToTrack, StrongBox<int> progress)
        {
            while (streamToTrack.CanSeek)
            {
                var pos = streamToTrack.Position / (float)streamToTrack.Length * 100;
                progress.Value = (int)pos;

                Thread.Sleep(50);
            }
        }
    }


    private async Task<DownloadableAddonJsonModel?> GetDownloadableAddonDtoAsync(string pathToFile)
    {
        using var archive = ZipArchive.OpenArchive(pathToFile);
        var addonJson = archive.Entries.FirstOrDefault(static x => x.Key.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

        if (addonJson is null)
        {
            return null;
        }

        await using var stream = addonJson.OpenEntryStream();

        var manifest = JsonSerializer.Deserialize(
            stream,
            AddonManifestContext.Default.AddonJsonModel
            );

        FileInfo fileInfo = new(pathToFile);
        var fileSize = fileInfo.Length;

        if (manifest is null)
        {
            return null;
        }

        var folderName = manifest.AddonType switch
        {
            AddonTypeEnum.TC => "Campaigns",
            AddonTypeEnum.Map => "Maps",
            AddonTypeEnum.Mod => "Mods",
            _ => throw new NotSupportedException(),
        };

        var gameName = manifest.SupportedGame.Game switch
        {
            GameEnum.Duke3D => "Duke3D",
            GameEnum.Duke64 => "Duke64",
            GameEnum.Blood => "Blood",
            GameEnum.Wang => "Wang",
            GameEnum.Fury => "Fury",
            GameEnum.Slave => "Slave",
            GameEnum.NAM => "NAM",
            GameEnum.WW2GI => "WW2GI",
            GameEnum.Redneck => "Redneck",
            GameEnum.RidesAgain => "Redneck",
            GameEnum.TekWar => "TekWar",
            GameEnum.Witchaven => "WH",
            GameEnum.Witchaven2 => "WH2",
            GameEnum.Standalone => "Standalone",
            _ => throw new NotSupportedException(),
        };

        var downloadUrl = $"{CommonConstants.S3Endpoint}/{CommonConstants.S3Bucket}/{CommonConstants.S3SubFolder}/{gameName}/{folderName}/{Path.GetFileName(pathToFile)}";

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var fileStream = File.OpenRead(pathToFile);
        var hash = await SHA256.HashDataAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
        var hashStr = Convert.ToHexString(hash);

        DownloadableAddonJsonModel downloadableAddon = new()
        {
            Id = manifest.Id,
            Title = manifest.Title,
            DownloadUrl = new(downloadUrl),
            Game = manifest.SupportedGame.Game,
            AddonType = manifest.AddonType,
            Version = manifest.Version,
            Description = manifest.Description,
            Author = manifest.Author,
            FileSize = fileSize,
            Dependencies = manifest.Dependencies?.Addons?.Select(d => d.Id)?.ToList(),
            UpdateDate = DateTime.UtcNow,
            MD5 = string.Empty,
            Sha256 = hashStr
        };

        return downloadableAddon;
    }
}
