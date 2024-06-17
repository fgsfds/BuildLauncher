﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Server.DbEntities
{
    [Index(nameof(AddonVersionId))]
    [Table(name: "dependencies", Schema = "main")]
    public sealed class DependenciesDbEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(VersionsTable))]
        [Column("version_id")]
        public required int AddonVersionId { get; set; }

        [Column("dependency_id")]
        public required string DependencyId { get; set; }

        [Column("dependency_version", TypeName = "varchar(10)")]
        public required string? DependencyVersion { get; set; }


        public VersionsDbEntity VersionsTable { get; set; }
    }
}
