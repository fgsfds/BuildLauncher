using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

[Table("game_paths", Schema = "main")]
public sealed class GamePathsDbEntity
{
    [Key]
    [Column("game")]
    public required string Game { get; set; }

    [Column("path")]
    public required string? Path { get; set; }
}
