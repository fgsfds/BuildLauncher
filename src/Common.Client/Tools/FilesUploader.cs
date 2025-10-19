using System.Text.Json;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Addon;
using Common.All.Serializable.Downloadable;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using SharpCompress.Archives.Zip;

namespace Common.Client.Tools;

public sealed class FilesUploader
{
    private readonly IApiInterface _apiInterface;
    private readonly HttpClient _httpClient;


    public FilesUploader(
        IApiInterface apiInterface,
        HttpClient httpClient
        )
    {
        _apiInterface = apiInterface;
        _httpClient = httpClient;
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


    private async Task<DownloadableAddonJsonModel?> GetDownloadableAddonDtoAsync(string pathToFile)
    {
        using var archive = ZipArchive.Open(pathToFile);
        var addonJson = archive.Entries.FirstOrDefault(static x => x.Key == "addon.json");

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
            _ => ThrowHelper.ThrowNotSupportedException<string>(),
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
            _ => ThrowHelper.ThrowNotSupportedException<string>(),
        };

        var downloadUrl = $"{Consts.FilesRepo}/{gameName}/{folderName}/{Path.GetFileName(pathToFile)}";

        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

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
            UpdateDate = DateTime.UtcNow
        };

        return downloadableAddon;
    }
}
