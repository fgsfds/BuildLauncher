using Core.All.Enums;
using Core.All.Helpers;
using Core.Client.Helpers;
using Core.Client.Interfaces;

namespace Tools.Tools;

/// <summary>
///     Base class for tools.
/// </summary>
public abstract class BaseTool : IInstallable
{
    /// <summary>
    ///     Tool enum.
    /// </summary>
    public abstract ToolEnum ToolEnum { get; }

    /// <summary>
    ///     Main executable
    /// </summary>
    public string Exe
    {
        get
        {
            return CommonProperties.OSEnum switch
            {
                OSEnum.Windows => WinExe,
                OSEnum.Linux => LinExe,
                _ => throw new ArgumentOutOfRangeException(CommonProperties.OSEnum.ToString())
            };
        }
    }

    /// <summary>
    ///     Windows executable.
    /// </summary>
    protected abstract string WinExe { get; }

    /// <summary>
    ///     Linux executable.
    /// </summary>
    protected abstract string LinExe { get; }

    /// <summary>
    ///     Name of the tool.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets a value indicating whether the tool can be installed.
    /// </summary>
    public virtual bool CanBeInstalled => true;

    /// <summary>
    ///     Gets a value indicating whether the tool can be launched.
    /// </summary>
    public abstract bool CanBeLaunched { get; }

    /// <summary>
    ///     Gets the install prompt text.
    /// </summary>
    public virtual string? InstallText => null;

    /// <summary>
    ///     Path to tool executable.
    /// </summary>
    public string ToolExeFilePath => Path.Combine(InstallFolderPath, Exe);

    /// <summary>
    ///     Tool's icon identifier.
    /// </summary>
    public long IconId => ToolEnum.GetUniqueHash();

    /// <inheritdoc />
    public bool IsInstalled => InstalledVersion is not null;

    /// <inheritdoc />
    public virtual string InstallFolderPath => Path.Combine(ClientProperties.ToolsFolderPath, Name);

    /// <inheritdoc />
    public virtual string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(InstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(versionFile);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Gets the command-line arguments for starting the tool.
    /// </summary>
    public abstract string GetStartToolArgs();
}
