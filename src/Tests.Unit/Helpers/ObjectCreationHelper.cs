using Addons.Providers;
using Core.All;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Unit.Helpers;

/// <summary>
///     Helper for creating test provider instances.
/// </summary>
public static class ObjectCreationHelper
{
    /// <summary>
    ///     Creates an <see cref="InstalledAddonsProvider" /> wired up with a fresh <see cref="LocalFilesProvider" />,
    ///     <see cref="MetadataProvider" />, and <see cref="OriginalCampaignsProvider" /> for the given game.
    ///     The caller must dispose the result.
    /// </summary>
    internal static (InstalledAddonsProvider installedAddonsProvider, LocalFilesProvider localFilesProvider) CreateInstalledAddonsProvider(BaseGame game, IConfigProvider config)
    {
        Mock<ICacheAdder<Stream>> cache = new();
        ChannelBroadcaster<DiHelper.LocalFileEvent> channelBroadcaster = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([game]);

        var localFilesProvider = new LocalFilesProvider(gamesProvider.Object, cache.Object, channelBroadcaster, NullLogger<LocalFilesProvider>.Instance);

        var metadataProvider = new MetadataProvider(
            localFilesProvider,
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
            );

        var originalCampaignsProvider = new OriginalCampaignsProvider(config);

        var installedAddonsProvider = new InstalledAddonsProvider(
            game,
            config,
            originalCampaignsProvider,
            metadataProvider,
            localFilesProvider,
            channelBroadcaster,
            NullLogger<InstalledAddonsProvider>.Instance
            );

        return (installedAddonsProvider, localFilesProvider);
    }
}
