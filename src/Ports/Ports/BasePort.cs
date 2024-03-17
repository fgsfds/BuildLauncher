using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Ports.Providers;
using System.Text;

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
        /// Games supported by the port
        /// </summary>
        public abstract List<GameEnum> SupportedGames { get; }

        /// <summary>
        /// Url to the port repository
        /// </summary>
        public abstract Uri RepoUrl { get; }

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
        public virtual string PathToPortFolder => Path.Combine(CommonProperties.PortsFolderPath, PortFolderName);

        /// <summary>
        /// Is port installed
        /// </summary>
        public virtual bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        /// Path to port exe
        /// </summary>
        public string FullPathToExe => Path.Combine(PathToPortFolder, Exe);


        /// <summary>
        /// Name of the config file
        /// </summary>
        protected abstract string ConfigFile { get; }

        /// <summary>
        /// Cmd parameter to add folder to search path
        /// </summary>
        protected abstract string AddDirectoryParam { get; }

        /// <summary>
        /// Cmd parameter to load additional file
        /// </summary>
        protected abstract string AddFileParam { get; }

        /// <summary>
        /// Cmd parameter to load additional Def file
        /// </summary>
        protected abstract string AddDefParam { get; }


        /// <summary>
        /// Name of the folder that contains the port files
        /// By default is the same as <see cref="Name"/>
        /// </summary>
        protected virtual string PortFolderName => Name;


        /// <summary>
        /// Get command line parameters to start the game with selected campaign and autoload mods
        /// </summary>
        /// <param name="game">Game<param>
        /// <param name="mod">Map/campaign</param>
        /// <param name="skipIntro">Skip intro</param>
        public string GetStartGameArgs(IGame game, IMod mod, bool skipIntro)
        {
            StringBuilder sb = new();

            BeforeStart(game, mod);

            GetStartCampaignArgs(sb, game, mod);

            GetAutoloadModsArgs(sb, game, mod, game.GetAutoloadMods(true));

            if (skipIntro)
            {
                GetSkipIntroParameter(sb);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Check if autoload mod works with current port and addon
        /// </summary>
        /// <param name="autoloadMod">Autoload mod</param>
        /// <param name="campaign">Campaign</param>
        protected bool ValidateAutoloadMod(AutoloadMod autoloadMod, IMod campaign)
        {
            if (!autoloadMod.IsEnabled)
            {
                //skipping disabled mods
                return false;
            }

            if (!autoloadMod.SupportedPorts?.Contains(PortEnum) ?? false)
            {
                //skipping mods not supported by the current port
                return false;
            }

            if (campaign.Addon is not null &&
                autoloadMod.SupportedAddons is not null &&
                !autoloadMod.SupportedAddons.Contains(campaign.Addon))
            {
                //skipping mods not supported by the current addon
                return false;
            }

            return true;
        }


        /// <summary>
        /// Method to perform before starting the port
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="campaign">Campaign</param>
        protected virtual void BeforeStart(IGame game, IMod campaign) { }

        /// <summary>
        /// Get command line arguments to start custom map or campaign
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        /// <param name="game">Game<param>
        /// <param name="mod">Map/campaign</param>
        protected abstract void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod);

        /// <summary>
        /// Get command line arguments to load mods
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        /// <param name="game">Game<param>
        /// <param name="campaign">Campaign\map<param>
        /// <param name="autoloadMods">Mods</param>
        protected abstract void GetAutoloadModsArgs(StringBuilder sb, IGame game, IMod campaign, Dictionary<Guid, IMod> autoloadMods);

        /// <summary>
        /// Return command line parameter to skip intro
        /// </summary>
        /// <param name="sb">String builder for parameters</param>
        protected abstract void GetSkipIntroParameter(StringBuilder sb);
    }
}
