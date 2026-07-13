using Addons.Helpers;
using Addons.Providers;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Unit.Helpers;

public static class ObjectCreationHelper
{
    internal static (InstalledAddonsProvider installedAddonsProvider, MetadataProvider metadataProvider) CreateInstalledAddonsProviderWithMetadata(BaseGame game, IConfigProvider config)
    {
        Mock<ICacheAdder<Stream>> bitmapsCache = new();

        var metadataProvider = new MetadataProvider(
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
            );

        var originalCampaignsProvider = new OriginalCampaignsProvider(config);

        var installedAddonsProvider = new InstalledAddonsProvider(
            game,
            config,
            bitmapsCache.Object,
            originalCampaignsProvider,
            metadataProvider,
            new ArchivedAddonExtractor(NullLogger<ArchivedAddonExtractor>.Instance),
            NullLogger<InstalledAddonsProvider>.Instance
            );

        return (installedAddonsProvider, metadataProvider);
    }

    internal static InstalledAddonsProvider CreateInstalledAddonsProvider(BaseGame game, IConfigProvider config)
    {
        Mock<ICacheAdder<Stream>> bitmapsCache = new();

        var metadataProvider = new MetadataProvider(
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
            );

        var originalCampaignsProvider = new OriginalCampaignsProvider(config);

        var installedAddonsProvider = new InstalledAddonsProvider(
            game,
            config,
            bitmapsCache.Object,
            originalCampaignsProvider,
            metadataProvider,
            new ArchivedAddonExtractor(NullLogger<ArchivedAddonExtractor>.Instance),
            NullLogger<InstalledAddonsProvider>.Instance
            );

        return installedAddonsProvider;
    }
}
