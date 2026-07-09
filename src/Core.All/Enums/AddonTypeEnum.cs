namespace Core.All.Enums;

/// <summary>
///     Defines the type of an addon.
/// </summary>
public enum AddonTypeEnum : byte
{
    /// <summary>
    ///     Official addon.
    /// </summary>
    Official = 0,

    /// <summary>
    ///     Total conversion addon.
    /// </summary>
    TC = 1,

    /// <summary>
    ///     Map addon.
    /// </summary>
    Map = 2,

    /// <summary>
    ///     Mod addon.
    /// </summary>
    Mod = 3
}
