using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for disabled addons.
/// </summary>
[Table(name: "disabled_addons", Schema = "main")]
public sealed class DisabledDbEntity
{
    /// <summary>
    ///     Addon identifier.
    /// </summary>
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }
}
