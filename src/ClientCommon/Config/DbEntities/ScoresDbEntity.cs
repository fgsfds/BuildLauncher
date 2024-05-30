using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientCommon.Config.DbEntities;

[Table(name: "scores", Schema = "main")]
public sealed class ScoresDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }
    
    [Column("is_upvoted")]
    public required bool IsUpvoted { get; set; }
}
