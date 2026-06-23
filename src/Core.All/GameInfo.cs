using System.Diagnostics.CodeAnalysis;
using Core.All.Enums;

namespace Core.All;

public readonly struct GameInfo
{
    public required GameEnum GameEnum { get; init; }
    public required string? GameVersion { get; init; }
    public required string? GameCrc { get; init; }

    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum)
    {
        GameEnum = gameEnum;
        GameVersion = null;
        GameCrc = null;
    }

    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum, Enum gameVersion)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion.ToString();
        GameCrc = null;
    }

    [SetsRequiredMembers]
    public GameInfo(GameEnum gameEnum, string? gameVersion, string? gameCrc)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion;
        GameCrc = gameCrc;
    }
}
