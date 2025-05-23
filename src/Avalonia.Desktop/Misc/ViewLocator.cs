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
    private readonly InstalledPortsProvider _installedPortsProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly BitmapsCache _bitmapsCache;

    private readonly Dictionary<object, UserControl> _controlsCache = [];

    public ViewLocator(
        InstalledPortsProvider installedPortsProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        BitmapsCache bitmapsCache
        )
    {
        _installedPortsProvider = installedPortsProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
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
            CampaignsViewModel campsVm => new CampaignsControl(campsVm, _installedPortsProvider, _installedAddonsProviderFactory.GetSingleton(campsVm.Game), _bitmapsCache),
            MapsViewModel mapsVm => new MapsControl(mapsVm, _installedPortsProvider, _installedAddonsProviderFactory.GetSingleton(mapsVm.Game), _bitmapsCache),
            ModsViewModel modsVM => new ModsControl(_installedAddonsProviderFactory.GetSingleton(modsVM.Game)),
            DownloadsViewModel => new DownloadsControl(),
            _ => throw new NotImplementedException($"Can't find control for {data} ViewModel.")
        };

        _controlsCache.Add(data, newControl);
        return newControl;
    }

    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}
