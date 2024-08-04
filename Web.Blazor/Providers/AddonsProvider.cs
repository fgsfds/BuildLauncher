using Common.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Blazor.DbEntities;
using Web.Blazor.Helpers;

namespace Web.Blazor.Providers
{
    public sealed class AddonsProvider
    {
        private readonly ILogger<AddonsProvider> _logger;
        private readonly DatabaseContextFactory _dbContextFactory;

        public GeneralReleaseEntity? AppRelease { get; private set; }

        public AddonsProvider(
            ILogger<AddonsProvider> logger,
            DatabaseContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Return addons list for a game
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        internal List<DownloadableAddonEntity> GetAddons(GameEnum gameEnum)
        {
            using var dbContext = _dbContextFactory.Get();

            Stopwatch sw = new();
            sw.Start();

            Dictionary<string, AddonsDbEntity>? addons;

            if (gameEnum is GameEnum.Redneck)
            {
                addons = dbContext.Addons.AsNoTracking().Where(x => x.GameId == (byte)GameEnum.Redneck || x.GameId == (byte)GameEnum.RidesAgain).ToDictionary(static x => x.Id);
            }
            else
            {
                addons = dbContext.Addons.AsNoTracking().Where(x => x.GameId == (byte)gameEnum).ToDictionary(static x => x.Id);
            }

            var versions = dbContext.Versions.AsNoTracking().Where(x => addons.Keys.Contains(x.AddonId)).ToDictionary(static x => x.Id);
            var dependencies = dbContext.Dependencies.AsNoTracking().Where(x => versions.Keys.Contains(x.AddonVersionId)).ToLookup(static x => x.AddonVersionId);
            var installs = dbContext.Installs.AsNoTracking().ToDictionary(static x => x.AddonId, static y => y.Installs);
            var ratings = dbContext.Rating.AsNoTracking().ToDictionary(static x => x.AddonId, static y => y.Rating);

            List<DownloadableAddonEntity> result = new(versions.Count);

            foreach (var version in versions)
            {
                var addon = addons[version.Value.AddonId];

                List<string>? depsResult = null;
                var addonDeps = dependencies[version.Key];

                foreach (var dep in addonDeps)
                {
                    depsResult ??= [];

                    var depAddon = addons[dep.DependencyId];

                    depsResult.Add(depAddon.Title + $"{(dep.DependencyVersion is not null ? $", {dep.DependencyVersion}" : string.Empty)}");
                }

                var hasInstalls = installs.TryGetValue(addon.Id, out var installsNumber);
                var hasRating = ratings.TryGetValue(addon.Id, out var ratingNumber);

                DownloadableAddonEntity newDownloadable = new()
                {
                    AddonType = (AddonTypeEnum)addon.AddonType,
                    Id = addon.Id,
                    Game = (GameEnum)addon.GameId,
                    DownloadUrl = version.Value.DownloadUrl,
                    Title = addon.Title,
                    Version = version.Value.Version,
                    FileSize = version.Value.FileSize,
                    IsDisabled = version.Value.IsDisabled,
                    Description = version.Value.Description,
                    Author = version.Value.Author,
                    Dependencies = depsResult,
                    Installs = hasInstalls ? installsNumber : 0,
                    Score = 0,
                    Rating = hasRating ? ratingNumber : 0,
                    UpdateDate = version.Value.UpdateDate
                };

                result.Add(newDownloadable);
            }

            sw.Stop();
            _logger.LogInformation($"Got addons for {gameEnum} in {sw.ElapsedMilliseconds} ms.");

            return result;
        }

        internal int IncreaseNumberOfInstalls(string addonId)
        {
            using var dbContext = _dbContextFactory.Get();
            var fix = dbContext.Installs.Find(addonId);

            int newInstalls;

            if (fix is null)
            {
                InstallsDbEntity newInstallsEntity = new()
                {
                    AddonId = addonId,
                    Installs = 1
                };

                dbContext.Installs.Add(newInstallsEntity);
                newInstalls = 1;
            }
            else
            {
                fix.Installs += 1;
                newInstalls = fix.Installs;
            }

            dbContext.SaveChanges();
            return newInstalls;
        }

        internal decimal ChangeRating(string addonId, sbyte rating, bool isNew)
        {
            using var dbContext = _dbContextFactory.Get();
            var existingRating = dbContext.Rating.Find(addonId) ?? throw new Exception($"Rating for {addonId} is not found");

            existingRating.RatingSum += rating;

            if (isNew)
            {
                existingRating.RatingTotal++;
            }

            dbContext.SaveChanges();

            return existingRating.Rating;
        }

        internal void AddReport(string addonId, string text)
        {
            using var dbContext = _dbContextFactory.Get();

            ReportsDbEntity entity = new()
            {
                AddonId = addonId,
                ReportText = text
            };

            dbContext.Reports.Add(entity);
            dbContext.SaveChanges();
        }

        internal Dictionary<string, decimal> GetRating()
        {
            using var dbContext = _dbContextFactory.Get();
            return dbContext.Rating.ToDictionary(static x => x.AddonId, static y => y.Rating);
        }

        internal bool AddAddonToDatabase(AddonsJsonEntity addon)
        {
            using var dbContext = _dbContextFactory.Get();

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                List<VersionsDbEntity>? olderVersion = null;

                var existingAddon = dbContext.Addons.Find([addon.Id]);

                if (existingAddon is not null)
                {
                    existingAddon.Title = addon.Title;

                    olderVersion = [.. dbContext.Versions.Where(x => x.AddonId.Equals(existingAddon.Id))];
                }
                else
                {
                    dbContext.Addons.Add(new()
                    {
                        Id = addon.Id,
                        Title = addon.Title,
                        GameId = (byte)addon.Game,
                        AddonType = (byte)addon.AddonType
                    });
                }


                dbContext.SaveChanges();


                var existingVersion = dbContext.Versions.SingleOrDefault(x => x.AddonId == addon.Id && x.Version == addon.Version);

                if (existingVersion is not null)
                {
                    return false;
                }

                dbContext.Versions.Add(new()
                {
                    AddonId = addon.Id,
                    Version = addon.Version,
                    DownloadUrl = new(addon.DownloadUrl),
                    Description = addon.Description,
                    IsDisabled = false,
                    FileSize = addon.FileSize,
                    Author = addon.Author,
                    UpdateDate = DateTime.Now.ToUniversalTime()
                });

                dbContext.SaveChanges();


                if (addon.Dependencies is not null)
                {
                    existingVersion = dbContext.Versions.SingleOrDefault(x => x.AddonId == addon.Id && x.Version == addon.Version);

                    if (existingVersion is null)
                    {
                        throw new Exception("Addon doesn't exist");
                    }

                    foreach (var dep in addon.Dependencies)
                    {
                        dbContext.Dependencies.Add(new()
                        {
                            AddonVersionId = existingVersion.Id,
                            DependencyId = dep.Key,
                            DependencyVersion = dep.Value
                        });
                    }

                    dbContext.SaveChanges();
                }


                var existingScore = dbContext.Rating.Find(addon.Id);

                if (existingScore is null)
                {
                    dbContext.Rating.Add(new()
                    {
                        AddonId = addon.Id,
                        RatingSum = 0,
                        RatingTotal = 0
                    });

                    dbContext.SaveChanges();
                }

                if (olderVersion is not null)
                {
                    foreach (var version in olderVersion)
                    {
                        version.IsDisabled = true;
                    }

                    dbContext.SaveChanges();
                }


                transaction.Commit();

                return true;
            }
        }
    }
}
