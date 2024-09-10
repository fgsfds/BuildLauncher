using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Server.DbEntities;

[Table(name: "installs", Schema = "main")]
public sealed class InstallsDbEntity
{
    [Key]
    [ForeignKey(nameof(AddonsTable))]
    [Column("addon_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string AddonId { get; set; }

    [Column("value")]
    public required int Installs { get; set; }


    public AddonsDbEntity AddonsTable { get; set; }
}
