namespace Tests.Unit.Helpers;

/// <summary>
///     Shared string constants used when building expected command-line arguments in tests.
/// </summary>
internal static class CmdArgsTestData
{
    /// <summary>
    ///     Enabled mod zip file name.
    /// </summary>
    public const string EnabledMod = "enabled_mod.zip";

    /// <summary>
    ///     Mod that requires the official addon.
    /// </summary>
    public const string ModRequiresAddon = "mod_requires_addon.zip";

    /// <summary>
    ///     Mod that is incompatible with the official addon.
    /// </summary>
    public const string ModIncompatibleWithAddon = "mod_incompatible_with_addon.zip";

    /// <summary>
    ///     Incompatible mod with a compatible version.
    /// </summary>
    public const string IncompatibleModWithCompatibleVersion = "incompatible_mod_with_compatible_version.zip";

    /// <summary>
    ///     Dependent mod.
    /// </summary>
    public const string DependentMod = "dependent_mod.zip";

    /// <summary>
    ///     Dependent mod with a compatible version.
    /// </summary>
    public const string DependentModWithCompatibleVersion = "dependent_mod_with_compatible_version.zip";

    /// <summary>
    ///     Mod that requires a supported feature.
    /// </summary>
    public const string FeatureMod = "feature_mod.zip";

    /// <summary>
    ///     First enabled DEF file.
    /// </summary>
    public const string EnabledDef1 = "ENABLED1.DEF";

    /// <summary>
    ///     Second enabled DEF file.
    /// </summary>
    public const string EnabledDef2 = "ENABLED2.DEF";

    /// <summary>
    ///     First enabled CON file.
    /// </summary>
    public const string EnabledCon1 = "ENABLED1.CON";

    /// <summary>
    ///     Second enabled CON file.
    /// </summary>
    public const string EnabledCon2 = "ENABLED2.CON";

    /// <summary>
    ///     Main CON file of a total conversion.
    /// </summary>
    public const string TcCon = "TC.CON";

    /// <summary>
    ///     Main DEF file of a total conversion.
    /// </summary>
    public const string TcDef = "TC.DEF";

    /// <summary>
    ///     First additional CON file of a total conversion.
    /// </summary>
    public const string TcCon1 = "TC1.CON";

    /// <summary>
    ///     Second additional CON file of a total conversion.
    /// </summary>
    public const string TcCon2 = "TC2.CON";

    /// <summary>
    ///     First additional DEF file of a total conversion.
    /// </summary>
    public const string TcDef1 = "TC1.DEF";

    /// <summary>
    ///     Second additional DEF file of a total conversion.
    /// </summary>
    public const string TcDef2 = "TC2.DEF";

    /// <summary>
    ///     RTS file of a total conversion.
    /// </summary>
    public const string TcRts = "TC.RTS";

    /// <summary>
    ///     Default game CON file.
    /// </summary>
    public const string GameCon = "GAME.CON";

    /// <summary>
    ///     Override main DEF value.
    /// </summary>
    public const string OverrideDef = "a";

    /// <summary>
    ///     Loose map file name.
    /// </summary>
    public const string LooseMap = "LOOSE.MAP";
}
