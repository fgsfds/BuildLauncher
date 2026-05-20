using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives.Zip;

namespace Core.Client.Tools;

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


    /// <summary>
    /// Uploads addon to a corresponding folder and adds it to the database.
    /// </summary>
    /// <param name="pathToFile">Path to file</param>
    /// <param name="progress">Uploading progress</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Result> UploadAddonAndAddToDbAsync(
        string pathToFile,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        var manifest = await GetMainManifest(pathToFile).ConfigureAwait(false);

        if (!manifest.IsSuccess)
        {
            return new(manifest.ResultEnum, manifest.Message);
        }

        var path = GetUrl(manifest.ResultObject, pathToFile);

        var uploadResult = await UploadAddonAsync(path, pathToFile, progress, cancellationToken).ConfigureAwait(false);

        if (!uploadResult.IsSuccess)
        {
            return new(uploadResult.ResultEnum, uploadResult.Message);
        }

        var downloadAddonEntity = await GetDownloadableAddonDtoAsync(path, pathToFile, manifest.ResultObject).ConfigureAwait(false);

        if (!downloadAddonEntity.IsSuccess)
        {
            return new(downloadAddonEntity.ResultEnum, downloadAddonEntity.Message);
        }

        var result = await _apiInterface.AddAddonToDatabaseAsync(manifest.ResultObject, downloadAddonEntity.ResultObject).ConfigureAwait(false);

        return new(result ? ResultEnum.Success : ResultEnum.Error, "Error while adding addon to the database.");
    }


    /// <summary>
    /// Uploads file to the Upload folder.
    /// </summary>
    /// <param name="subfolderName">Destination folder in the bucket</param>
    /// <param name="pathToFile">Path to file</param>
    /// <param name="progress">Uploading progress</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Result> UploadFileToUploadsFolderAsync(
        string subfolderName,
        string pathToFile,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation($"Uploading file");

        try
        {
            var signedUrl = await _apiInterface.GetSignedUrlAsync(Path.Combine(subfolderName, Path.GetFileName(pathToFile))).ConfigureAwait(false);

            if (!signedUrl.IsSuccess)
            {
                return new(signedUrl.ResultEnum, signedUrl.Message);
            }

            using var fileStream = File.OpenRead(pathToFile);
            using StreamContent content = new(fileStream);

            _ = Task.Run(() => { TrackProgress(fileStream, progress); }, cancellationToken);

            using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.Upload.GetDescription());
            using var response = await httpClient.PutAsync(signedUrl.ResultObject, content, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is System.Net.HttpStatusCode.PreconditionFailed)
            {
                return new(ResultEnum.Error, "Wrong secret key.");
            }
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return new(ResultEnum.Error, errorMessage);
            }

            using var check = await httpClient.GetAsync(signedUrl.ResultObject, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None).ConfigureAwait(false);

            FileInfo fileSize = new(pathToFile);

            if (!check.IsSuccessStatusCode ||
                check.Content.Headers.ContentLength != fileSize.Length)
            {
                return new(ResultEnum.Error, "Error while uploading file.");
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
    }


    /// <summary>
    /// Upload multiple files to S3
    /// </summary>
    /// <param name="folder">Destination folder in the bucket</param>
    /// <param name="file">List of paths to files</param>
    /// <param name="progress">Uploading progress</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successfully uploaded</returns>
    private async Task<Result> UploadAddonAsync(
        string folder,
        string file,
        StrongBox<int> progress,
        CancellationToken cancellationToken
        )
    {
        _logger.LogInformation($"Uploading file");

        try
        {
            using var fileStream = File.OpenRead(file);
            using StreamContent content = new(fileStream);

            _ = Task.Run(() => { TrackProgress(fileStream, progress); }, cancellationToken);

            var sha = await SHA256.HashDataAsync(fileStream, cancellationToken).ConfigureAwait(false);
            var shaStr = Convert.ToHexString(sha);
            fileStream.Position = 0;

            using HttpRequestMessage request = new();
            request.Method = HttpMethod.Put;
            request.Content = content;
            request.Headers.Add("x-amz-meta-checksum-sha256", shaStr);
            request.RequestUri = new(folder);

            using var httpClient = _httpClientFactory.CreateClient(HttpClientEnum.AuthUpload.GetDescription());
            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.PreconditionFailed)
            {
                return new(ResultEnum.Error, "File already exists.");
            }
            else if (response.StatusCode is HttpStatusCode.Forbidden)
            {
                return new(ResultEnum.Error, "Wrong secret key.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return new(ResultEnum.Error, errorMessage);
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
    }


    /// <summary>
    /// Returns manifest from addon.json if exists.
    /// </summary>
    /// <param name="pathToFile">Path to file.</param>
    private async Task<Result<AddonJsonModel?>> GetMainManifest(string pathToFile)
    {
        using var archive = ZipArchive.OpenArchive(pathToFile);
        var addonJson = archive.Entries.FirstOrDefault(static x => x.Key.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

        if (addonJson is null)
        {
            return new(ResultEnum.NotFound, null, "Can't find addon info in the provided archive.");
        }

        using var stream = await addonJson.OpenEntryStreamAsync().ConfigureAwait(false);

        var manifest = await JsonSerializer.DeserializeAsync(
            stream,
            AddonManifestContext.Default.AddonJsonModel
            ).ConfigureAwait(false);

        if (manifest is null)
        {
            return new(ResultEnum.Error, null, "Error while deserializing addon.json.");
        }

        return new(ResultEnum.Success, manifest, string.Empty);
    }


    private string GetUrl(AddonJsonModel manifest, string pathToFile)
    {
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
        return downloadUrl;
    }


    private async Task<Result<DownloadableAddonJsonModel?>> GetDownloadableAddonDtoAsync(string downloadUrl, string pathToFile, AddonJsonModel manifest)
    {
        FileInfo fileInfo = new(pathToFile);
        var fileSize = fileInfo.Length;

        using var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return new(ResultEnum.Error, null, $"File {downloadUrl} doesn't exist.");
        }

        using var fileStream = File.OpenRead(pathToFile);
        var sha = await SHA256.HashDataAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
        var shaStr = Convert.ToHexString(sha);
        var md5 = await MD5.HashDataAsync(fileStream, CancellationToken.None).ConfigureAwait(false);
        var md5Str = Convert.ToHexString(md5);

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
            MD5 = md5Str,
            Sha256 = shaStr
        };

        return new(ResultEnum.Success, downloadableAddon, string.Empty);
    }


    private static void TrackProgress(FileStream streamToTrack, StrongBox<int> progress)
    {
        while (streamToTrack.CanSeek)
        {
            var pos = streamToTrack.Position / (float)streamToTrack.Length * 100;
            progress.Value = (int)pos;

            Thread.Sleep(50);
        }
    }
}
