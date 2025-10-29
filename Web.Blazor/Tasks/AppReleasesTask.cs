using Common.All.Providers;

namespace Web.Blazor.Tasks;

public sealed class AppReleasesTask : IHostedService, IDisposable
{
    private readonly RepoAppReleasesProvider _appReleasesProvider;

    private Timer? _timer;

    public AppReleasesTask(RepoAppReleasesProvider appReleasesProvider)
    {
        _appReleasesProvider = appReleasesProvider;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(1)
            );

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _ = _appReleasesProvider.GetLatestReleaseAsync(false);
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