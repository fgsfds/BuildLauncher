#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Server.DbEntities;

[Index(nameof(AddonId))]
[Table(name: "versions", Schema = "main")]
public sealed class VersionsDbEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [ForeignKey(nameof(AddonsTable))]
    [Column("addon_id")]
    public required string AddonId { get; set; }

    [Column("version", TypeName = "varchar(10)")]
    public required string Version { get; set; }

    [Column("download_url")]
    public required Uri DownloadUrl { get; set; }

    [Column("file_size")]
    public required long FileSize { get; set; }

    [Column("author")]
    public required string? Author { get; set; }

    [Column("description")]
    public required string? Description { get; set; }

    [Column("is_disabled")]
    public required bool IsDisabled { get; set; }

    [Column("updated")]
    public required DateTime UpdateDate { get; set; }


    public AddonsDbEntity AddonsTable { get; set; }
}
