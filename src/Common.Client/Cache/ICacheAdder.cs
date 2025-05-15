namespace Common.Client.Cache;

public interface ICacheAdder<T>
{
    void InitializeCache();

    bool TryAddToCache(long id, T item);

    bool TryAddGridToCache(long id, T item);

    bool TryAddPreviewToCache(long id, T item);

    bool TryRemoveFromCache(long id);
}
