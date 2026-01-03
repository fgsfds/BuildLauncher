using Common.All.Enums;

namespace Common.All.Interfaces;

public interface IReleaseProvider<T> where T : Enum
{
    Task<Dictionary<OSEnum, GeneralRelease>?> GetLatestReleaseAsync(T e);
}
