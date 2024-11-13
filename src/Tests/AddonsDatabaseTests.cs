using Common.Entities;
using System.Text;
using System.Text.Json;
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
        var addonsJson = JsonSerializer.Deserialize(addonsJsonString, DownloadableAddonsDictionaryContext.Default.DictionaryGameEnumListDownloadableAddonEntity);

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
                    continue;
                }
            }
        }

        _output.WriteLine(sb.ToString());
        Assert.True(sb.Length < 1);
    }
}
