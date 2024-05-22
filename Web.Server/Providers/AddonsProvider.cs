using Common.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web.Server.DbEntities;
using Web.Server.Helpers;

namespace Web.Server.Providers
{
    public sealed class AddonsProvider
    {
        private readonly ILogger<AppReleasesProvider> _logger;
        private readonly DatabaseContextFactory _dbContextFactory;

        public GeneralReleaseEntity? AppRelease { get; private set; }

        public AddonsProvider(
            ILogger<AppReleasesProvider> logger,
            DatabaseContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Return addons list for a game
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        public List<DownloadableAddonEntity> GetAddonsList(GameEnum gameEnum)
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
            var scores = dbContext.Scores.AsNoTracking().ToDictionary(static x => x.AddonId, static y => y.Score);

            List<DownloadableAddonEntity> result = new(versions.Count);

            foreach (var version in versions)
            {
                var addon = addons[version.Value.AddonId];

                List<string>? depsResult = null;
                var addonDeps = dependencies[version.Key];

                foreach (var dep in addonDeps)
                {
                    depsResult ??= [];

                    var depVersion = versions[dep.DependencyVersionId];
                    var depAddon = addons[depVersion.AddonId];

                    depsResult.Add(depAddon.Title);
                }

                var hasInstalls = installs.TryGetValue(addon.Id, out var installsNumber);
                var hasScore = scores.TryGetValue(addon.Id, out var scoreNumber);

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
                    Score = hasScore ? scoreNumber : 0
                };

                result.Add(newDownloadable);
            }

            sw.Stop();
            _logger.LogInformation($"Got addons for {gameEnum} in {sw.ElapsedMilliseconds} ms.");

            return result;
        }

        internal int IncreaseAddonInstallsCount(string addonId)
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

        internal int ChangeAddonScore(string addonId, sbyte increment)
        {
            using var dbContext = _dbContextFactory.Get();
            var fix = dbContext.Scores.Find(addonId);

            int newScore;

            if (fix is null)
            {
                ScoresDbEntity newScoreEntity = new()
                {
                    AddonId = addonId,
                    Score = increment
                };

                dbContext.Scores.Add(newScoreEntity);
                newScore = increment;
            }
            else
            {
                fix.Score += increment;
                newScore = fix.Score;
            }

            dbContext.SaveChanges();
            return newScore;
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

        internal Dictionary<string, int> GetScores()
        {
            using var dbContext = _dbContextFactory.Get();
            return dbContext.Scores.ToDictionary(static x => x.AddonId, static y => y.Score);
        }
    }
}
