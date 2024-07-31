using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Blazor.DbEntities
{
    [Index(nameof(Name))]
    [Table(name: "games", Schema = "main")]
    public sealed class GamesDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required byte Id { get; set; }

        [Column("name")]
        public required string Name { get; set; }
    }
}
