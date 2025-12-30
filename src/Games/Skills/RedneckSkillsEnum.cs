using System.ComponentModel;

namespace Games.Skills;

public enum RedneckSkillsEnum : byte
{
    [Description("Wuss")]
    Wuss = 1,

    [Description("Meejum")]
    Meejum = 2,

    [Description("Hard Ass")]
    HardAss = 3,

    [Description("Killbilly")]
    KillBilly = 4,

    //Can't be started from the command line
    //[Description("Psychobilly")]
    //Psychobilly = 5
}
