namespace Api.Common.Requests;

public abstract class BaseRequest
{
    /// <summary>
    /// Requesting client version.
    /// </summary>
    public required Version ClientVersion { get; set; }
}
