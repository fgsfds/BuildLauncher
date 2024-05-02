using Common.Helpers;

namespace Tools.Tools
{
    /// <summary>
    /// Base class for ports
    /// </summary>
    public abstract class BaseTool
    {
        /// <summary>
        /// Main executable
        /// </summary>
        public abstract string Exe { get; }

        /// <summary>
        /// Name of the port
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Currently installed version
        /// </summary>
        public abstract string? InstalledVersion { get; }

        /// <summary>
        /// Url to the port repository
        /// </summary>
        public abstract Uri RepoUrl { get; }

        /// <summary>
        /// Predicate for Windows release
        /// </summary>
        public abstract Func<GitHubReleaseAsset, bool> WindowsReleasePredicate { get; }

        /// <summary>
        /// Path to port install folder
        /// </summary>
        public virtual string PathToToolFolder => Path.Combine(CommonProperties.ToolsFolderPath, ToolFolderName);

        /// <summary>
        /// Is port installed
        /// </summary>
        public virtual bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        /// Path to port exe
        /// </summary>
        public string FullPathToExe => Path.Combine(PathToToolFolder, Exe);

        /// <summary>
        /// Name of the folder that contains the port files
        /// By default is the same as <see cref="Name"/>
        /// </summary>
        protected virtual string ToolFolderName => Name;

        /// <summary>
        /// Port's icon
        /// </summary>
        public Stream Icon => ImageHelper.FileNameToStream($"{Name}.png");
    }
}
