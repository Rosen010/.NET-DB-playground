using Npgsql;

namespace FinanceTracker.AdoNet.Data;

/// <summary>
/// Factory for creating PostgreSQL connections using raw ADO.NET.
///
/// ADO.NET is the lowest-level data access technology in .NET.
/// It provides direct control over database operations but requires
/// more boilerplate code compared to ORMs like Dapper or EF Core.
/// </summary>
public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a new PostgreSQL connection.
    /// The caller is responsible for opening and disposing the connection.
    /// </summary>
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
