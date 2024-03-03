using Common.Enums;
using System.Collections.Immutable;

namespace Common.Interfaces
{
    public interface IGame
    {
        /// <summary>
        /// Full name of the game
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Short name of the game
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Game enum
        /// </summary>
        GameEnum GameEnum { get; }

        /// <summary>
        /// Main startup file (GRP, RFF etc)
        /// </summary>
        string MainFile { get; }

        /// <summary>
        /// Main startup file ()
        /// </summary>
        string StartupFile { get; }

        /// <summary>
        /// Game install folder
        /// </summary>
        string GameInstallFolder { get; set; }

        /// <summary>
        /// Is base game installed
        /// </summary>
        bool IsBaseGameInstalled { get; }

        /// <summary>
        /// List of files required for the base game to work
        /// </summary>
        List<string> RequiredFiles { get; }

        /// <summary>
        /// Path to custom campaigns folder
        /// </summary>
        string CampaignsFolderPath { get; }

        /// <summary>
        /// Path to custom maps folder
        /// </summary>
        string MapsFolderPath { get; }

        /// <summary>
        /// Path to autoload mods folder
        /// </summary>
        string ModsFolderPath { get; }

        /// <summary>
        /// Path to special folder
        /// </summary>
        string SpecialFolderPath { get; }


        /// <summary>
        /// Create combined autoload mod
        /// </summary>
        void CreateCombinedMod();

        /// <summary>
        /// Get list of official addons and custom campaigns
        /// </summary>
        ImmutableList<IMod> GetCampaigns();

        /// <summary>
        /// Get list of custom maps
        /// </summary>
        ImmutableList<IMod> GetSingleMaps();

        /// <summary>
        /// Get list of autoload mods
        /// </summary>
        ImmutableList<IMod> GetAutoloadMods();
    }
}