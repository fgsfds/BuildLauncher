using CommunityToolkit.Diagnostics;

namespace Common.Enums.Skills;

public enum Duke3DSkillsEnum : byte
{
    PieceOfcake = 1,
    LetsRock = 2,
    ComeGetSome = 3,
    DamnImGood = 4
}

public static class Duke3DSkillsEnumHelper
{
    public static string ToReadableString(this Duke3DSkillsEnum skill)
    {
        return skill switch
        {
            Duke3DSkillsEnum.PieceOfcake => "Piece of Cake",
            Duke3DSkillsEnum.LetsRock => "Let's Rock",
            Duke3DSkillsEnum.ComeGetSome => "Come Get Some",
            Duke3DSkillsEnum.DamnImGood => "Damn I'm Good",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(skill))
        };
    }
}
