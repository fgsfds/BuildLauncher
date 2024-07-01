using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Blazor.DbEntities
{
    [Index(nameof(GameId))]
    [Index(nameof(AddonType))]
    [Table(name: "addons", Schema = "main")]
    public sealed class AddonsDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required string Id{ get; init; }

        [ForeignKey(nameof(GameTable))]
        [Column("game_id")]
        public required byte GameId { get; set; }

        [ForeignKey(nameof(AddonTypeTable))]
        [Column("addon_type_id")]
        public required byte AddonType { get; set; }

        [Column("title")]
        public required string Title { get; set; }


        public GamesDbEntity GameTable { get; set; }
        public AddonTypeDbEntity AddonTypeTable { get; set; }
    }
}
