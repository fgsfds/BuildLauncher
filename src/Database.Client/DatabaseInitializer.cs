using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Database.Client;

/// <summary> Serializes application startup migrations and recovers abandoned EF locks without deleting SQLite journals. </summary>
public static class DatabaseInitializer
{
    /// <summary> Applies migrations while holding a crash-released lock beside the database. </summary>
    /// <param name="context">The application's file-backed SQLite database.</param>
    /// <param name="cancellationToken">Cancels startup or waiting for another startup migration.</param>
    /// <returns>The number of abandoned EF migration-lock rows removed.</returns>
    /// <remarks>All application startup migrations must use this entry point. External migration tools must not run concurrently.</remarks>
    public static async Task<int> InitializeAsync(DatabaseContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var connection = context.Database.GetDbConnection();
        var dataSource = connection.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
        {
            throw new ArgumentException("Startup recovery requires a file-backed SQLite database.", nameof(context));
        }

        var lockPath = Path.GetFullPath(dataSource) + ".startup.lock";
        var startupLock = await AcquireStartupLockAsync(lockPath, cancellationToken).ConfigureAwait(false);
        await using var startupLockScope = startupLock.ConfigureAwait(false);
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen)
            {
                await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            var removedLocks = 0;
            var command = connection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsLock';";
                var lockTableExists = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false), System.Globalization.CultureInfo.InvariantCulture) != 0;
                if (lockTableExists)
                {
                    // SQLite's write transaction also waits for any active database writer before recovery.
                    var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                    await using (transaction.ConfigureAwait(false))
                    {
                        removedLocks = await context.Database.ExecuteSqlRawAsync("DELETE FROM __EFMigrationsLock;", cancellationToken).ConfigureAwait(false);
                        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            return removedLocks;
        }
        finally
        {
            if (!wasOpen)
            {
                await context.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }
    }

    /// <summary> Waits for exclusive startup ownership without treating a leftover lock filename as an active lock. </summary>
    /// <param name="path">The database-specific startup lock file.</param>
    /// <param name="cancellationToken">Cancels waiting for another process.</param>
    /// <returns>The open handle that owns the startup lock.</returns>
    private static async Task<FileStream> AcquireStartupLockAsync(string path, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var wait = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            while (true)
            {
                wait.Token.ThrowIfCancellationRequested();
                try
                {
                    // Keep the filename after disposal: unlinking it could let two processes lock different files.
                    return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException) when (!wait.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), wait.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Another process is initializing the database. Close the other BuildLauncher instance and retry. Startup lock: {path}");
        }
    }
}
