using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
    [Table(name: "scores", Schema = "main")]
    public sealed class ScoresDbEntity
    {
        [Key]
        [ForeignKey(nameof(AddonsTable))]
        [Column("addon_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required string AddonId { get; set; }

        [Column("value")]
        public required int Score { get; set; }


        public AddonsDbEntity AddonsTable { get; set; }
    }
}
