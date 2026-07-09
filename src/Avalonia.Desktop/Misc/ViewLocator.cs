using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Desktop.Controls;
using Avalonia.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Ports.Providers;

namespace Avalonia.Desktop.Misc;

/// <summary>
///     Locates and creates views for view models.
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    private readonly BitmapsCache _bitmapsCache;

    /// <summary>
    ///     Cache of created controls keyed by their data context.
    /// </summary>
    private readonly Dictionary<object, UserControl> _controlsCache = [];

    private readonly PortsProvider _installedPortsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViewLocator" /> class.
    /// </summary>
    /// <param name="installedPortsProvider">The ports provider.</param>
    /// <param name="bitmapsCache">The bitmaps cache.</param>
    public ViewLocator(
        PortsProvider installedPortsProvider,
        BitmapsCache bitmapsCache
        )
    {
        _installedPortsProvider = installedPortsProvider;
        _bitmapsCache = bitmapsCache;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool Match(object? data)
    {
        return data is ObservableObject;
    }
}
