using Database.Client.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Database.Client;

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
        catch (Exception)
        {
            ConvertOldConfig();
        }
    }

    [Obsolete]
    private void ConvertOldConfig()
    {
        var settings = Settings.ToList();
        var disabled = DisabledAddons.ToList();
        var playtimes = Playtimes.ToList();
        var paths = GamePaths.ToList();

        _ = Database.EnsureDeleted();
        Database.Migrate();

        Settings.AddRange(settings);
        DisabledAddons.AddRange(disabled);
        Playtimes.AddRange(playtimes);
        GamePaths.AddRange(paths);

        _ = SaveChanges();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSqlite("Data Source=config.db");
    }
}
