using ClientCommon.Config.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace ClientCommon.Config;

public sealed class DatabaseContext : DbContext
{
    public DbSet<DisabledDbEntity> DisabledAddons { get; set; }
    public DbSet<PlaytimesDbEntity> Playtimes { get; set; }
    public DbSet<ScoresDbEntity> Scores { get; set; }
    public DbSet<SettingsDbEntity> Settings { get; set; }
    public DbSet<GamePathsDbEntity> GamePaths { get; set; }

    public DatabaseContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=config.db");
    }
}
