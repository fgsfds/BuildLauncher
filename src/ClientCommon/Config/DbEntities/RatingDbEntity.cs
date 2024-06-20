using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientCommon.Config.DbEntities;

[Table(name: "rating", Schema = "main")]
public sealed class RatingDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }
    
    [Column("rating")]
    public required byte Rating { get; set; }
}
