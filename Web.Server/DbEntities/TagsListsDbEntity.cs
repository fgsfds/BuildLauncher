using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
    [Index(nameof(AddonId))]
    [Table(name: "tags_lists", Schema = "main")]
    public sealed class TagsListsDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(AddonsTable))]
        [Column("addon_id")]
        public required string AddonId { get; set; }

        [ForeignKey(nameof(TagsTable))]
        [Column("tag_id")]
        public required int TagId { get; set; }


        public AddonsDbEntity AddonsTable { get; set; }
        public TagsDbEntity TagsTable { get; set; }
    }
}
