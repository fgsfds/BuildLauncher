using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for addon options.
/// </summary>
[Table(name: "addon_options", Schema = "main")]
public sealed class OptionsDbEntity
{
    /// <summary>
    ///     Addon identifier.
    /// </summary>
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    /// <summary>
    ///     Serialized enabled options.
    /// </summary>
    [Column("enabled_options")]
    public required string EnabledOptions { get; set; }
}
