#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using Database.Client.DbEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Database.Client;

/// <summary>
///     Database context for the Build Launcher SQLite database.
/// </summary>
public sealed class DatabaseContext : DbContext
{
    /// <summary>
    ///     Gets or sets the disabled addons.
    /// </summary>
    public DbSet<DisabledDbEntity> DisabledAddons { get; set; }

    /// <summary>
    ///     Gets or sets the playtimes.
    /// </summary>
    public DbSet<PlaytimesDbEntity> Playtimes { get; set; }

    /// <summary>
    ///     Gets or sets the ratings.
    /// </summary>
    public DbSet<RatingDbEntity> Rating { get; set; }

    /// <summary>
    ///     Gets or sets the settings.
    /// </summary>
    public DbSet<SettingsDbEntity> Settings { get; set; }

    /// <summary>
    ///     Gets or sets the game paths.
    /// </summary>
    public DbSet<GamePathsDbEntity> GamePaths { get; set; }

    /// <summary>
    ///     Gets or sets the custom ports.
    /// </summary>
    public DbSet<CustomPortsDbEntity> CustomPorts { get; set; }

    /// <summary>
    ///     Gets or sets the favorites.
    /// </summary>
    public DbSet<FavoritesDbEntity> Favorites { get; set; }

    /// <summary>
    ///     Gets or sets the addon options.
    /// </summary>
    public DbSet<OptionsDbEntity> Options { get; set; }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.ConfigureWarnings(x =>
                                                 x.Ignore(RelationalEventId.PendingModelChangesWarning));

        _ = optionsBuilder.UseSqlite("Data Source=BuildLauncher.db");
    }
}
