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

    private readonly Dictionary<object, UserControl> _controlsCache = [];

    public ViewLocator(
        InstalledPortsProvider installedPortsProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory
        )
    {
        _installedPortsProvider = installedPortsProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
    }

    public Control Build(object? data)
    {
        if (data is not null && _controlsCache.TryGetValue(data, out var control))
        {
            return control;
        }

        UserControl newControl = data switch
        {
            CampaignsViewModel campsVm => new CampaignsControl(campsVm, _installedPortsProvider, _installedAddonsProviderFactory.GetSingleton(campsVm.Game)),
            MapsViewModel mapsVm => new MapsControl(mapsVm, _installedPortsProvider, _installedAddonsProviderFactory.GetSingleton(mapsVm.Game)),
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
