using System.ComponentModel;

namespace Games.Skills;

/// <summary>
///     Skill levels for World War II GI.
/// </summary>
public enum WWIISkillsEnum : byte
{
    /// <summary>
    ///     Draftee.
    /// </summary>
    [Description("Draftee")]
    Draftee = 1,

    /// <summary>
    ///     GI.
    /// </summary>
    [Description("GI")]
    GI = 2,

    /// <summary>
    ///     Paratrooper.
    /// </summary>
    [Description("Paratrooper")]
    Paratrooper = 3,

    /// <summary>
    ///     Veteran.
    /// </summary>
    [Description("Veteran")]
    Veteran = 4
}
