using System.Collections.Concurrent;
using Addons.Addons;
using Avalonia.Desktop.Helpers;
using Avalonia.Media.Imaging;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;
using Core.All.Helpers;
using Core.Client.Cache;
using Core.Client.Helpers;
using Ports.Ports;
using Tools.Tools;

namespace Avalonia.Desktop.Misc;

/// <summary>
///     Cache for bitmap images used throughout the application.
/// </summary>
public sealed class BitmapsCache : ICacheAdder<Stream>, ICacheGetter<Bitmap>, IDisposable
{
    /// <summary>
    ///     The internal cache of bitmap images keyed by hash.
    /// </summary>
    private readonly ConcurrentDictionary<long, Bitmap> _cache = [];

    private readonly IReadOnlyList<BasePort> _ports;

    private readonly IReadOnlyList<BaseTool> _tools;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BitmapsCache" /> class.
    /// </summary>
    /// <param name="ports">The available ports.</param>
    /// <param name="tools">The available tools.</param>
    public BitmapsCache(
        IEnumerable<BasePort> ports,
        IEnumerable<BaseTool> tools
        )
    {
        _ports = [.. ports];
        _tools = [.. tools];
    }

    /// <summary>
    ///     Initializes the cache with all known images.
    /// </summary>
    public void InitializeCache()
    {
        InitOfficialCampaignsCache();
        InitPortsCache();
        InitToolsCache();
    }

    /// <summary>
    ///     Tries to add a stream to the cache.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="item">The stream to add.</param>
    /// <returns>True if the item was added; false if it already exists.</returns>
    public bool TryAddToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        Bitmap bitmap = new(item);

        return _cache.TryAdd(id, bitmap);
    }

    /// <summary>
    ///     Tries to add a grid-sized image to the cache.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="item">The stream to add.</param>
    /// <returns>True if the item was added; false if it already exists.</returns>
    public bool TryAddGridToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        var bitmap = Bitmap.DecodeToWidth(item, (int)DesktopConsts.GridImageWidth);

        return _cache.TryAdd(id, bitmap);
    }

    /// <summary>
    ///     Tries to add a preview-sized image to the cache.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="item">The stream to add.</param>
    /// <returns>True if the item was added; false if it already exists.</returns>
    public bool TryAddPreviewToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        var bitmap = Bitmap.DecodeToWidth(item, 320);

        return _cache.TryAdd(id, bitmap);
    }

    /// <summary>
    ///     Tries to remove an item from the cache.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>True if the item was removed; false otherwise.</returns>
    public bool TryRemoveFromCache(long id)
    {
        if (_cache.Remove(id, out var bitmap))
        {
            bitmap.Dispose();

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Gets a bitmap from the cache by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>The bitmap if found; otherwise null.</returns>
    public Bitmap? GetFromCache(long id) => _cache.TryGetValue(id, out var bitmap) ? bitmap : null;


    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var bitmap in _cache.Values)
        {
            bitmap.Dispose();
        }
    }


    /// <summary>
    ///     Initializes the cache with official campaign images.
    /// </summary>
    private void InitOfficialCampaignsCache()
    {
        var addonsAss = typeof(BaseAddon).Assembly;

        using var blood = ImageHelper.FileNameToStream("Blood.blood.png", addonsAss);
        _ = TryAddGridToCache(GameEnum.Blood.GetUniqueHash(), blood);

        using var cp = ImageHelper.FileNameToStream("Blood.cp.jpg", addonsAss);
        _ = TryAddGridToCache(BloodAddonEnum.BloodCP.GetUniqueHash(), cp);


        using var carib = ImageHelper.FileNameToStream("Duke3D.carib.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeVaca.GetUniqueHash(), carib);

        using var duke3d = ImageHelper.FileNameToStream("Duke3D.duke3d.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Duke3D.GetUniqueHash(), duke3d);

        using var duke64 = ImageHelper.FileNameToStream("Duke3D.duke64.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Duke64.GetUniqueHash(), duke64);

        using var dukeZH = ImageHelper.FileNameToStream("Duke3D.dukezh.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.DukeZeroHour.GetUniqueHash(), dukeZH);

        using var dukeDc = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeDC.GetUniqueHash(), dukeDc);

        using var dukeWt = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg", addonsAss);
        _ = TryAddGridToCache(DukeVersionEnum.Duke3D_WT.GetUniqueHash(), dukeWt);

        using var dukeNw = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeNW.GetUniqueHash(), dukeNw);


        using var fury = ImageHelper.FileNameToStream("Fury.fury.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Fury.GetUniqueHash(), fury);

        using var ashock = ImageHelper.FileNameToStream("Fury.aftershock.jpg", addonsAss);
        _ = TryAddGridToCache("Aftershock".GetHashCode(), ashock);


        using var nam = ImageHelper.FileNameToStream("NAM.nam.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.NAM.GetUniqueHash(), nam);


        using var again = ImageHelper.FileNameToStream("Redneck.again.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.RidesAgain.GetUniqueHash(), again);

        using var rr = ImageHelper.FileNameToStream("Redneck.redneck.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Redneck.GetUniqueHash(), rr);

        using var r66 = ImageHelper.FileNameToStream("Redneck.route66.jpg", addonsAss);
        _ = TryAddGridToCache(RedneckAddonEnum.Route66.GetUniqueHash(), r66);


        using var ps = ImageHelper.FileNameToStream("Slave.slave.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Slave.GetUniqueHash(), ps);


        using var tw = ImageHelper.FileNameToStream("TekWar.tekwar.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.TekWar.GetUniqueHash(), tw);


        using var td = ImageHelper.FileNameToStream("Wang.twin.jpg", addonsAss);
        _ = TryAddGridToCache(WangAddonEnum.TwinDragon.GetUniqueHash(), td);

        using var wang = ImageHelper.FileNameToStream("Wang.wang.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Wang.GetUniqueHash(), wang);

        using var wd = ImageHelper.FileNameToStream("Wang.wanton.jpg", addonsAss);
        _ = TryAddGridToCache(WangAddonEnum.Wanton.GetUniqueHash(), wd);


        using var wh1 = ImageHelper.FileNameToStream("Witchaven.wh1.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Witchaven.GetUniqueHash(), wh1);

        using var wh2 = ImageHelper.FileNameToStream("Witchaven.wh2.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.Witchaven2.GetUniqueHash(), wh2);


        using var plat = ImageHelper.FileNameToStream("WW2GI.platoon.jpg", addonsAss);
        _ = TryAddGridToCache(WW2GIAddonEnum.Platoon.GetUniqueHash(), plat);

        using var ww2 = ImageHelper.FileNameToStream("WW2GI.ww2gi.jpg", addonsAss);
        _ = TryAddGridToCache(GameEnum.WW2GI.GetUniqueHash(), ww2);
    }

    /// <summary>
    ///     Initializes the cache with port icon images.
    /// </summary>
    private void InitPortsCache()
    {
        var portsAss = typeof(BasePort).Assembly;

        foreach (var port in _ports)
        {
            using var png = ImageHelper.FileNameToStream($"{port.ShortName}.png", portsAss);
            _ = TryAddToCache(port.PortEnum.GetUniqueHash(), png);
        }
    }

    /// <summary>
    ///     Initializes the cache with tool icon images.
    /// </summary>
    private void InitToolsCache()
    {
        var toolsAss = typeof(BaseTool).Assembly;

        foreach (var port in _tools)
        {
            using var png = ImageHelper.FileNameToStream($"{port.Name}.png", toolsAss);
            _ = TryAddToCache(port.ToolEnum.GetUniqueHash(), png);
        }
    }
}
