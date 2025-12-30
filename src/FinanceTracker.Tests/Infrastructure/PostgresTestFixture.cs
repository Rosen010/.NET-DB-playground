using Testcontainers.PostgreSql;
using FinanceTracker.Database;
using FinanceTracker.Dapper.Data;

namespace FinanceTracker.Tests.Infrastructure;

/// <summary>
/// Shared test fixture that provides a PostgreSQL database container for integration tests.
/// Uses Testcontainers to spin up an isolated PostgreSQL instance.
/// Implements IAsyncLifetime to handle async setup and teardown.
/// </summary>
public class PostgresTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public string ConnectionString => _postgresContainer.GetConnectionString();
    public DbConnectionFactory ConnectionFactory => new(ConnectionString);

    public PostgresTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("finance_tracker_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL container
        await _postgresContainer.StartAsync();

        // Run migrations to set up the schema (without seed data for cleaner tests)
        MigrationRunner.MigrateUp(ConnectionString);

        // Configure Dapper for snake_case mapping
        DapperConfig.Configure();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for sharing the PostgreSQL fixture across test classes.
/// This ensures a single container is used for all tests in the collection.
/// </summary>
[CollectionDefinition("PostgreSQL")]
public class PostgresCollectionFixture : ICollectionFixture<PostgresTestFixture>
{
    // This class has no code; it's just a marker for xUnit to create the collection
}
