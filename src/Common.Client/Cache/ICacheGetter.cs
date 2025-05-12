namespace Common.Client.Cache;

public interface ICacheGetter<T>
{
    T? GetFromCache(long id);
}
