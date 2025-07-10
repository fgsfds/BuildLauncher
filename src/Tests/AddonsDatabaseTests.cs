using System.Text;
using System.Text.Json;
using Common.Common.Serializable.Downloadable;
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

        httpClient.DefaultRequestHeaders.Add("User-Agent", "BuildLauncher");
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var addonsJsonString = File.ReadAllText("../../../../db/addons.json");
        var addonsJson = JsonSerializer.Deserialize(addonsJsonString, DownloadableAddonJsonModelDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonJsonModel);

        Assert.NotNull(addonsJson);

        StringBuilder sb = new();

        foreach (var pair in addonsJson)
        {
            foreach (var addon in pair.Value)
            {
                var url = addon.DownloadUrl;
                var size = addon.FileSize;

                using var header = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!header.IsSuccessStatusCode)
                {
                    _ = sb.AppendLine($"File {url} doesn't exist.");
                    continue;
                }

                if (header.Content.Headers.ContentLength != size)
                {
                    _ = sb.AppendLine($"File {url} size doesn't match. Expected {size} got {header.Content.Headers.ContentLength}");
                }
            }
        }

        _output.WriteLine(sb.ToString());
        Assert.True(sb.Length < 1);
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

        using var minio = new MinioClient()
            .WithEndpoint("s3.fgsfds.link:9000")
            .WithCredentials(access, secretKey: secret)
            .Build();

        var args = new ListObjectsArgs()
            .WithBucket("buildlauncher")
            .WithRecursive(true);

        var files = new List<string>();
        await foreach (var item in minio.ListObjectsEnumAsync(args))
        {
            if (item.Key.EndsWith('/'))
            {
                continue;
            }

            if (item.Key.EndsWith("s3browser-sync-metadata", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            files.Add("https://s3.fgsfds.link/buildlauncher/" + item.Key);
        }

        var loose = files.Except(addonsUrls);

        StringBuilder sb = new();

        foreach (var item in loose)
        {
            _ = sb.AppendLine($"File {item} is loose.");
        }

        _output.WriteLine(sb.ToString());
        Assert.True(sb.Length < 1);
    }
}
