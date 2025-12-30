using System.ComponentModel;

namespace Games.Skills;

public enum Duke3DSkillsEnum : byte
{
    [Description("Piece of Cake")]
    PieceOfCake = 1,

    [Description("Let's Rock")]
    LetsRock = 2,

    [Description("Come Get Some")]
    ComeGetSome = 3,

    [Description("Damn I'm Good")]
    DamnImGood = 4
}
