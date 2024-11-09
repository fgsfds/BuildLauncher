namespace Api.Common.Requests;

public abstract class BaseRequest
{
    public required Version ClientVersion { get; set; }
}
