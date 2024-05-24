using ClientCommon.Helpers;
using Common.Enums;
using Common.Helpers;
using System.Reflection;

namespace Tools.Tools
{
    /// <summary>
    /// Base class for tools
    /// </summary>
    public abstract class BaseTool
    {
        /// <summary>
        /// Main executable
        /// </summary>
        public abstract string Exe { get; }

        /// <summary>
        /// Name of the tool
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Get cmd arguments
        /// </summary>
        public abstract string GetStartToolArgs();

        /// <summary>
        /// Port enum
        /// </summary>
        public abstract ToolEnum ToolEnum { get; }


        /// <summary>
        /// Path to tool install folder
        /// </summary>
        public virtual string PathToExecutableFolder => Path.Combine(ClientProperties.ToolsFolderPath, ToolFolderName);

        /// <summary>
        /// Is tool installed
        /// </summary>
        public virtual bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        /// Name of the folder that contains the tool files
        /// By default is the same as <see cref="Name"/>
        /// </summary>
        protected virtual string ToolFolderName => Name;

        /// <summary>
        /// Currently installed version
        /// </summary>
        public virtual string? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToExecutableFolder, "version");

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
        public string FullPathToExe => Path.Combine(PathToExecutableFolder, Exe);

        /// <summary>
        /// Tool's icon
        /// </summary>
        public Stream Icon => ImageHelper.FileNameToStream($"{Name}.png", Assembly.GetExecutingAssembly());
    }
}
