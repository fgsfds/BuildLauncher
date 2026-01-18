using Common.All.Enums;
using Common.All.Interfaces;

namespace Web.Blazor.Tasks;

internal sealed class PortsReleasesTask : IHostedService, IDisposable
{
    private readonly IReleaseProvider<PortEnum> _portsReleasesProvider;

    private Timer? _timer;

    public PortsReleasesTask(IReleaseProvider<PortEnum> portsReleasesProvider)
    {
        _portsReleasesProvider = portsReleasesProvider;
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
        //_ = _portsReleasesProvider.RetrieveAsync();
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