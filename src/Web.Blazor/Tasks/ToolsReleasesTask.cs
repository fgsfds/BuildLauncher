namespace Web.Blazor.Tasks;

internal sealed class ToolsReleasesTask : IHostedService, IDisposable
{
    //private readonly ToolsReleasesRepoRetriever _toolsReleasesProvider;

    private Timer? _timer;

    public ToolsReleasesTask(
        //ToolsReleasesRepoRetriever toolsReleasesProvider
        )
    {
        //_toolsReleasesProvider = toolsReleasesProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
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
        //_ = _toolsReleasesProvider.GetLatestReleasesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}