using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for NAM.
/// </summary>
public enum NamSkillsEnum : byte
{
    /// <summary>
    ///     Boot.
    /// </summary>
    [Description("Boot")]
    Boot = 1,

    /// <summary>
    ///     Grunt.
    /// </summary>
    [Description("Grunt")]
    Grunt = 2,

    /// <summary>
    ///     Salty.
    /// </summary>
    [Description("Salty")]
    Salty = 3,

    /// <summary>
    ///     Locked On.
    /// </summary>
    [Description("Locked On")]
    LockedOn = 4
}
