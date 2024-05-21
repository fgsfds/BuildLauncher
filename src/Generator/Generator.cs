using Common.Helpers;
using Mods.Serializable;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System.Text.Json;
using Web.Server.Entities;

namespace Generator
{
    internal class Generator
    {
        private static void Main(string[] args)
        {
            var unpackedDirectory = args[0];
            var packedDirectory = args[1];

            if (!Directory.Exists(packedDirectory))
            {
                Directory.CreateDirectory(packedDirectory);
            }

            var addonsJson = Path.Combine(packedDirectory, "addons.json");

            var files = Directory.EnumerateFiles(unpackedDirectory, "addon.json", SearchOption.AllDirectories);

            List<AddonsJsonEntity> manifests = [];

            foreach (var addonJson in files)
            {
                var addonJsonFolder = Path.GetDirectoryName(addonJson)!;
                var relativePathToFileFolder = addonJsonFolder.Replace(unpackedDirectory, string.Empty);

                var zipPath = relativePathToFileFolder.Replace('\\', '/') + ".zip";
                var zipName = Path.GetFileName(zipPath);

                var pathInRepo = Consts.FilesRepo + zipPath;

                var pathToNewFile = packedDirectory + relativePathToFileFolder + ".zip";

                FileStream fs = new(addonJson, FileMode.Open);

                AddonDto? json = null;

                try
                {
                    json = JsonSerializer.Deserialize(fs, AddonManifestContext.Default.AddonDto) ?? throw new NullReferenceException();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("---------------------");
                    Console.WriteLine(addonJson);
                    Console.WriteLine(ex);
                    Console.WriteLine("---------------------");

                    continue;
                }
                finally
                {
                    fs.Dispose();
                }

                if (Directory.GetLastWriteTimeUtc(addonJsonFolder) > File.GetLastWriteTimeUtc(addonsJson) ||
                    Directory.GetLastWriteTimeUtc(addonJson) > File.GetLastWriteTimeUtc(addonsJson) ||
                    !File.Exists(pathToNewFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(pathToNewFile)!);

                    using (var archive = ZipArchive.Create())
                    {
                        archive.AddAllFromDirectory(addonJsonFolder);
                        archive.SaveTo(pathToNewFile, CompressionType.Deflate);
                    }

                    Console.WriteLine($"Created {zipName}");
                }

                FileInfo fileInfo = new(pathToNewFile);
                var fileSize = fileInfo.Length;

                AddonsJsonEntity downMod = new()
                {
                    Id = json.Id,
                    Title = json.Title,
                    DownloadUrl = pathInRepo,
                    Game = json.SupportedGame.Game,
                    AddonType = json.AddonType,
                    Version = json.Version,
                    Description = json.Description,
                    Author = json.Author,
                    FileSize = fileSize,
                    Dependencies = json.Dependencies?.Addons is null ? null : json.Dependencies.Addons.ToDictionary(x => x.Id, y => y.Version)
                };

                manifests.Add(downMod);
            }

            var manifestsString = JsonSerializer.Serialize(manifests, AddonsJsonEntityListContext.Default.ListAddonsJsonEntity);

            File.WriteAllText(addonsJson, manifestsString);
        }
    }
}
