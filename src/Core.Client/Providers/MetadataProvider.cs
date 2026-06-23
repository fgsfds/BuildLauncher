using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Core.Client.Providers;

public sealed class MetadataProvider
{
    public event EventHandler<ValueTuple<GameEnum, AddonTypeEnum, string>?>? MetadataUpdatedEvent;

    private readonly IApiInterface _apiInterface;
    private readonly ILogger<MetadataProvider> _logger;

    private readonly Dictionary<string, Dictionary<string, AddonManifestJsonModel>> _updatesCache = [];

    public MetadataProvider(
        IApiInterface apiInterface,
        ILogger<MetadataProvider> logger
        )
    {
        _apiInterface = apiInterface;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var metadata = await _apiInterface.GetMetadataAsync().ConfigureAwait(false);

        if (metadata is null)
        {
            return;
        }

        var metaDict = metadata.ToDictionary(x => new AddonId(x.Id, x.Version));

        var files = Directory.EnumerateFiles(ClientProperties.AddonsFolderPath, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    using var archive = ArchiveFactory.OpenArchive(file);
                    var manifests = archive.Entries.Where(x => x.Key!.Contains("addon", StringComparison.OrdinalIgnoreCase) && x.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                    foreach (var manifest in manifests)
                    {
                        using var stream = await manifest.OpenEntryStreamAsync().ConfigureAwait(false);
                        var originalManifest = await JsonSerializer.DeserializeAsync(
                            stream,
                            AddonManifestJsonContext.Default.AddonManifestJsonModel
                            ).ConfigureAwait(false);

                        if (originalManifest is null)
                        {
                            continue;
                        }

                        AddToCacheIfNewer(metaDict, file, originalManifest, manifest.Key);
                    }
                }
                else if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    using var originalManifestStr = File.OpenRead(file);
                    var originalManifest = await JsonSerializer.DeserializeAsync(
                        originalManifestStr,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel
                        ).ConfigureAwait(false);

                    if (originalManifest is null)
                    {
                        continue;
                    }

                    AddToCacheIfNewer(metaDict, file, originalManifest, file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting metadata from {FileName}.", file);
                continue;
            }
        }

        if (_updatesCache.Count > 0)
        {
            MetadataUpdatedEvent?.Invoke(this, null);
        }
    }

    public bool IsMetadataUpdateAvailable(string path) => _updatesCache.TryGetValue(path, out _);

    public async Task<Result<bool>> UpdateMetadataAsync(string path)
    {
        try
        {
            if (!_updatesCache.TryGetValue(path, out var updates))
            {
                return new(ResultEnum.Error, false, string.Empty);
            }

            if (path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using (var archive = ZipArchive.OpenArchive(path))
                {
                    List<MemoryStream> streams = new(updates.Count);

                    foreach (var update in updates)
                    {
                        var existing = archive.Entries.FirstOrDefault(x => x.Key.Equals(update.Key));

                        if (existing is not null)
                        {
                            archive.RemoveEntry(existing);
                        }

                        var ms = new MemoryStream();
                        streams.Add(ms);
                        await JsonSerializer.SerializeAsync(ms, update.Value, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

                        archive.AddEntry(update.Key, ms);
                    }

                    archive.SaveTo(path + ".temp", new(CompressionType.None));
                    streams.ForEach(x => x.Dispose());
                }

                File.Delete(path);
                File.Move(path + ".temp", path);

                _updatesCache.Remove(path);

                MetadataUpdatedEvent?.Invoke(this, new(
                    updates.First().Value.SupportedGame.Game,
                    updates.First().Value.AddonType,
                    path
                    ));
            }
            else if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(path);
                var addonJson = JsonSerializer.Serialize(updates.First().Value, AddonManifestJsonContext.Default.AddonManifestJsonModel);
                await File.WriteAllTextAsync(path, addonJson).ConfigureAwait(false);

                MetadataUpdatedEvent?.Invoke(this, new(
                    updates.First().Value.SupportedGame.Game,
                    updates.First().Value.AddonType,
                    path
                    ));
            }
        }
        catch (Exception ex)
        {
            return new(ResultEnum.Error, false, ex.ToString());
        }

        return new(ResultEnum.Success, false, string.Empty);
    }

    private void AddToCacheIfNewer(Dictionary<AddonId, AddonManifestJsonModel> metaDict, string file, AddonManifestJsonModel originalManifest, string jsonName)
    {
        if (!metaDict.TryGetValue(new(originalManifest.Id, originalManifest.Version), out var actualVersion))
        {
            return;
        }

        var newManifestStr = JsonSerializer.Serialize(actualVersion, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        var originalManifestStr = JsonSerializer.Serialize(originalManifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        if (!originalManifestStr.Equals(newManifestStr))
        {
            if (!_updatesCache.TryAdd(file, new() { { jsonName, actualVersion } }))
            {
                _updatesCache[file].Add(jsonName, actualVersion);
            }
        }
    }
}
