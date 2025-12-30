using System.ComponentModel;

namespace Games.Skills;

public enum WWIISkillsEnum : byte
{
    [Description("Draftee")]
    Draftee = 1,

    [Description("GI")]
    GI = 2,

    [Description("Paratrooper")]
    Paratrooper = 3,

    [Description("Veteran")]
    Veteran = 4
}
