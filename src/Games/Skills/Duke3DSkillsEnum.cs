using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for Duke Nukem 3D.
/// </summary>
public enum Duke3DSkillsEnum : byte
{
    /// <summary>
    ///     Piece of Cake.
    /// </summary>
    [Description("Piece of Cake")]
    PieceOfCake = 1,

    /// <summary>
    ///     Let's Rock.
    /// </summary>
    [Description("Let's Rock")]
    LetsRock = 2,

    /// <summary>
    ///     Come Get Some.
    /// </summary>
    [Description("Come Get Some")]
    ComeGetSome = 3,

    /// <summary>
    ///     Damn I'm Good.
    /// </summary>
    [Description("Damn I'm Good")]
    DamnImGood = 4
}
