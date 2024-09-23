using Web.Blazor.Providers;

namespace Web.Blazor.Tasks;

public sealed class PortsReleasesTask : IHostedService, IDisposable
{
    private readonly PortsReleasesProvider _portsReleasesProvider;

    private Timer _timer;

    public PortsReleasesTask(PortsReleasesProvider portsReleasesProvider)
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
        _ = _portsReleasesProvider.GetLatestReleasesAsync();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _ = _timer.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}