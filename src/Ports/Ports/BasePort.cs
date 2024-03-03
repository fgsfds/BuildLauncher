using Common.Helpers;
using Common.Enums;
using Ports.Providers;
using System.Collections.Immutable;
using Common.Interfaces;

namespace Ports.Ports
{
    /// <summary>
    /// Base class for ports
    /// </summary>
    public abstract class BasePort
    {
        /// <summary>
        /// Port enum
        /// </summary>
        public abstract PortEnum PortEnum { get; }

        /// <summary>
        /// Main executable
        /// </summary>
        public abstract string Exe { get; }

        /// <summary>
        /// Name of the port
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Name of the config file
        /// </summary>
        public abstract string ConfigFile { get; }

        /// <summary>
        /// Games supported by the port
        /// </summary>
        public abstract List<GameEnum> SupportedGames { get; }

        /// <summary>
        /// Url to the port repository
        /// </summary>
        public abstract Uri RepoUrl { get; }

        /// <summary>
        /// Get command line arguments to start custom map or campaign
        /// </summary>
        /// <param name="game">Game<param>
        /// <param name="mod">Map/campaign</param>
        public abstract string GetStartCampaignArgs(IGame game, IMod mod);

        /// <summary>
        /// Get command line arguments to load mods
        /// </summary>
        /// <param name="game">Game<param>
        /// <param name="mods">Mods</param>
        public abstract string GetAutoloadModsArgs(IGame game, ImmutableList<IMod> mods);

        /// <summary>
        /// Return command line parameter to skip intro
        /// </summary>
        public abstract string GetSkipIntroParameter();

        /// <summary>
        /// Predicate to find windows release
        /// </summary>
        public abstract Func<GitHubReleaseAsset, bool> WindowsReleasePredicate { get; }

        /// <summary>
        /// Currently installed version
        /// </summary>
        public abstract int? InstalledVersion { get; }

        /// <summary>
        /// Path to port install folder
        /// </summary>
        public string FolderPath => Path.Combine(CommonProperties.PortsFolderPath, PortFolder);

        /// <summary>
        /// Path to port exe
        /// </summary>
        public string FullPathToExe => Path.Combine(FolderPath, Exe);

        /// <summary>
        /// Is port installed
        /// </summary>
        public bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        /// Name of the folder that contains the port files
        /// By default is the same as <see cref="Name"/>
        /// </summary>
        public virtual string PortFolder => Name;

        /// <summary>
        /// Method to perform before starting the port
        /// </summary>
        /// <param name="game"></param>
        public virtual void BeforeStart(IGame game) { }
    }
}
