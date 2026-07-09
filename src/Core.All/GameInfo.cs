using System.Diagnostics.CodeAnalysis;
using Core.All.Enums;

namespace Core.All;

/// <summary>
///     Identifies a game and its version for addon compatibility checks.
/// </summary>
public readonly struct GameInfo
{
    /// <summary>
    ///     Gets the game identifier.
    /// </summary>
    public required GameEnum GameEnum { get; init; }

    /// <summary>
    ///     Gets the optional game version string.
    /// </summary>
    public required string? GameVersion { get; init; }

    /// <summary>
    ///     Gets the optional game CRC hash.
    /// </summary>
    public required string? GameCrc { get; init; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameInfo" /> struct with only a game identifier.
    /// </summary>
    /// <param name="gameEnum">Game identifier.</param>
    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum)
    {
        GameEnum = gameEnum;
        GameVersion = null;
        GameCrc = null;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameInfo" /> struct with a game identifier and version.
    /// </summary>
    /// <param name="gameEnum">Game identifier.</param>
    /// <param name="gameVersion">Game version.</param>
    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum, Enum gameVersion)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion.ToString();
        GameCrc = null;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameInfo" /> struct.
    /// </summary>
    /// <param name="gameEnum">Game identifier.</param>
    /// <param name="gameVersion">Optional game version.</param>
    /// <param name="gameCrc">Optional game CRC hash.</param>
    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum, string? gameVersion, string? gameCrc)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion;
        GameCrc = gameCrc;
    }
}
