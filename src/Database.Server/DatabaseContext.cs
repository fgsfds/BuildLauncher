using Database.Server.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Database.Server;

public sealed class DatabaseContext : DbContext
{
    private readonly bool _isDevMode = false;

    public DbSet<GamesDbEntity> Games { get; set; }
    public DbSet<AddonTypeDbEntity> AddonTypes { get; set; }
    public DbSet<AddonsDbEntity> Addons { get; set; }
    public DbSet<VersionsDbEntity> Versions { get; set; }
    public DbSet<InstallsDbEntity> Installs { get; set; }
    public DbSet<RatingsDbEntity> Rating { get; set; }
    public DbSet<ReportsDbEntity> Reports { get; set; }
    public DbSet<DependenciesDbEntity> Dependencies { get; set; }


    public DatabaseContext()
    {
        _isDevMode = true;
        Database.Migrate();
    }

    public DatabaseContext(bool isDevMode)
    {
        _isDevMode = isDevMode;
        Database.Migrate();
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_isDevMode)
        {
            _ = optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=buildlauncher;Username=postgres;Password=123;Include Error Detail=True");
        }
        else
        {
            var dbip = Environment.GetEnvironmentVariable("DbIp")!;
            var dbport = Environment.GetEnvironmentVariable("DbPort")!;
            var user = Environment.GetEnvironmentVariable("DbUser")!;
            var password = Environment.GetEnvironmentVariable("DbPass")!;
            var dbName = Environment.GetEnvironmentVariable("DbName")!;

            _ = optionsBuilder.UseNpgsql($"Host={dbip};Port={dbport};Database={dbName};Username={user};Password={password}");
        }
    }
}
