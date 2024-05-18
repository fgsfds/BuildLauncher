using Superheater.Web.Server.Providers;

namespace Superheater.Web.Server.Tasks
{
    public sealed class PortsReleasesTask : IHostedService, IDisposable
    {
        private readonly ILogger<PortsReleasesTask> _logger;
        private readonly PortsReleasesProvider _portsReleasesProvider;

        private bool _runOnce = false;
        private Timer _timer;

        public PortsReleasesTask(
            ILogger<PortsReleasesTask> logger,
            PortsReleasesProvider portsReleasesProvider
            )
        {
            _logger = logger;
            _portsReleasesProvider = portsReleasesProvider;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            if (!_runOnce)
            {
                _portsReleasesProvider.GetLatestReleasesAsync().Wait(stoppingToken);
                _runOnce = true;
            }

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
            _ = _portsReleasesProvider.GetLatestReleasesAsync();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}