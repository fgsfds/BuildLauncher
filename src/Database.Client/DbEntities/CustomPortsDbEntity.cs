using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.All.Enums;

namespace Database.Client.DbEntities;

[Table(name: "custom_ports", Schema = "main")]
public sealed class CustomPortsDbEntity
{
    [Key]
    [Column("name")]
    public required string Name { get; set; }

    [Column("path")]
    public required string PathToExe { get; set; }

    [Column("port")]
    public required PortEnum PortEnum { get; set; }
}
