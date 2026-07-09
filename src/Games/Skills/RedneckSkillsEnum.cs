using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for Redneck Rampage.
/// </summary>
public enum RedneckSkillsEnum : byte
{
    /// <summary>
    ///     Wuss.
    /// </summary>
    [Description("Wuss")]
    Wuss = 1,

    /// <summary>
    ///     Meejum.
    /// </summary>
    [Description("Meejum")]
    Meejum = 2,

    /// <summary>
    ///     Hard Ass.
    /// </summary>
    [Description("Hard Ass")]
    HardAss = 3,

    /// <summary>
    ///     Killbilly.
    /// </summary>
    [Description("Killbilly")]
    KillBilly = 4

    //Can't be started from the command line
    //[Description("Psychobilly")]
    //Psychobilly = 5
}
