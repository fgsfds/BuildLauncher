using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for Blood.
/// </summary>
public enum BloodSkillsEnum : byte
{
    /// <summary>
    ///     Still Kicking.
    /// </summary>
    [Description("Still Kicking")]
    StillKicking = 0,

    /// <summary>
    ///     Pink on the Inside.
    /// </summary>
    [Description("Pink on the Inside")]
    PinkOnTheInside = 1,

    /// <summary>
    ///     Lightly Broiled.
    /// </summary>
    [Description("Lightly Broiled")]
    LightlyBroiled = 2,

    /// <summary>
    ///     Well Done.
    /// </summary>
    [Description("Well Done")]
    WellDone = 3,

    /// <summary>
    ///     Extra Crispy.
    /// </summary>
    [Description("Extra Crispy")]
    ExtraCrispy = 4
}
