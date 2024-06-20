using ClientCommon.Config.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace ClientCommon.Config;

public sealed class DatabaseContext : DbContext
{
    public DbSet<DisabledDbEntity> DisabledAddons { get; set; }
    public DbSet<PlaytimesDbEntity> Playtimes { get; set; }
    public DbSet<RatingDbEntity> Rating { get; set; }
    public DbSet<SettingsDbEntity> Settings { get; set; }
    public DbSet<GamePathsDbEntity> GamePaths { get; set; }

    public DatabaseContext()
    {
        try
        {
            Database.Migrate();
        }
        catch
        {
            Database.EnsureDeleted();
            Database.Migrate();
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=config.db");
    }
}
