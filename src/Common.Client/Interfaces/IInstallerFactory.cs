namespace Common.Client.Interfaces;

public interface IInstallerFactory<T, out TInstaller>
    where T : IInstallable
    where TInstaller : InstallerBase<T>
{
    TInstaller Create(T instanceEnum);
}