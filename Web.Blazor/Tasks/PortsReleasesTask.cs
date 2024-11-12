using Common.Common.Interfaces;
using Common.Entities;
using Common.Enums;

namespace Web.Blazor.Tasks;

public sealed class PortsReleasesTask : IHostedService, IDisposable
{
    private readonly IRetriever<Dictionary<PortEnum, GeneralReleaseEntity>?> _portsReleasesProvider;

    private Timer? _timer;

    public PortsReleasesTask(IRetriever<Dictionary<PortEnum, GeneralReleaseEntity>?> portsReleasesProvider)
    {
        _portsReleasesProvider = portsReleasesProvider;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(6)
            );

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _ = _portsReleasesProvider.RetrieveAsync();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _ = _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}