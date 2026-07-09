namespace Core.Client.Cache;

/// <summary>
///     Defines methods for adding, removing, and initializing cache entries.
/// </summary>
/// <typeparam name="T">The type of items stored in the cache.</typeparam>
public interface ICacheAdder<T>
{
    /// <summary>
    ///     Initializes the cache store.
    /// </summary>
    void InitializeCache();

    /// <summary>
    ///     Attempts to add an item to the cache by its identifier.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="item">The item to cache.</param>
    /// <returns>true if the item was added successfully; otherwise, false.</returns>
    bool TryAddToCache(long id, T item);

    /// <summary>
    ///     Attempts to add a grid image to the cache by its identifier.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="item">The grid image data to cache.</param>
    /// <returns>true if the grid image was added successfully; otherwise, false.</returns>
    bool TryAddGridToCache(long id, T item);

    /// <summary>
    ///     Attempts to add a preview image to the cache by its identifier.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="item">The preview image data to cache.</param>
    /// <returns>true if the preview image was added successfully; otherwise, false.</returns>
    bool TryAddPreviewToCache(long id, T item);

    /// <summary>
    ///     Attempts to remove an item from the cache by its identifier.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <returns>true if the item was removed successfully; otherwise, false.</returns>
    bool TryRemoveFromCache(long id);
}
