using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for application settings.
/// </summary>
[Table(name: "settings", Schema = "main")]
public sealed class SettingsDbEntity
{
    /// <summary>
    ///     Setting name.
    /// </summary>
    [Key]
    [Column("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Setting value.
    /// </summary>
    [Column("value")]
    public required string Value { get; set; }
}
