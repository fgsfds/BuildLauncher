using ClientCommon.API;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Mods.Serializable;
using SharpCompress.Archives.Zip;
using System.Text.Json;

namespace Mods
{
    public sealed class FilesUploader
    {
        private readonly ApiInterface _apiInterface;


        public FilesUploader(ApiInterface apiInterface)
        {
            _apiInterface = apiInterface;
        }


        public async Task<bool> AddAddonToDatabaseAsync(string pathToFile)
        {
            var entity = await GetDownloadableAddonDtoAsync(pathToFile);

            if (entity is null)
            {
                return false;
            }

            var result = await _apiInterface.AddAddonToDatabaseAsync(entity);

            return result;
        }


        private async Task<AddonsJsonEntity?> GetDownloadableAddonDtoAsync(string pathToFile)
        {
            using var archive = ZipArchive.Open(pathToFile);
            var addonJson = archive.Entries.FirstOrDefault(static x => x.Key == "addon.json");

            if (addonJson is null)
            {
                return null;
            }

            var manifest = JsonSerializer.Deserialize(
                addonJson.OpenEntryStream(),
                AddonManifestContext.Default.AddonDto
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
                _ => throw new NotImplementedException(),
            };

            var gameName = manifest.SupportedGame.Game switch
            {
                GameEnum.Duke3D => "Duke3D",
                GameEnum.Duke64 => "Duke64",
                GameEnum.Blood => "Blood",
                GameEnum.ShadowWarrior => "Wang",
                GameEnum.Fury => "Fury",
                GameEnum.Exhumed => "Slave",
                GameEnum.NAM => "NAM",
                GameEnum.WWIIGI => "WWII",
                GameEnum.Redneck => "Redneck",
                GameEnum.RidesAgain => "Redneck",
                GameEnum.TekWar => "TekWar",
                GameEnum.Witchaven => "WH",
                GameEnum.Witchaven2 => "WH2",
                _ => throw new NotImplementedException(),
            };

            var downloadUrl = $"{Consts.FilesRepo}/{gameName}/{folderName}/{Path.GetFileName(pathToFile)}";

            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            AddonsJsonEntity downMod = new()
            {
                Id = manifest.Id,
                Title = manifest.Title,
                DownloadUrl = downloadUrl,
                Game = manifest.SupportedGame.Game,
                AddonType = manifest.AddonType,
                Version = manifest.Version,
                Description = manifest.Description,
                Author = manifest.Author,
                FileSize = fileSize,
                Dependencies = manifest.Dependencies?.Addons is null ? null : manifest.Dependencies.Addons.ToDictionary(x => x.Id, y => y.Version)
            };

            return downMod;
        }
    }
}
