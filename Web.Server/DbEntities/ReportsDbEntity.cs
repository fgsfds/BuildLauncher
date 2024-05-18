using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
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
}
