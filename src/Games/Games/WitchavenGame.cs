using Common.All.Enums;
using Games.Skills;

namespace Games.Games;

public sealed class WitchavenGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Witchaven;

    /// <inheritdoc/>
    public override string FullName => "Witchaven";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result = ["JOESND", "SONGS"];

            for (var i = 0; i < 11; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            for (var i = 1; i < 26; i++)
            {
                result.Add($"LEVEL{i}.MAP");
            }

            return result;
        }
    }

    /// <summary>
    /// Files required for Witchaven 2
    /// </summary>
    public List<string> Witchaven2RequiredFiles
    {
        get
        {
            List<string> result = ["JOESND", "W_SONGS"];

            for (var i = 0; i < 16; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            for (var i = 1; i < 16; i++)
            {
                result.Add($"LEVEL{i}.MAP");
            }

            return result;
        }
    }

    /// <summary>
    /// Path to Witchaven 2 install folder
    /// </summary>
    public string? Witchaven2InstallPath { get; set; }

    /// <summary>
    /// Is Witchaven 2 installed
    /// </summary>
    public bool IsWitchaven2Installed => IsInstalled(Witchaven2RequiredFiles, Witchaven2InstallPath);

    /// <inheritdoc/>
    public override Enum? Skills => null;
}
