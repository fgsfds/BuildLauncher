using System.Text.Json;
using Core.All.Serializable.Downloadable;
using Core.Client;
using Core.Client.Api;
using Core.Client.Helpers;
using Core.Client.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Avalonia.Desktop.Helpers;

/// <summary> Runs the production updater against an isolated loopback release fixture. </summary>
internal static class PublishedUpdateTest
{
    /// <summary> Downloads a real executable and installs it while the old executable is running. </summary>
    /// <returns>The preparation operation; successful installation exits this process.</returns>
    internal static async Task RunAsync()
    {
        using var metadata = File.OpenRead(Path.Combine(AppContext.BaseDirectory, ".smoke-update.json"));
        var releases = await JsonSerializer.DeserializeAsync(metadata,
            GeneralReleaseJsonModelContext.Default.ListGeneralReleaseJsonModel).ConfigureAwait(false);
        var release = releases?.Single() ?? throw new InvalidDataException("Missing update fixture.");
        if (release.DownloadUrl is not { IsLoopback: true, Scheme: "http" })
        {
            throw new InvalidDataException("Published update tests require an HTTP loopback fixture.");
        }

        var services = new ServiceCollection();
        _ = services.AddHttpClient();
        using var provider = services.BuildServiceProvider();
        var downloader = new FilesDownloader(provider.GetRequiredService<IHttpClientFactory>(), NullLogger<FilesDownloader>.Instance);
        var installer = new AppUpdateInstaller(downloader,
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance), NullLogger<AppUpdateInstaller>.Instance);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await installer.DownloadAndUnpackReleaseAsync(release, ClientProperties.WorkingFolder,
            ClientProperties.ExecutableName, timeout.Token).ConfigureAwait(false);
        AppUpdateInstaller.InstallUpdate(["--offline", "--smoke-test-restart"]);
    }
}
