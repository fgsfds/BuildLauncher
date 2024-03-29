﻿using Common.Enums;

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
        /// Def file name
        /// </summary>
        string DefFile { get; }

        /// <summary>
        /// Game install folder
        /// </summary>
        string? GameInstallFolder { get; set; }

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

        IInstalledModsProvider InstalledModsProvider { get; init; }

        IDownloadableModsProvider DownloadableModsProvider { get; init; }


        /// <summary>
        /// Create combined autoload mod
        /// </summary>
        /// <param name="additionalDef">Additional text for def</param>
        void CreateCombinedMod(string? additionalDef = null);

        /// <summary>
        /// Get list of official addons and custom campaigns
        /// </summary>
        Dictionary<Guid, IMod> GetCampaigns();

        /// <summary>
        /// Get list of custom maps
        /// </summary>
        Dictionary<Guid, IMod> GetSingleMaps();

        /// <summary>
        /// Get list of autoload mods
        /// </summary>
        Dictionary<Guid, IMod> GetAutoloadMods(bool enabledOnly);
    }
}