namespace Mods.Mods
{
    public sealed class AutoloadMod : BaseMod
    {
        /// <summary>
        /// Is mod enabled
        /// </summary>
        public required bool IsEnabled { get; set; }

        /// <summary>
        /// Addons that support this mod
        /// </summary>
        public required List<string>? SupportedAddons { get; set; }
    }
}
