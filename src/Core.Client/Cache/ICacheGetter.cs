namespace Core.Client.Cache;

public interface ICacheGetter<T>
{
    T? GetFromCache(long id);
}
