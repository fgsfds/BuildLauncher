namespace Common.Enums.Skills;

public enum FurySkillsEnum : byte
{
    FirstBlood = 1,
    WantonCarnage = 2,
    UltraViscera = 3,
    MaximumFury = 4,
    AngelOfDeath = 5
}

public static class FurySkillsEnumHelper
{
    public static string ToReadableString(this FurySkillsEnum skill)
    {
        return skill switch
        {
            FurySkillsEnum.FirstBlood => "First Blood",
            FurySkillsEnum.WantonCarnage => "Wanton Carnage",
            FurySkillsEnum.UltraViscera => "Ultra Viscera",
            FurySkillsEnum.MaximumFury => "Maximum Fury",
            FurySkillsEnum.AngelOfDeath => "Angel of Death",
            _ => throw new NotImplementedException(),
        };
    }

}
