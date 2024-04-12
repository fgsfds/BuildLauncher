using Common.Enums;

namespace Common.Interfaces
{
    public interface IAddon
    {
        /// <summary>
        /// Mod's ID
        /// </summary>
        string Id { get; init; }

        /// <summary>
        /// Type of the addon
        /// </summary>
        AddonTypeEnum Type { get; init; }

        /// <summary>
        /// List of supported games
        /// </summary>
        HashSet<GameEnum>? SupportedGames { get; init; }

        /// <summary>
        /// List of supported games CRCs
        /// </summary>
        HashSet<int>? SupportedGamesCrcs { get; init; }

        /// <summary>
        /// Name of the mod
        /// </summary>
        string Title { get; init; }

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
        /// Cover image
        /// </summary>
        Stream? Image { get; init; }

        /// <summary>
        /// Ports that support this campaign
        /// if null - supported by all ports that support the game
        /// </summary>
        HashSet<PortEnum>? SupportedPorts { get; init; }

        /// <summary>
        /// Mod's version
        /// </summary>
        string? Version { get; init; }

        /// <summary>
        /// List of mods that the current mod depends on
        /// </summary>
        Dictionary<string, string?>? Dependencies { get; init; }

        /// <summary>
        /// List of mods that the current mod it incompatible with
        /// </summary>
        Dictionary<string, string?>? Incompatibles { get; init; }

        /// <summary>
        /// Does the mod have all dependencies and incompatibles in check
        /// </summary>
        bool IsAvailable { get; set; }

        /// <summary>
        /// Main def file
        /// </summary>
        string? MainDef { get; init; }

        /// <summary>
        /// Additional def files
        /// </summary>
        HashSet<string>? AdditionalDefs { get; init; }

        /// <summary>
        /// Map that will be started when the mod is loaded
        /// </summary>
        IStartMap? StartMap { get; init; }


        /// <summary>
        /// Create markdown description of the mod
        /// </summary>
        string ToMarkdownString();
    }
}