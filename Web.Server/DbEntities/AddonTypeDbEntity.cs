using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
    [Table(name: "addon_type", Schema = "main")]
    public sealed class AddonTypeDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required byte Id { get; set; }

        [Column("type")]
        public required string Type { get; set; }
    }
}
