using Core.All.Providers;

namespace Core.All.Interfaces;

/// <summary>
/// Maps entity enum values to their repository configuration for fetching releases.
/// </summary>
/// <typeparam name="T">Enum type identifying the entity (port, tool, app).</typeparam>
public interface IRepositoriesProvider<in T> where T : Enum
{
    /// <summary>
    /// Returns the repository configuration for the specified release enum.
    /// </summary>
    /// <param name="releaseEnum">Target release enum.</param>
    RepositoryEntity GetRepo(T releaseEnum);
}
