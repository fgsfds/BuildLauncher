using System.ComponentModel;

namespace Games.Skills;

public enum NamSkillsEnum : byte
{
    [Description("Root")]
    Root = 0,

    [Description("Grunt")]
    Grunt = 1,

    [Description("Salty")]
    Salty = 2,

    [Description("Locked On")]
    LockedOn = 3
}
