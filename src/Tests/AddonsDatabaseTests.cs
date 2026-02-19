using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using Minio;
using Minio.DataModel.Args;
using Xunit.Abstractions;

namespace Tests;

public sealed class AddonsDatabaseTests
{
    private readonly ITestOutputHelper _output;

    public AddonsDatabaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DatabaseFilesIntegrityTest()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using HttpClient httpClient = new();

        httpClient.DefaultRequestHeaders.Add("User-Agent", "UnitTest");
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var addonsJsonString = File.ReadAllText("../../../../db/addons.json");
        var addonsJson = JsonSerializer.Deserialize(addonsJsonString, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        Assert.NotNull(addonsJson);

        StringBuilder sbFails = new();
        StringBuilder sbSuccesses = new();

        foreach (var pair in addonsJson)
        {
            foreach (var addon in pair.Value)
            {
                var url = addon.DownloadUrl;
                var size = addon.FileSize;
                var hash = addon.Sha256;

                bool needToSkip = false;

                if (string.IsNullOrWhiteSpace(hash))
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't have hash in the database.");
                    needToSkip = true;
                }

                if (size < 1)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't have size in the database.");
                    needToSkip = true;
                }

                if (needToSkip)
                {
                    continue;
                }

                using var header = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!header.IsSuccessStatusCode)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't exist.");
                    continue;
                }

                if (header.Content.Headers.ContentLength is null)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't have size in the header.");
                }
                else if (size != header.Content.Headers.ContentLength)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} size doesn't match. Expected {size} got {header.Content.Headers.ContentLength}.");
                }

                //hash of files from my storage
                if (url.ToString().StartsWith(CommonConstants.S3Endpoint))
                {
                    var actualHash = header.Headers
                        .FirstOrDefault(x => x.Key.Equals("x-amz-meta-checksum-sha256"))
                        .Value
                        ?.FirstOrDefault();

                    if (actualHash is null)
                    {
                        _ = sbFails.AppendLine($"[Error] File {url} doesn't have Hash.");
                    }
                    else
                    {
                        if (!actualHash.Equals(hash, StringComparison.OrdinalIgnoreCase))
                        {
                            _ = sbFails.AppendLine($"[Error] File {url} has wrong Hash.");
                        }
                        else
                        {
                            _ = sbSuccesses.AppendLine($"[Info] File's {url} Hash matches: {hash}.");
                        }
                    }
                }
                else
                {
                    await using var stream = await httpClient.GetStreamAsync(url).ConfigureAwait(false);

                    var actualHash = await SHA256.HashDataAsync(stream);
                    var actualHashStr = Convert.ToHexString(actualHash);

                    if (!actualHashStr.Equals(hash, StringComparison.OrdinalIgnoreCase))
                    {
                        _ = sbFails.AppendLine($"[Error] File {url} has wrong Sha256.");
                    }
                    else
                    {
                        _ = sbSuccesses.AppendLine($"[Info] File's {url} Sha256 matches: {hash}.");
                    }
                }
            }
        }

        _output.WriteLine(sbSuccesses.ToString());
        Assert.True(sbFails.Length < 1, sbFails.ToString());
    }

    [Fact]
    public async Task LooseFilesTest()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var addonsJsonString = File.ReadAllText("../../../../db/addons.json");
        var addonsJson = JsonSerializer.Deserialize(addonsJsonString, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        Assert.NotNull(addonsJson);


        List<string>? addonsUrls = new List<string>();

        foreach (var a in addonsJson.Values)
        {
            foreach (var b in a)
            {
                addonsUrls.Add(b.DownloadUrl.ToString());
            }
        }

        var access = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("S3_SECRET_KEY");

        Assert.NotNull(access);
        Assert.NotNull(secret);

        using var minioClient = new MinioClient();
        using var iMinioClient = minioClient
            .WithEndpoint(CommonConstants.S3Endpoint)
            .WithCredentials(access, secret)
            .WithSSL(false)
            .Build();

        var args = new ListObjectsArgs()
            .WithBucket(CommonConstants.S3Bucket)
            .WithPrefix(CommonConstants.S3SubFolder + '/')
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

            filesInBucket.Add($"{CommonConstants.S3Endpoint}/{CommonConstants.S3Bucket}/{item.Key}");
        }

        var loose = filesInBucket.Except(addonsUrls);

        StringBuilder sb = new();

        foreach (var item in loose)
        {
            _ = sb.AppendLine($"[Error] File {item} is loose.");
        }

        _output.WriteLine(sb.ToString());
        Assert.True(sb.Length < 1);
    }


    [Fact, Trait("Category", "Database")]
    public async Task UploadFixTest()
    {
        //if (!OperatingSystem.IsWindows())
        //{
        //    return;
        //}

        //ClientProperties.IsOfflineMode = true;

        //using HttpClient httpClient = new();
        //var logger = new Mock<ILogger>().Object;
        //var httpClientFactory = new Mock<IHttpClientFactory>();
        //httpClientFactory.Setup(x => x.CreateClient()).Returns(httpClient);
        //ProgressReport progressReport = new();
        //OfflineApiInterface api = new(logger);
        //FilesUploader uploader = new(api, httpClientFactory.Object);

        //await uploader.UploadFilesAsync("test", [Path.Combine("resources", "test_fix.zip")], CancellationToken.None);

        //var url = $"{CommonConstants.S3Endpoint}/{CommonConstants.S3Bucket}/uploads/{CommonConstants.S3Folder}/test/test_fix.zip";

        //using var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        //Assert.True(resp.IsSuccessStatusCode);

        //var timespan = DateTime.Now - resp.Content.Headers.LastModified;
        //Assert.True(timespan < TimeSpan.FromSeconds(5));
    }
}
