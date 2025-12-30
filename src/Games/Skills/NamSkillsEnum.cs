using System.ComponentModel;

namespace Games.Skills;

public enum NamSkillsEnum : byte
{
    [Description("Boot")]
    Boot = 1,

    [Description("Grunt")]
    Grunt = 2,

    [Description("Salty")]
    Salty = 3,

    [Description("Locked On")]
    LockedOn = 4
}
