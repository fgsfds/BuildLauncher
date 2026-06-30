using Core.All.Enums;
using Core.All.Releases;
using Microsoft.Extensions.Logging;

namespace Tools.Releases;

/// <summary>
///     Provides the latest GitHub releases for editor/mapping tools.
/// </summary>
public sealed class ToolsRepoReleasesProvider : ReleaseProviderBase<ToolEnum>
{
    /// <inheritdoc />
    public ToolsRepoReleasesProvider(
        ILogger<ToolsRepoReleasesProvider> logger,
        IHttpClientFactory httpClientFactory
        ) : base(new ToolsRepositoriesProvider(), logger, httpClientFactory) { }
}
