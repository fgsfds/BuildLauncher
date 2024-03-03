namespace Mods.Mods
{
    /// <summary>
    /// Custom map
    /// </summary>
    public sealed class SingleMap : BaseMod
    {
        /// <summary>
        /// Name of the map file
        /// </summary>
        public required string MapFile { get; init; }
    }
}
