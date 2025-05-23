using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Client.DbEntities;

[Table(name: "favorite_addons", Schema = "main")]
[PrimaryKey(nameof(AddonId), nameof(Version))]
public sealed class FavoritesDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    [Key]
    [Column("addon_version")]
    public required string Version { get; set; }
}
