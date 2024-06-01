namespace Common.Enums.Skills;

public enum WangSkillsEnum : byte
{
    TinyGrasshopper = 1,
    IHaveNoFear = 2,
    WhoWantsWang = 3,
    NoPainNoGain = 4
}

public static class WangSkillsEnumHelper
{
    public static string ToReadableString(this WangSkillsEnum skill)
    {
        return skill switch
        {
            WangSkillsEnum.TinyGrasshopper => "Tiny Grasshopper",
            WangSkillsEnum.IHaveNoFear => "I Have No Fear",
            WangSkillsEnum.WhoWantsWang => "Who Wants Wang",
            WangSkillsEnum.NoPainNoGain => "No Pain No Gain",
            _ => throw new NotImplementedException(),
        };
    }

}
