using Core.All.Enums;
using Core.All.Releases;
using Microsoft.Extensions.Logging;

namespace Ports.Releases;

/// <summary>
/// Provides the latest GitHub releases for game ports, with special-case handling for EDuke32 (custom source) and shared-cache for NBlood-derived repos.
/// </summary>
public sealed class PortsRepoReleasesProvider : ReleaseProviderBase<PortEnum>
{
    /// <inheritdoc />
    public PortsRepoReleasesProvider(
        ILogger<PortsRepoReleasesProvider> logger,
        IHttpClientFactory httpClientFactory
        ) : base(new PortsRepositoriesProvider(), logger, httpClientFactory)
    {
    }
}
