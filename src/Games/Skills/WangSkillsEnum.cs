using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for Shadow Warrior.
/// </summary>
public enum WangSkillsEnum : byte
{
    /// <summary>
    ///     Tiny Grasshopper.
    /// </summary>
    [Description("Tiny Grasshopper")]
    TinyGrasshopper = 1,

    /// <summary>
    ///     I Have No Fear.
    /// </summary>
    [Description("I Have No Fear")]
    IHaveNoFear = 2,

    /// <summary>
    ///     Who Wants Wang.
    /// </summary>
    [Description("Who Wants Wang")]
    WhoWantsWang = 3,

    /// <summary>
    ///     No Pain No Gain.
    /// </summary>
    [Description("No Pain No Gain")]
    NoPainNoGain = 4
}
