using System.Diagnostics.CodeAnalysis;
using Common.Enums;

namespace Common;

public readonly struct GameStruct
{
    public GameEnum GameEnum { get; }
    public string? GameVersion { get; }
    public string? GameCrc { get; }

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
