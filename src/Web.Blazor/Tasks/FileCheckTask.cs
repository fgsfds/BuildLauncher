using Database.Server;
using Microsoft.EntityFrameworkCore;

namespace Web.Blazor.Tasks;

internal sealed class FileCheckTask : IHostedService, IDisposable
{
    private readonly ILogger<FileCheckTask> _logger;
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly HttpClient _httpClient;

    private Timer? _timer;

    public FileCheckTask(
        ILogger<FileCheckTask> logger,
        IDbContextFactory<DatabaseContext> dbContextFactory,
        HttpClient httpClient
        )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _httpClient = httpClient;
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

    private async void DoWork(object? state)
    {
        _logger.LogInformation("File check started");

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var files = dbContext.Versions.AsNoTracking().ToList();

        foreach (var file in files)
        {
            try
            {
                if (file.IsDisabled)
                {
                    continue;
                }

                using var result = await _httpClient.GetAsync(file.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError($"File doesn't exist or unavailable: {file.AddonId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while checking file: {file.AddonId}");
            }

        }

        _logger.LogInformation("File check ended");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}