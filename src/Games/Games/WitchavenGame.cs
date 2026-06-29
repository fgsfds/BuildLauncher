using Core.All.Enums;

namespace Games.Games;

/// <summary>
///     Represents the game Witchaven and its associated addon detection.
/// </summary>
public sealed class WitchavenGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Witchaven;

    /// <inheritdoc />
    public override string FullName => "Witchaven";

    /// <inheritdoc />
    public override string ShortName => FullName;

    /// <inheritdoc />
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result =
            [
                "JOESND",
                "SONGS"
            ];

            result.AddRange(GenerateNumberedFiles("TILES", "ART", 0, 11, 3));
            result.AddRange(GenerateNumberedFiles("LEVEL", "MAP", 1, 26, 0));

            return result;
        }
    }

    /// <summary>
    ///     Files required for Witchaven 2.
    /// </summary>
    public List<string> Witchaven2RequiredFiles
    {
        get
        {
            List<string> result =
            [
                "JOESND",
                "W_SONGS"
            ];

            result.AddRange(GenerateNumberedFiles("TILES", "ART", 0, 16, 3));
            result.AddRange(GenerateNumberedFiles("LEVEL", "MAP", 1, 16, 0));

            return result;
        }
    }

    /// <summary>
    ///     Path to Witchaven 2 install folder.
    /// </summary>
    public string? Witchaven2InstallPath { get; set; }

    /// <summary>
    ///     Is Witchaven 2 installed.
    /// </summary>
    public bool IsWitchaven2Installed => IsInstalled(Witchaven2RequiredFiles, Witchaven2InstallPath);

    /// <inheritdoc />
    public override Enum? Skills => null;
}
