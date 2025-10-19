using System.Collections.Concurrent;
using Addons.Addons;
using Avalonia.Desktop.Helpers;
using Avalonia.Media.Imaging;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Enums.Versions;
using Common.All.Helpers;
using Common.Client.Cache;
using Common.Client.Helpers;
using Ports.Ports;

namespace Avalonia.Desktop.Misc;

public sealed class BitmapsCache : ICacheAdder<Stream>, ICacheGetter<Bitmap>, IDisposable
{
    private readonly ConcurrentDictionary<long, Bitmap> _cache = [];

    public void InitializeCache()
    {
        InitOfficialCampaignsCache();
        InitPortsCache();
    }

    public bool TryAddToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        Bitmap bitmap = new(item);
        return _cache.TryAdd(id, bitmap);
    }

    public bool TryAddGridToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        Bitmap bitmap = Bitmap.DecodeToWidth(item, (int)DesktopConsts.GridImageWidth);
        return _cache.TryAdd(id, bitmap);
    }

    public bool TryAddPreviewToCache(long id, Stream item)
    {
        if (_cache.TryGetValue(id, out _))
        {
            return false;
        }

        Bitmap bitmap = Bitmap.DecodeToWidth(item, 320);
        return _cache.TryAdd(id, bitmap);
    }

    public bool TryRemoveFromCache(long id)
    {
        if (_cache.Remove(id, out var bitmap))
        {
            bitmap.Dispose();
            return true;
        }

        return false;
    }

    public Bitmap? GetFromCache(long id) => _cache.TryGetValue(id, out var bitmap) ? bitmap : null;


    private void InitOfficialCampaignsCache()
    {
        var addonsAss = typeof(BaseAddonEntity).Assembly;

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

        using var dukeDc = ImageHelper.FileNameToStream("Duke3D.dukedc.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeDC.GetUniqueHash(), dukeDc);

        using var dukeWt = ImageHelper.FileNameToStream("Duke3D.dukewt.jpg", addonsAss);
        _ = TryAddGridToCache(DukeVersionEnum.Duke3D_WT.GetUniqueHash(), dukeWt);

        using var dukeNw = ImageHelper.FileNameToStream("Duke3D.nwinter.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeNW.GetUniqueHash(), dukeNw);


        using var fury = ImageHelper.FileNameToStream("Fury.fury.jpg", addonsAss);
        _ = TryAddGridToCache(DukeAddonEnum.DukeNW.GetUniqueHash(), fury);

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

    private void InitPortsCache()
    {
        var portsAss = typeof(BasePort).Assembly;

        using var bgdx = ImageHelper.FileNameToStream("BuildGDX.png", portsAss);
        _ = TryAddToCache(PortEnum.BuildGDX.GetUniqueHash(), bgdx);

        using var ed = ImageHelper.FileNameToStream("EDuke32.png", portsAss);
        _ = TryAddToCache(PortEnum.EDuke32.GetUniqueHash(), ed);

        using var fury = ImageHelper.FileNameToStream("Fury.png", portsAss);
        _ = TryAddToCache(PortEnum.Fury.GetUniqueHash(), fury);

        using var nblood = ImageHelper.FileNameToStream("NBlood.png", portsAss);
        _ = TryAddToCache(PortEnum.NBlood.GetUniqueHash(), nblood);

        using var notblood = ImageHelper.FileNameToStream("NotBlood.png", portsAss);
        _ = TryAddToCache(PortEnum.NotBlood.GetUniqueHash(), notblood);

        using var pcex = ImageHelper.FileNameToStream("PCExhumed.png", portsAss);
        _ = TryAddToCache(PortEnum.PCExhumed.GetUniqueHash(), pcex);

        using var raze = ImageHelper.FileNameToStream("Raze.png", portsAss);
        _ = TryAddToCache(PortEnum.Raze.GetUniqueHash(), raze);

        using var rn = ImageHelper.FileNameToStream("RedNukem.png", portsAss);
        _ = TryAddToCache(PortEnum.RedNukem.GetUniqueHash(), rn);

        using var vsw = ImageHelper.FileNameToStream("VoidSW.png", portsAss);
        _ = TryAddToCache(PortEnum.VoidSW.GetUniqueHash(), vsw);

        using var db = ImageHelper.FileNameToStream("DosBox.png", portsAss);
        _ = TryAddToCache(PortEnum.DosBox.GetUniqueHash(), db);
    }


    public void Dispose()
    {
        foreach (var bitmap in _cache.Values)
        {
            bitmap.Dispose();
        }
    }
}
