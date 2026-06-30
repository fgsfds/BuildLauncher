using Core.All.Enums;
using Core.All.Releases;
using Microsoft.Extensions.Logging;

namespace Core.Client.Releases;

/// <summary>
/// Provides the latest BuildLauncher app release from GitHub for self-update.
/// </summary>
public sealed class AppRepoReleasesProvider : ReleaseProviderBase<AppReleaseEnum>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AppRepoReleasesProvider"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    public AppRepoReleasesProvider(
        ILogger<AppRepoReleasesProvider> logger,
        IHttpClientFactory httpClientFactory
        ) : base(new AppRepositoriesProvider(),logger, httpClientFactory)
    {
    }
}
