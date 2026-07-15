using System.Security.Cryptography;
using System.Text.Json;
using Core.All.Serializable.Addon;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Moq;
using S3;

namespace Tests.Database;

/// <summary>
///     Tests for the addons database integrity and operations.
/// </summary>
public sealed class AddonsDatabaseTests
{
    /// <summary>
    ///     Shared HTTP client for testing remote file integrity.
    /// </summary>
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonsDatabaseTests" /> class.
    /// </summary>
    public AddonsDatabaseTests()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "UnitTest");
    }

    /// <summary>
    ///     Gets theory data for testing addon database file integrity.
    /// </summary>
    public static IEnumerable<TheoryDataRow<Uri, long, string>> GetAddonTestData()
    {
        var jsonString = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
        var addonsJson = JsonSerializer.Deserialize(jsonString, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        Assert.NotNull(addonsJson);

        foreach (var pair in addonsJson)
        {
            foreach (var addon in pair.Value)
            {
                yield return new(addon.DownloadUrl, addon.FileSize, addon.Sha256);
            }
        }
    }

    /// <summary>
    ///     Tests that database files exist and have correct sizes and hashes.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetAddonTestData))]
    public async Task DatabaseFilesIntegrityTest(Uri url, long size, string hash)
    {
        Assert.False(string.IsNullOrWhiteSpace(hash), $"File {url} doesn't have hash in the database.");
        Assert.True(size > 1, $"File {url} doesn't have size in the database.");

        using var header = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        Assert.True(header.IsSuccessStatusCode, $"File {url} doesn't exist.");
        Assert.True(header.Content.Headers.ContentLength > 1, $"File {url} doesn't have size in the header.");
        Assert.Equal(size, header.Content.Headers.ContentLength);

        if (url.ToString().StartsWith(S3Constants.S3Endpoint))
        {
            var actualHashStr = header.Headers
                                      .FirstOrDefault(x => x.Key.Equals("x-amz-meta-checksum-sha256")).Value
                                     ?.FirstOrDefault();

            Assert.NotNull(actualHashStr);
            Assert.Equal(hash, actualHashStr, true);
        }
        else
        {
            await using var stream = await _httpClient.GetStreamAsync(url);

            var actualHash = await SHA256.HashDataAsync(stream);
            var actualHashStr = Convert.ToHexString(actualHash);

            Assert.NotNull(actualHashStr);
            Assert.Equal(hash, actualHashStr, true);
        }
    }

    /// <summary>
    ///     Tests that there are no loose files in the S3 bucket not referenced by the database.
    /// </summary>
    [Fact]
    public async Task LooseFilesTest()
    {
        var addonsJsonString = File.ReadAllText(ClientProperties.PathToLocalAddonsJson);
        var addonsJson = JsonSerializer.Deserialize(addonsJsonString, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        Assert.NotNull(addonsJson);

        var addonsUrls = new List<string>();

        foreach (var a in addonsJson.Values)
        {
            foreach (var b in a)
            {
                addonsUrls.Add(b.DownloadUrl.ToString());
            }
        }

        var access = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");

        Assert.NotNull(access);
        Assert.NotNull(secret);

        using var minioClient = new MinioClient();

        using var iMinioClient = minioClient
                                .WithEndpoint(S3Constants.S3Endpoint.Split("//").Last())
                                .WithCredentials(access, secret)
                                .WithSSL(false)
                                .Build();

        var args = new ListObjectsArgs()
                  .WithBucket(S3Constants.S3Bucket)
                  .WithPrefix(S3Constants.S3SubFolder + '/')
                  .WithRecursive(true);

        var filesInBucket = new List<string>();

        await foreach (var item in iMinioClient.ListObjectsEnumAsync(args))
        {
            if (item.Key.EndsWith('/'))
            {
                continue;
            }

            if (item.Key.Contains("metadata", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            filesInBucket.Add($"{S3Constants.S3Endpoint}/{S3Constants.S3Bucket}/{item.Key}");
        }

        var looseFiles = filesInBucket.Except(addonsUrls).ToList();

        Assert.False(
            looseFiles.Count != 0,
            $"Loose files: {Environment.NewLine}{string.Join(Environment.NewLine, looseFiles)}"
            );
    }


    /// <summary>
    ///     Tests that uploading a file to S3 succeeds and returns correct metadata.
    /// </summary>
    [Fact]
    public async Task UploadAddonTest()
    {
        Mock<IConfigProvider> config = new();
        S3UtilitiesFactory s3factory = new(config.Object);
        S3FilesUploader filesUploader = new(s3factory, NullLogger<S3FilesUploader>.Instance);

        var uploadResult = await filesUploader.UploadFileToPublicAsync(Path.Combine("Files", "TEST.MAP"), "test/TEST.MAP", new(), CancellationToken.None);

        Assert.True(uploadResult.IsSuccess);

        var timespan = DateTime.UtcNow - uploadResult.ResultObject.Value.LastModified;
        Assert.True(timespan < TimeSpan.FromSeconds(5));
        Assert.Equal(213378, uploadResult.ResultObject.Value.Size);
        Assert.Equal("https://s3-nl.hostkey.com/b8743306-fgsfds/uploads/buildlauncher/test/TEST.MAP", uploadResult.ResultObject.Value.Url.ToString());
    }

    /// <summary>
    ///     Tests that the manifests JSON file is valid and deserializable.
    /// </summary>
    [Fact]
    public async Task ManifestsJsonTest()
    {
        var manifestsJsonString = await File.ReadAllTextAsync(ClientProperties.PathToLocalManifestsJson);
        var manifests = JsonSerializer.Deserialize(manifestsJsonString, AddonManifestJsonContext.Default.ListAddonManifestJsonModel);

        Assert.NotNull(manifests);
    }
}
