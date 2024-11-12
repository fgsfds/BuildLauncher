#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Server.DbEntities;

[Index(nameof(AddonId))]
[Table(name: "reports", Schema = "main")]
public sealed class ReportsDbEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(AddonsTable))]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    [Column("text")]
    public required string ReportText { get; set; }


    public AddonsDbEntity AddonsTable { get; set; }
}
