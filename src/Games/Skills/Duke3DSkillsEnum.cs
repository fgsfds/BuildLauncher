using System.ComponentModel;

namespace Games.Skills;

public enum Duke3DSkillsEnum : byte
{
    [Description("Piece of Cake")]
    PieceOfCake = 0,

    [Description("Let's Rock")]
    LetsRock = 1,

    [Description("Come Get Some")]
    ComeGetSome = 2,

    [Description("Damn I'm Good")]
    DamnImGood = 3
}
