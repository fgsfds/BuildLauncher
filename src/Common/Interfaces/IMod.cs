﻿using Common.Enums;

namespace Common.Interfaces
{
    public interface IMod
    {
        /// <summary>
        /// Type of the mod
        /// </summary>
        ModTypeEnum ModType { get; init; }

        /// <summary>
        /// Name of the mod
        /// </summary>
        string DisplayName { get; init; }

        /// <summary>
        /// Mod's author
        /// </summary>
        string? Author { get; init; }

        /// <summary>
        /// Mod description
        /// </summary>
        string? Description { get; init; }

        /// <summary>
        /// Path to mod's file
        /// </summary>
        string? PathToFile { get; init; }

        /// <summary>
        /// Name of the mod file
        /// </summary>
        string? FileName { get; }

        /// <summary>
        /// Main startup file (CON, INI, MAP etc)
        /// </summary>
        string? StartupFile { get; init; }

        /// <summary>
        /// Cover image
        /// </summary>
        Stream? Image { get; init; }

        /// <summary>
        /// Ports that support this campaign
        /// if null - supported by all ports that support the game
        /// </summary>
        List<PortEnum>? SupportedPorts { get; init; }

        /// <summary>
        /// Url to the mod's website
        /// </summary>
        string? Url { get; init; }

        /// <summary>
        /// Mod's version
        /// </summary>
        float? Version { get; init; }

        /// <summary>
        /// Is official campaign
        /// </summary>
        bool IsOfficial { get; init; }

        /// <summary>
        /// Create markdown description of the mod
        /// </summary>
        string ToMarkdownString();
    }
}