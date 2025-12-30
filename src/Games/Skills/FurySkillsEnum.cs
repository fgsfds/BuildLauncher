using System.ComponentModel;

namespace Games.Skills;

public enum FurySkillsEnum : byte
{
    [Description("First Blood")]
    FirstBlood = 1,

    [Description("Wanton Carnage")]
    WantonCarnage = 2,

    [Description("Ultra Viscera")]
    UltraViscera = 3,

    [Description("Maximum Fury")]
    MaximumFury = 4,

    [Description("Angel of Death")]
    AngelOfDeath = 5
}
