using CommunityToolkit.Diagnostics;

namespace Common.Enums.Skills;

public enum RedneckSkillsEnum : byte
{
    Wuss = 1,
    Meejum = 2,
    HardAss = 3,
    KillBilly = 4,
    Psychobilly = 5
}

public static class RedneckSkillsEnumHelper
{
    public static string ToReadableString(this RedneckSkillsEnum skill)
    {
        return skill switch
        {
            RedneckSkillsEnum.Wuss => "Wuss",
            RedneckSkillsEnum.Meejum => "Meejum",
            RedneckSkillsEnum.HardAss => "Hard Ass",
            RedneckSkillsEnum.KillBilly => "Killbilly",
            RedneckSkillsEnum.Psychobilly => "Psychobilly",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(skill))
        };
    }
}
