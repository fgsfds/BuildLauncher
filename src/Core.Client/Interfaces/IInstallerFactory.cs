namespace Core.Client.Interfaces;

/// <summary>
///     Defines a factory for creating installer instances for a given installable item.
/// </summary>
/// <typeparam name="T">The type of installable item.</typeparam>
/// <typeparam name="TInstaller">The type of installer to create.</typeparam>
public interface IInstallerFactory<T, out TInstaller>
    where T : IInstallable
    where TInstaller : InstallerBase<T>
{
    /// <summary>
    ///     Creates an installer for the specified installable instance.
    /// </summary>
    /// <param name="instanceEnum">The installable item.</param>
    /// <returns>An installer instance.</returns>
    TInstaller Create(T instanceEnum);
}
