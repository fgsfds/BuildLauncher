using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Desktop.Controls;
using Avalonia.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Ports.Providers;

namespace Avalonia.Desktop.Misc;

public sealed class ViewLocator : IDataTemplate
{
    private readonly PortsProvider _installedPortsProvider;
    private readonly BitmapsCache _bitmapsCache;

    private readonly Dictionary<object, UserControl> _controlsCache = [];

    public ViewLocator(
        PortsProvider installedPortsProvider,
        BitmapsCache bitmapsCache
        )
    {
        _installedPortsProvider = installedPortsProvider;
        _bitmapsCache = bitmapsCache;
    }

    public Control Build(object? data)
    {
        if (data is not null && _controlsCache.TryGetValue(data, out var control))
        {
            return control;
        }

        UserControl newControl = data switch
        {
            CampaignsViewModel campsVm => new CampaignsControl(campsVm, _installedPortsProvider, _bitmapsCache),
            MapsViewModel mapsVm => new MapsControl(mapsVm, _installedPortsProvider, _bitmapsCache),
            ModsViewModel modsVM => new ModsControl(modsVM),
            DownloadsViewModel => new DownloadsControl(),
            _ => throw new NotSupportedException($"Can't find control for {data} ViewModel.")
        };

        _controlsCache.Add(data, newControl);
        return newControl;
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}
