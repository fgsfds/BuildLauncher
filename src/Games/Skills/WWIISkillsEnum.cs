using System.ComponentModel;

namespace Games.Skills;

public enum WWIISkillsEnum : byte
{
    [Description("Draftee")]
    Draftee = 0,

    [Description("GI")]
    GI = 1,

    [Description("Paratrooper")]
    Paratrooper = 2,

    [Description("Veteran")]
    Veteran = 3
}
