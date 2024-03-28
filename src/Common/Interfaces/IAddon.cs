using Common.Enums;

namespace Common.Interfaces
{
    public interface IAddon
    {
        /// <summary>
        /// Mod's ID
        /// </summary>
        string Id { get; init; }

        GameEnum Game { get; init; }

        List<int>? GameCrcs { get; init; }

        /// <summary>
        /// Type of the mod
        /// </summary>
        ModTypeEnum ModType { get; init; }

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
        List<PortEnum>? SupportedPorts { get; init; }

        /// <summary>
        /// Mod's version
        /// </summary>
        string? Version { get; init; }

        /// <summary>
        /// Is loose unarchived file
        /// </summary>
        bool IsLoose { get; init; }

        /// <summary>
        /// Built-in def file
        /// </summary>
        string? DefFileContents { get; init; }

        HashSet<string>? Dependencies { get; init; }
        HashSet<string>? Incompatibles { get; init; }

        /// <summary>
        /// Create markdown description of the mod
        /// </summary>
        string ToMarkdownString();
    }
}