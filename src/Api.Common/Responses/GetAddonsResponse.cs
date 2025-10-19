using Common.All.Serializable.Downloadable;

namespace Api.Common.Responses;

public sealed class GetAddonsResponse
{
    public required List<DownloadableAddonJsonModel> AddonsList { get; set; }
}