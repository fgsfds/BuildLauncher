using Common.Entities;

namespace Api.Common.Responses;

public sealed class GetAddonsResponse
{
    public List<DownloadableAddonEntity> AddonsList { get; set; }
}