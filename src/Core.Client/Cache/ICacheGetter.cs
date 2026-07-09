namespace Core.Client.Cache;

/// <summary>
///     Defines a method for retrieving items from the cache by identifier.
/// </summary>
/// <typeparam name="T">The type of items stored in the cache.</typeparam>
public interface ICacheGetter<T>
{
    /// <summary>
    ///     Retrieves an item from the cache by its identifier.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <returns>The cached item, or null if not found.</returns>
    T? GetFromCache(long id);
}
