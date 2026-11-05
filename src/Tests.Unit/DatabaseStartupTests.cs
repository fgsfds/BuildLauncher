using Database.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tests.Unit;

/// <summary> Reproduces database startup with recovery files and an abandoned EF migration lock. </summary>
public sealed class DatabaseStartupTests : IDisposable
{
    /// <summary> The isolated test directory. </summary>
    private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary> Creates the test directory. </summary>
    public DatabaseStartupTests() => _ = Directory.CreateDirectory(_root);

    /// <summary> Removes only this test's databases. </summary>
    public void Dispose() => Directory.Delete(_root, true);

    /// <summary> Creates a non-pooled SQLite context using the application's migrations. </summary>
    /// <param name="name">The test database filename.</param>
    /// <returns>The context.</returns>
    private DatabaseContext Context(string name)
    {
        var connection = new SqliteConnectionStringBuilder { DataSource = Path.Combine(_root, name), Pooling = false };
        return new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connection.ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options);
    }

    /// <summary> Valid WAL/SHM files do not prevent EF startup while another connection remains open. </summary>
    [Fact]
    public async Task Migrate_ValidWalAndOpenConnection_PreservesCommittedData()
    {
        await using var writer = Context("healthy.db");
        await writer.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await writer.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("PRAGMA wal_autocheckpoint=0; INSERT INTO settings(name,value) VALUES ('recovery-test','committed');", TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(_root, "healthy.db-wal")));
        Assert.True(File.Exists(Path.Combine(_root, "healthy.db-shm")));

        await using var reader = Context("healthy.db");
        await reader.Database.MigrateAsync(TestContext.Current.CancellationToken);
        Assert.Equal("committed", (await reader.Settings.SingleAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken)).Value);
    }

    /// <summary> A committed migration-lock row makes the existing startup wait indefinitely, even with no pending migrations. </summary>
    [Fact]
    public async Task Migrate_AbandonedMigrationLock_BlocksUntilCanceled()
    {
        await using var writer = Context("abandoned.db");
        await writer.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await writer.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("PRAGMA wal_autocheckpoint=0; INSERT INTO settings(name,value) VALUES ('recovery-test','committed'); INSERT INTO __EFMigrationsLock(Id,Timestamp) VALUES (1,'2025-10-11 00:00:00');", TestContext.Current.CancellationToken);
        Assert.True(File.Exists(Path.Combine(_root, "abandoned.db-wal")));
        await using var reader = Context("abandoned.db");
        Assert.Empty(await reader.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(350));
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.Database.MigrateAsync(timeout.Token));
        Assert.Equal("committed", (await reader.Settings.SingleAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken)).Value);
    }

    /// <summary> Recovery preserves uncheckpointed commits after restart and applies pending migrations. </summary>
    /// <param name="pendingMigration">Whether the snapshot still needs the latest migration.</param>
    /// <param name="includeSharedMemory">Whether the restart also retains the shared-memory file.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Initialize_CrashSnapshot_RecoversLockAndPreservesWal(bool pendingMigration, bool includeSharedMemory)
    {
        await using var writer = Context("source.db");
        await writer.GetService<IMigrator>().MigrateAsync(pendingMigration ? "20250523132855_AddFavoritesTable" : null, TestContext.Current.CancellationToken);
        await writer.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE); PRAGMA wal_autocheckpoint=0;", TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("INSERT INTO settings(name,value) VALUES ('recovery-test','committed'); INSERT INTO __EFMigrationsLock(Id,Timestamp) VALUES (1,'2025-10-11 00:00:00');", TestContext.Current.CancellationToken);

        // No writes occur while copying this isolated database and its recovery files.
        File.Copy(Path.Combine(_root, "source.db"), Path.Combine(_root, "restart.db"));
        File.Copy(Path.Combine(_root, "source.db-wal"), Path.Combine(_root, "restart.db-wal"));
        if (includeSharedMemory)
        {
            File.Copy(Path.Combine(_root, "source.db-shm"), Path.Combine(_root, "restart.db-shm"));
        }

        File.Copy(Path.Combine(_root, "source.db"), Path.Combine(_root, "without-wal.db"));
        await using (var withoutWal = Context("without-wal.db"))
        {
            Assert.False(await withoutWal.Settings.AnyAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken));
        }

        await using var recovered = Context("restart.db");
        Assert.Equal(1, await DatabaseInitializer.InitializeAsync(recovered, TestContext.Current.CancellationToken));
        Assert.Equal("committed", (await recovered.Settings.SingleAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken)).Value);
        Assert.Empty(await recovered.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await DatabaseInitializer.InitializeAsync(recovered, TestContext.Current.CancellationToken));
        Assert.Equal("committed", (await writer.Settings.SingleAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken)).Value);
    }

    /// <summary> Recovery never clears a migration lock while another startup holds exclusive ownership. </summary>
    [Fact]
    public async Task Initialize_ActiveStartupLock_CancelsWithoutClearingEfLock()
    {
        await using var writer = Context("busy.db");
        await writer.Database.MigrateAsync(TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("INSERT INTO __EFMigrationsLock(Id,Timestamp) VALUES (1,'2025-10-11 00:00:00');", TestContext.Current.CancellationToken);
        await using var reader = Context("busy.db");
        using (var held = new FileStream(Path.Combine(_root, "busy.db.startup.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
            _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DatabaseInitializer.InitializeAsync(reader, cancellation.Token));
            await writer.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
            using var command = writer.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM __EFMigrationsLock;";
            Assert.Equal(1L, await command.ExecuteScalarAsync(TestContext.Current.CancellationToken));
        }

        Assert.Equal(1, await DatabaseInitializer.InitializeAsync(reader, TestContext.Current.CancellationToken));
    }

    /// <summary> An open connection and valid recovery files remain usable during normal initialization. </summary>
    [Fact]
    public async Task Initialize_ValidWal_PreservesOpenConnectionAndUnrelatedFiles()
    {
        await using var writer = Context("normal.db");
        await writer.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await writer.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        _ = await writer.Database.ExecuteSqlRawAsync("INSERT INTO settings(name,value) VALUES ('recovery-test','committed');", TestContext.Current.CancellationToken);
        var unrelated = Path.Combine(_root, "other.db-wal");
        await File.WriteAllTextAsync(unrelated, "unrelated recovery data", TestContext.Current.CancellationToken);
        await using var reader = Context("normal.db");
        Assert.Equal(0, await DatabaseInitializer.InitializeAsync(reader, TestContext.Current.CancellationToken));
        Assert.Equal("committed", (await writer.Settings.SingleAsync(x => x.Name == "recovery-test", TestContext.Current.CancellationToken)).Value);
        Assert.Equal("unrelated recovery data", await File.ReadAllTextAsync(unrelated, TestContext.Current.CancellationToken));
        Assert.Equal(0, await DatabaseInitializer.InitializeAsync(writer, TestContext.Current.CancellationToken));
        #pragma warning disable IDISP004 // The DbContext owns this borrowed connection.
        Assert.Equal(System.Data.ConnectionState.Open, writer.Database.GetDbConnection().State);
        #pragma warning restore IDISP004
    }

    /// <summary> Fresh databases migrate normally and initialize repeatedly. </summary>
    [Fact]
    public async Task Initialize_FreshDatabase_CreatesSchema()
    {
        await using var context = Context("new.db");
        Assert.Equal(0, await DatabaseInitializer.InitializeAsync(context, TestContext.Current.CancellationToken));
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await DatabaseInitializer.InitializeAsync(context, TestContext.Current.CancellationToken));
    }

    /// <summary> Genuine corruption is reported instead of deleting journals or resetting user data. </summary>
    [Fact]
    public async Task Initialize_CorruptDatabase_PreservesEvidence()
    {
        var database = Path.Combine(_root, "corrupt.db");
        var bytes = new byte[4096];
        Array.Fill(bytes, (byte)0x7f);
        await File.WriteAllBytesAsync(database, bytes, TestContext.Current.CancellationToken);
        await using var context = Context("corrupt.db");
        var error = await Assert.ThrowsAsync<SqliteException>(() => DatabaseInitializer.InitializeAsync(context, TestContext.Current.CancellationToken));
        Assert.Equal(26, error.SqliteErrorCode);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(database, TestContext.Current.CancellationToken));
        using var released = new FileStream(database + ".startup.lock", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(released.CanWrite);
    }

    /// <summary> A pre-canceled startup creates neither a database nor a startup lock. </summary>
    [Fact]
    public async Task Initialize_PreCanceled_DoesNotCreateFiles()
    {
        await using var context = Context("canceled.db");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DatabaseInitializer.InitializeAsync(context, cancellation.Token));
        Assert.Empty(Directory.GetFiles(_root));
    }

    /// <summary> A queued startup succeeds after its predecessor releases ownership. </summary>
    [Fact]
    public async Task Initialize_StartupLockReleased_ResumesWaitingCaller()
    {
        var held = new FileStream(Path.Combine(_root, "queued.db.startup.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        await using var context = Context("queued.db");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var operation = DatabaseInitializer.InitializeAsync(context, timeout.Token);
        try
        {
            Assert.False(operation.IsCompleted);
            Assert.False(File.Exists(Path.Combine(_root, "queued.db")));
        }
        finally
        {
            held.Dispose();
            await operation;
        }

        Assert.Equal(0, await operation);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
    }
}
