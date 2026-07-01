using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.All.Enums;

namespace Database.Client.DbEntities;

/// <summary>
///     Database entity for custom ports.
/// </summary>
[Table(name: "custom_ports", Schema = "main")]
public sealed class CustomPortsDbEntity
{
    /// <summary>
    ///     Port name.
    /// </summary>
    [Key]
    [Column("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     Path to the port executable.
    /// </summary>
    [Column("path")]
    public required string PathToExe { get; set; }

    /// <summary>
    ///     Port type.
    /// </summary>
    [Column("port")]
    public required PortEnum PortEnum { get; set; }
}
