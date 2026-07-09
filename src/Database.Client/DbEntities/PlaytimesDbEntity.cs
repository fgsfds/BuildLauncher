using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for addon playtimes.
/// </summary>
[Table(name: "playtimes", Schema = "main")]
public sealed class PlaytimesDbEntity
{
    /// <summary>
    ///     Addon identifier.
    /// </summary>
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    /// <summary>
    ///     Total playtime.
    /// </summary>
    [Column("playtime")]
    public required TimeSpan Playtime { get; set; }
}
