using Common.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Common;

public readonly struct GameStruct
{
    public required GameEnum GameEnum { get; init; }
    public required string? GameVersion { get; init; }
    public required string? GameCrc { get; init; }

    [SetsRequiredMembers]
    public GameStruct(GameEnum gameEnum)
    {
        GameEnum = gameEnum;
        GameVersion = null;
        GameCrc = null;
    }

    [SetsRequiredMembers]
    public GameStruct(GameEnum gameEnum, Enum? gameVersion)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion?.ToString();
        GameCrc = null;
    }

    [SetsRequiredMembers]
    public GameStruct(GameEnum gameEnum, string? gameVersion, string? gameCrc)
    {
        GameEnum = gameEnum;
        GameVersion = gameVersion;
        GameCrc = gameCrc;
    }
}
