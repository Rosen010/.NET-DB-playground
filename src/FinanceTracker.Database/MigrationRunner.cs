using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Database;

/// <summary>
/// Provides methods to run database migrations using FluentMigrator.
/// </summary>
public static class MigrationRunner
{
    /// <summary>
    /// Runs all pending migrations against the specified PostgreSQL database.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public static void MigrateUp(string connectionString)
    {
        var serviceProvider = CreateServices(connectionString);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }

    /// <summary>
    /// Rolls back the last migration.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public static void MigrateDown(string connectionString)
    {
        var serviceProvider = CreateServices(connectionString);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateDown(0);
    }

    /// <summary>
    /// Rolls back to a specific migration version.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="version">The target version to roll back to.</param>
    public static void MigrateDownTo(string connectionString, long version)
    {
        var serviceProvider = CreateServices(connectionString);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateDown(version);
    }

    /// <summary>
    /// Lists all available migrations and their status.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public static void ListMigrations(string connectionString)
    {
        var serviceProvider = CreateServices(connectionString);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.ListMigrations();
    }

    private static IServiceProvider CreateServices(string connectionString)
    {
        return new ServiceCollection()
            // Add FluentMigrator services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                // Use PostgreSQL
                .AddPostgres()
                // Set the connection string
                .WithGlobalConnectionString(connectionString)
                // Scan this assembly for migrations
                .ScanIn(typeof(MigrationRunner).Assembly).For.Migrations())
            // Enable logging to console
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }
}
