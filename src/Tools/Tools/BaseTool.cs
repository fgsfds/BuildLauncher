using Common.All.Enums;
using Common.All.Helpers;
using Common.Client.Helpers;
using Common.Client.Interfaces;

namespace Tools.Tools;

/// <summary>
/// Base class for tools
/// </summary>
public abstract class BaseTool : IInstallable
{
    /// <summary>
    /// Tool enum
    /// </summary>
    public abstract ToolEnum ToolEnum { get; }

    /// <summary>
    /// Main executable
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
    /// Windows executable
    /// </summary>
    protected abstract string WinExe { get; }

    /// <summary>
    /// Linux executable
    /// </summary>
    protected abstract string LinExe { get; }

    /// <summary>
    /// Name of the tool
    /// </summary>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public bool IsInstalled => InstalledVersion is not null;

    /// <inheritdoc/>
    public virtual string InstallFolderPath => Path.Combine(ClientProperties.ToolsFolderPath, Name);

    /// <inheritdoc/>
    public virtual string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(InstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            return File.ReadAllText(versionFile);
        }
    }

    /// <summary>
    /// Can tool be installed
    /// </summary>
    public virtual bool CanBeInstalled => true;

    /// <summary>
    /// Can tool be launched
    /// </summary>
    public abstract bool CanBeLaunched { get; }

    public virtual string? InstallText => null;

    /// <summary>
    /// Path to tool exe
    /// </summary>
    public string ToolExeFilePath => Path.Combine(InstallFolderPath, Exe);

    /// <summary>
    /// Tool's icon
    /// </summary>
    public long IconId => ToolEnum.GetUniqueHash();

    /// <summary>
    /// Get cmd arguments
    /// </summary>
    public abstract string GetStartToolArgs();
}
