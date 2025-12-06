using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

[Table(name: "addon_options", Schema = "main")]
public sealed class OptionsDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    [Column("enabled_options")]
    public required string EnabledOptions { get; set; }
}