using Database.Server;

namespace Web.Blazor.Tasks;

public sealed class FileCheckTask : IHostedService, IDisposable
{
    private readonly ILogger<AppReleasesTask> _logger;
    private readonly DatabaseContextFactory _dbContextFactory;
    private readonly HttpClient _httpClient;

    private Timer _timer;

    public FileCheckTask(
        ILogger<AppReleasesTask> logger,
        DatabaseContextFactory dbContextFactory,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _httpClient = httpClient;
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
        _logger.LogInformation("File check started");

        using var dbContext = _dbContextFactory.Get();
        var files = dbContext.Versions.Select(v => v.DownloadUrl).ToList();

        foreach (var file in files)
        {
            var result = _httpClient.GetAsync(file, HttpCompletionOption.ResponseHeadersRead).Result;

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError($"File doesn't exist or unavailable: {file}");
                continue;
            }
        }

        _logger.LogInformation("File check ended");
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