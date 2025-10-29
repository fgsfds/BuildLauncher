using Common.All.Enums;
using Common.All.Serializable.Downloadable;

namespace Common.All.Interfaces;

public interface IReleaseProvider<T> where T : Enum
{
    Task<Dictionary<OSEnum, GeneralReleaseJsonModel>?> GetLatestReleaseAsync(T e);
}
