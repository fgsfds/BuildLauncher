using System.ComponentModel;

namespace Games.Skills;

public enum BloodSkillsEnum : byte
{
    [Description("Still Kicking")]
    StillKicking = 0,

    [Description("Pink on the Inside")]
    PinkOnTheInside = 1,

    [Description("Lightly Broiled")]
    LightlyBroiled = 2,

    [Description("Well Done")]
    WellDone = 3,

    [Description("Extra Crispy")]
    ExtraCrispy = 4
}
