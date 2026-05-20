using Common.All.Enums;

namespace Api.Common.Requests;

public sealed class GetAddonsRequest : BaseRequest
{
    public required GameEnum GameEnum { get; set; }
}