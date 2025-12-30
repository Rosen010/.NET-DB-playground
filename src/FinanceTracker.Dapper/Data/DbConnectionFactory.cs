using System.Data;
using Npgsql;

namespace FinanceTracker.Dapper.Data;

/// <summary>
/// Factory for creating database connections.
/// Centralizes connection creation and ensures consistent connection handling.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a new database connection.
    /// The caller is responsible for disposing the connection.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
