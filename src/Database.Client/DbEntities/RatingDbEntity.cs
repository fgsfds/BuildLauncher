using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for addon ratings.
/// </summary>
[Table(name: "rating", Schema = "main")]
public sealed class RatingDbEntity
{
    /// <summary>
    ///     Addon identifier.
    /// </summary>
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    /// <summary>
    ///     Rating value.
    /// </summary>
    [Column("rating")]
    public required byte Rating { get; set; }
}
