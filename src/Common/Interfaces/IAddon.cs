using Common.Enums;

namespace Common.Interfaces
{
    public interface IAddon
    {
        /// <summary>
        /// Addon ID
        /// </summary>
        string Id { get; init; }

        /// <summary>
        /// Type of the addon
        /// </summary>
        AddonTypeEnum Type { get; init; }

        /// <summary>
        /// Port features required to run addon
        /// </summary>
        HashSet<FeatureEnum>? RequiredFeatures { get; init; }

        /// <summary>
        /// List of supported games
        /// </summary>
        HashSet<GameEnum>? SupportedGames { get; init; }

        /// <summary>
        /// List of required games CRCs
        /// </summary>
        HashSet<int>? RequiredGamesCrcs { get; init; }

        /// <summary>
        /// Name of the addon
        /// </summary>
        string Title { get; init; }

        /// <summary>
        /// Addon author
        /// </summary>
        string? Author { get; init; }

        /// <summary>
        /// Addon description
        /// </summary>
        string? Description { get; init; }

        /// <summary>
        /// Path to addon file
        /// </summary>
        string? PathToFile { get; init; }

        /// <summary>
        /// Name of the addon file
        /// </summary>
        string? FileName { get; }

        /// <summary>
        /// Cover image
        /// </summary>
        Stream? Image { get; init; }

        /// <summary>
        /// Preview image
        /// </summary>
        Stream? Preview { get; init; }

        /// <summary>
        /// Ports that support this campaign
        /// if null - supported by all ports that support the game
        /// </summary>
        HashSet<PortEnum>? SupportedPorts { get; init; }

        /// <summary>
        /// Addon version
        /// </summary>
        string? Version { get; init; }

        /// <summary>
        /// List of addons that the current addon requires to work
        /// </summary>
        Dictionary<string, string?>? Dependencies { get; init; }

        /// <summary>
        /// List of addons that the current addon is incompatible with
        /// </summary>
        Dictionary<string, string?>? Incompatibles { get; init; }

        /// <summary>
        /// Main def file
        /// </summary>
        string? MainDef { get; init; }

        /// <summary>
        /// Additional def files
        /// </summary>
        HashSet<string>? AdditionalDefs { get; init; }

        /// <summary>
        /// Map that will be started when the addon is loaded
        /// </summary>
        IStartMap? StartMap { get; init; }

        /// <summary>
        /// Time played
        /// </summary>
        TimeSpan Playtime { get; set; }


        /// <summary>
        /// Create markdown description of the addon
        /// </summary>
        string ToMarkdownString();

        /// <summary>
        /// Add time to current playtime
        /// </summary>
        /// <param name="time">Time to add</param>
        void UpdatePlaytime(TimeSpan time);
    }
}