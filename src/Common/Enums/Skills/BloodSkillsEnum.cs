namespace Common.Enums.Skills;

public enum BloodSkillsEnum : byte
{
    StillKicking = 1,
    PinkOnTheInside = 2,
    LightlyBroiled = 3,
    WellDone = 4,
    ExtraCrispy = 5
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
            BloodSkillsEnum.ExtraCrispy => "Extra Crisry",
            _ => throw new NotImplementedException(),
        };
    }

}
