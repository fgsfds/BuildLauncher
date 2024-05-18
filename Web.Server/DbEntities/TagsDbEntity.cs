using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
    [Index(nameof(Tag), IsUnique = true)]
    [Table(name: "tags", Schema = "main")]
    public sealed class TagsDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("tag")]
        public required string Tag { get; set; }
    }
}
