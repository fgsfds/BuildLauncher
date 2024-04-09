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
        ModTypeEnum Type { get; init; }

        HashSet<GameEnum>? SupportedGames { get; init; }

        HashSet<int>? SupportedGamesCrcs { get; init; }

        public bool IsEnabled { get; init; }

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

        bool IsAvailable { get; set; }

        Dictionary<string, string?>? Dependencies { get; init; }

        Dictionary<string, string?>? Incompatibles { get; init; }

        //string? MainCon { get; init; }

        string? MainDef { get; init; }

        //HashSet<string>? AdditionalCons { get; init; }

        HashSet<string>? AdditionalDefs { get; init; }

        //string? RTS { get; init; }
        //string? INI { get; init; }
        //string? RFF { get; init; }
        //string? SND { get; init; }

        IStartMap? StartMap { get; init; }


        /// <summary>
        /// Create markdown description of the mod
        /// </summary>
        string ToMarkdownString();
    }
}