using Common.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Web.Blazor.DbEntities;
using Web.Blazor.Helpers;

namespace Web.Blazor.Database
{
    public sealed class DatabaseContext : DbContext
    {
        private static bool _isRunOnce = false;

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
            if (ServerProperties.IsDevMode)
            {
                if (!_isRunOnce)
                {
                    //Database.EnsureDeleted();
                    _isRunOnce = true;
                }
            }

            Database.EnsureCreated();

            if (Addons is null || !Addons.Any())
            {
                FillDb();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ServerProperties.IsDevMode)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=buildlauncher;Username=postgres;Password=123;Include Error Detail=True");
            }
            else
            {
                var dbip = Environment.GetEnvironmentVariable("DbIp")!;
                var dbport = Environment.GetEnvironmentVariable("DbPort")!;
                var user = Environment.GetEnvironmentVariable("DbUser")!;
                var password = Environment.GetEnvironmentVariable("DbPass")!;
                optionsBuilder.UseNpgsql($"Host={dbip};Port={dbport};Database=buildlauncher;Username={user};Password={password}");
            }
        }

        [Obsolete]
        private bool FillDb()
        {
            try
            {
                using var httpClient = new HttpClient();
                var addons = httpClient.GetStringAsync("https://files.fgsfds.link/buildlauncher/addons.json").Result;
                var addonsList = JsonSerializer.Deserialize(addons, AddonsJsonEntityListContext.Default.ListAddonsJsonEntity)!;


                //TYPES
                var addonTypes = Enum.GetValues<AddonTypeEnum>();

                foreach (var type in addonTypes)
                {
                    AddonTypes.Add(new()
                    { 
                        Id = (byte)type, 
                        Type = type.ToString() 
                    });
                }

                //GAMES
                var gamesTypes = Enum.GetValues<GameEnum>();

                foreach (var type in gamesTypes)
                {
                    Games.Add(new() { 
                        Id = (byte)type, 
                        Name = type.ToString()
                    });
                }

                this.SaveChanges();


                //Addons
                foreach (var addon in addonsList)
                {
                    var existing = Addons.Find(addon.Id);

                    if (existing is not null)
                    {
                        continue;
                    }

                    Addons.Add(new()
                    {
                        Id = addon.Id,
                        Title = addon.Title,
                        GameId = (byte)addon.Game,
                        AddonType = (byte)addon.AddonType
                    });

                    this.SaveChanges();
                }


                //Versions
                foreach (var addon in addonsList)
                {
                    var existing = Addons.Find(addon.Id) ?? throw new Exception("Addon doesn't exist");

                    Versions.Add(new()
                    {
                        AddonId = existing.Id,
                        Version = addon.Version,
                        DownloadUrl = new(addon.DownloadUrl),
                        Description = addon.Description,
                        IsDisabled = false,
                        FileSize = addon.FileSize,
                        Author = addon.Author,
                        UpdateDate = DateTime.Now.ToUniversalTime()
                    });

                    this.SaveChanges();
                }


                //Dependencies
                foreach (var addon in addonsList)
                {
                    if (addon.Dependencies is null)
                    {
                        continue;
                    }

                    var existingVersion = Versions.SingleOrDefault(x => x.AddonId == addon.Id && x.Version == addon.Version);

                    if (existingVersion is null)
                    {
                        throw new Exception("Addon doesn't exist");
                    }

                    foreach (var dep in addon.Dependencies)
                    {
                        Dependencies.Add(new()
                        {
                            AddonVersionId = existingVersion.Id,
                            DependencyId = dep.Key,
                            DependencyVersion = dep.Value
                        });
                    }
                }

                this.SaveChanges();


                //Scores
                foreach (var addon in Addons.AsNoTracking().ToList())
                {
                    RatingsDbEntity score = new()
                    { 
                        AddonId = addon.Id,
                        RatingSum = 0,
                        RatingTotal = 0
                    };

                    Rating.Add(score);
                }

                this.SaveChanges();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
