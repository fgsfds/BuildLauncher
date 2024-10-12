using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Server.DbEntities;

[Table(name: "ratings", Schema = "main")]
public sealed class RatingsDbEntity
{
    [Key]
    [ForeignKey(nameof(AddonsTable))]
    [Column("addon_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required string AddonId { get; set; }

    [Column("rating_sum")]
    public required decimal RatingSum { get; set; }

    [Column("rating_total")]
    public required decimal RatingTotal { get; set; }

    [Column("rating")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal Rating { get; set; }


    public AddonsDbEntity AddonsTable { get; set; }
}
