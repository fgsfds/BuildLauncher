using Common.Entities;

namespace Api.Common.Responses;

public sealed class GetAddonsResponse
{
    public required List<DownloadableAddonEntity> AddonsList { get; set; }
}