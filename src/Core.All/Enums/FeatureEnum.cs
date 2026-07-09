namespace Core.All.Enums;

/// <summary>
///     Defines engine feature flags required by an addon.
/// </summary>
public enum FeatureEnum
{
    /// <summary>
    ///     EDuke32 CON script support.
    /// </summary>
    EDuke32_CON,

    /// <summary>
    ///     High-resolution texture support.
    /// </summary>
    Hightile,

    /// <summary>
    ///     Model rendering support.
    /// </summary>
    Models,

    /// <summary>
    ///     Sloped sprite support.
    /// </summary>
    Sloped_Sprites,

    /// <summary>
    ///     True Room Over Room support.
    /// </summary>
    TROR,

    /// <summary>
    ///     Wall rotate cstat support.
    /// </summary>
    Wall_Rotate_Cstat,

    /// <summary>
    ///     Dynamic lighting support.
    /// </summary>
    Dynamic_Lighting,

    /// <summary>
    ///     Modern type definitions support.
    /// </summary>
    Modern_Types,

    /// <summary>
    ///     Sound info definitions support.
    /// </summary>
    SndInfo,

    /// <summary>
    ///     Tile-from-texture support.
    /// </summary>
    TileFromTexture
}
