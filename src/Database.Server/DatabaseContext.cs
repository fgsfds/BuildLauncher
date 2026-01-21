using Database.Server.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Database.Server;

public sealed class DatabaseContext : DbContext
{
    public DbSet<GamesDbEntity> Games { get; set; }
    public DbSet<AddonTypeDbEntity> AddonTypes { get; set; }
    public DbSet<AddonsDbEntity> Addons { get; set; }
    public DbSet<VersionsDbEntity> Versions { get; set; }
    public DbSet<InstallsDbEntity> Installs { get; set; }
    public DbSet<RatingsDbEntity> Rating { get; set; }
    public DbSet<ReportsDbEntity> Reports { get; set; }
    public DbSet<DependenciesDbEntity> Dependencies { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
}
