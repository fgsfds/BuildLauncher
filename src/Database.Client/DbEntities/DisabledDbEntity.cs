﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Client.DbEntities;

[Table(name: "disabled_addons", Schema = "main")]
public sealed class DisabledDbEntity
{
    [Key]
    [Column("addon_id")]
    public required string AddonId { get; set; }
}