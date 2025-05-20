using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Helpers;

public sealed class FileLogger : ILogger
{
    private readonly string _path;

    public FileLogger(string path)
    {
        _path = path;

        try
        {
            File.WriteAllText(_path, string.Empty);
        }
        catch
        {
            // nothing to do
        }
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel level) => true;

    public void Log<TState>(
        LogLevel level,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(level))
        {
            return;
        }

        try
        {
            var msg = formatter(state, exception);
            var line = $"[{level + "]",-12}  [{DateTime.Now:dd.MM.yy HH:mm:ss}]  {msg}{Environment.NewLine}";

            File.AppendAllText(_path, line);

            if (exception is not null)
            {
                File.AppendAllText(_path, exception + Environment.NewLine);
            }

        }
        catch
        {
            // nothing to do
        }
    }
}

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    public FileLoggerProvider(string path) => _path = path;
    public ILogger CreateLogger(string categoryName) => new FileLogger(_path);
    public void Dispose() { }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string path)
    {
        _ = builder.Services.AddSingleton<ILoggerProvider>(_ => new FileLoggerProvider(path));
        return builder;
    }
}
