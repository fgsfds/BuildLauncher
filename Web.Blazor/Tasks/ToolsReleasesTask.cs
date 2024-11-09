﻿using Common.Common.Providers;
using Web.Blazor.Providers;

namespace Web.Blazor.Tasks;

public sealed class ToolsReleasesTask : IHostedService, IDisposable
{
    private readonly ToolsReleasesProvider _toolsReleasesProvider;

    private Timer _timer;

    public ToolsReleasesTask(ToolsReleasesProvider toolsReleasesProvider)
    {
        _toolsReleasesProvider = toolsReleasesProvider;
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
        _ = _toolsReleasesProvider.GetLatestReleasesAsync();
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