using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for favorite addons.
/// </summary>
[Table(name: "favorite_addons", Schema = "main")]
[PrimaryKey(nameof(AddonId), nameof(Version))]
public sealed class FavoritesDbEntity
{
    /// <summary>
    ///     Addon identifier.
    /// </summary>
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    /// <summary>
    ///     Addon version.
    /// </summary>
    [Key]
    [Column("addon_version")]
    public required string Version { get; set; }
}
