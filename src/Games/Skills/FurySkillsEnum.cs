using System.ComponentModel;

namespace Games.Skills;

public enum FurySkillsEnum : byte
{
    [Description("First Blood")]
    FirstBlood = 0,

    [Description("Wanton Carnage")]
    WantonCarnage = 1,

    [Description("Ultra Viscera")]
    UltraViscera = 2,

    [Description("Maximum Fury")]
    MaximumFury = 3,

    [Description("Angel of Death")]
    AngelOfDeath = 4
}
