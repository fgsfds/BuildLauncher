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
                var md5 = addon.MD5;

                using var header = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!header.IsSuccessStatusCode)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't exist.");
                    continue;
                }

                if (header.Content.Headers.ContentLength != size)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} size doesn't match. Expected {size} got {header.Content.Headers.ContentLength}");
                }

                if (md5 is null)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't have MD5 in the database.");
                }

                if (header.Headers.ETag?.Tag is null)
                {
                    _ = sbFails.AppendLine($"[Error] File {url} doesn't have ETag.");
                }
                else
                {
                    var md5e = header.Headers.ETag.Tag.Replace("\"", "");

                    if (md5e.Contains('-'))
                    {
                        _ = sbFails.AppendLine($"[Error] File {url} has incorrect ETag.");
                    }
                    else if (!md5e.Equals(md5, StringComparison.OrdinalIgnoreCase))
                    {
                        _ = sbFails.AppendLine($"[Error] File {url} has wrong MD5.");
                    }
                    else
                    {
                        _ = sbSuccesses.AppendLine($"[Info] File's {url} MD5 matches: {md5}.");
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

        var access = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var secret = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");

        Assert.NotNull(access);
        Assert.NotNull(secret);

        var split = Consts.FilesRepo.Split("/");
        var endpoint = split[^2];
        var bucket = split[^1];

        using var minioClient = new MinioClient();
        using var iMinioClient = minioClient
            .WithEndpoint(endpoint)
            .WithCredentials(access, secret)
            .WithSSL(false)
            .Build();

        var args = new ListObjectsArgs()
            .WithBucket(bucket)
            .WithRecursive(true);

        var filesInBucket = new List<string>();

        await foreach (var item in iMinioClient.ListObjectsEnumAsync(args))
        {
            if (item.Key.EndsWith('/'))
            {
                continue;
            }

            if (item.Key.EndsWith("s3browser-sync-metadata", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            filesInBucket.Add(Consts.FilesRepo + "/" + item.Key);
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
}
