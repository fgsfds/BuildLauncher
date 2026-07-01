using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for game installation paths.
/// </summary>
[Table("game_paths", Schema = "main")]
public sealed class GamePathsDbEntity
{
    /// <summary>
    ///     Game name.
    /// </summary>
    [Key]
    [Column("game")]
    public required string Game { get; set; }

    /// <summary>
    ///     Path to the game installation.
    /// </summary>
    [Column("path")]
    public required string? Path { get; set; }
}
