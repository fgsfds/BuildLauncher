#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Database.Client.DbEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Database.Client;

public sealed class DatabaseContext : DbContext
{
    public DbSet<DisabledDbEntity> DisabledAddons { get; set; }
    public DbSet<PlaytimesDbEntity> Playtimes { get; set; }
    public DbSet<RatingDbEntity> Rating { get; set; }
    public DbSet<SettingsDbEntity> Settings { get; set; }
    public DbSet<GamePathsDbEntity> GamePaths { get; set; }
    public DbSet<CustomPortsDbEntity> CustomPorts { get; set; }
    public DbSet<FavoritesDbEntity> Favorites { get; set; }
    public DbSet<OptionsDbEntity> Options { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.ConfigureWarnings(x =>
            x.Ignore(RelationalEventId.PendingModelChangesWarning));

        _ = optionsBuilder.UseSqlite("Data Source=BuildLauncher.db");
    }
}
