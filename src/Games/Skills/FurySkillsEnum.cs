using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for Ion Fury.
/// </summary>
public enum FurySkillsEnum : byte
{
    /// <summary>
    ///     First Blood.
    /// </summary>
    [Description("First Blood")]
    FirstBlood = 1,

    /// <summary>
    ///     Wanton Carnage.
    /// </summary>
    [Description("Wanton Carnage")]
    WantonCarnage = 2,

    /// <summary>
    ///     Ultra Viscera.
    /// </summary>
    [Description("Ultra Viscera")]
    UltraViscera = 3,

    /// <summary>
    ///     Maximum Fury.
    /// </summary>
    [Description("Maximum Fury")]
    MaximumFury = 4,

    /// <summary>
    ///     Angel of Death.
    /// </summary>
    [Description("Angel of Death")]
    AngelOfDeath = 5
}
