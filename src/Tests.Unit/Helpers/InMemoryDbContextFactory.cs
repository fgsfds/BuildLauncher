using Database.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Unit;

internal sealed class InMemoryDbContextFactory : IDbContextFactory<DatabaseContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _options;

    public InMemoryDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateDbContext();
        context.Database.EnsureCreated();
    }

    public DatabaseContext CreateDbContext()
    {
        return new DatabaseContext(_options);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
