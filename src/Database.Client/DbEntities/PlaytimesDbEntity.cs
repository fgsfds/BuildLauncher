using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

[Table(name: "playtimes", Schema = "main")]
public sealed class PlaytimesDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    [Column("playtime")]
    public required TimeSpan Playtime { get; set; }
}
