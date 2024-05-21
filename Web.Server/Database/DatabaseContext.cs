using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Web.Server.DbEntities;
using Web.Server.Entities;

namespace Web.Server.Database
{
    public sealed class DatabaseContext : DbContext
    {
        private static bool _isRunOnce = false;

        public DbSet<GamesDbEntity> Games { get; set; }
        public DbSet<AddonTypeDbEntity> AddonTypes { get; set; }
        public DbSet<AddonsDbEntity> Addons { get; set; }
        public DbSet<VersionsDbEntity> Versions { get; set; }
        public DbSet<InstallsDbEntity> Installs { get; set; }
        public DbSet<ScoresDbEntity> Scores { get; set; }
        public DbSet<ReportsDbEntity> Reports { get; set; }
        public DbSet<TagsDbEntity> Tags { get; set; }
        public DbSet<TagsListsDbEntity> TagsLists { get; set; }
        public DbSet<DependenciesDbEntity> Dependencies { get; set; }

        public DatabaseContext()
        {
#if DEBUG
            if (!_isRunOnce)
            {
                Database.EnsureDeleted();
                _isRunOnce = true;
            }
#endif

            Database.EnsureCreated();

            if (Addons is null || !Addons.Any())
            {
                FillDb();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=buildlauncher;Username=postgres;Password=123;Include Error Detail=True");
#else

            string dbip = Environment.GetEnvironmentVariable("DbIp")!;
            string dbport = Environment.GetEnvironmentVariable("DbPort")!;
            string user = Environment.GetEnvironmentVariable("DbUser")!;
            string password = Environment.GetEnvironmentVariable("DbPass")!;
            optionsBuilder.UseNpgsql($"Host={dbip};Port={dbport};Database=buildlauncher;Username={user};Password={password}");
#endif
        }

        [Obsolete]
        private bool FillDb()
        {
            try
            {
                using var httpClient = new HttpClient();
                var addons = httpClient.GetStringAsync("https://files.fgsfds.link/buildlauncher/addons.json").Result;
                var addonsList = JsonSerializer.Deserialize(addons, AddonsJsonEntityListContext.Default.ListAddonsJsonEntity);


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


                //TAGS
                //List<TagsDbEntity> tags = new();

                //foreach (var game in fixesList)
                //{
                //    foreach (var fix in game.Fixes)
                //    {
                //        if (fix.Tags is not null)
                //        {
                //            foreach (var tag in fix.Tags)
                //            {
                //                if (!tags.Any(x => x.Tag == tag))
                //                {
                //                    tags.Add(new() { Tag = tag });
                //                }
                //            }
                //        }
                //    }
                //}

                //Tags.AddRange(tags);
                //this.SaveChanges();


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
                        var existingDepVersion = Versions.SingleOrDefault(x => x.AddonId == dep.Key && (x.Version == dep.Value || dep.Value == null));

                        if (existingDepVersion is null)
                        {
                            continue;
                            //throw new Exception("Addon doesn't exist");
                        }

                        Dependencies.Add(new()
                        {
                            AddonVersionId = existingVersion.Id,
                            DependencyVersionId = existingDepVersion.Id
                        });

                        this.SaveChanges();
                    }
                }


                //TAGSLISTS
                //foreach (var game in addonsList)
                //{
                //    foreach (var fix in game.Fixes)
                //    {
                //        var tagsList = fix.Tags;

                //        if (tagsList is null)
                //        {
                //            continue;
                //        }

                //        foreach (var tag in tagsList)
                //        {
                //            var tagId = Tags.First(x => x.Tag == tag);

                //            TagsListsDbEntity entiry = new()
                //            {
                //                AddonId = fix.Guid,
                //                TagId = tagId.Id
                //            };

                //            TagsLists.Add(entiry);
                //        }
                //    }
                //}

                //this.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
