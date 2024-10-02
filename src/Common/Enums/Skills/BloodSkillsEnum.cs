using CommunityToolkit.Diagnostics;

namespace Common.Enums.Skills;

public enum BloodSkillsEnum : byte
{
    StillKicking = 0,
    PinkOnTheInside = 1,
    LightlyBroiled = 2,
    WellDone = 3,
    ExtraCrispy = 4
}

public static class BloodSkillsEnumHelper
{
    public static string ToReadableString(this BloodSkillsEnum skill)
    {
        return skill switch
        {
            BloodSkillsEnum.StillKicking => "Still Kicking",
            BloodSkillsEnum.PinkOnTheInside => "Pink on the Inside",
            BloodSkillsEnum.LightlyBroiled => "Lightly Broiled",
            BloodSkillsEnum.WellDone => "Well Done",
            BloodSkillsEnum.ExtraCrispy => "Extra Crispy",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(skill))
        };
    }

}
