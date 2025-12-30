using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the user repository.
/// Demonstrates basic CRUD operations with Dapper.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Dapper automatically maps columns to properties when MatchNamesWithUnderscores is enabled
        const string sql = "SELECT id, email, name, created_at FROM users ORDER BY name";

        return await connection.QueryAsync<User>(sql);
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT id, email, name, created_at FROM users WHERE id = @Id";

        // QueryFirstOrDefaultAsync returns null if no record found
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT id, email, name, created_at FROM users WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();

        // RETURNING id is PostgreSQL syntax to get the generated identity value
        const string sql = @"
            INSERT INTO users (email, name)
            VALUES (@Email, @Name)
            RETURNING id";

        // ExecuteScalarAsync returns the single value from RETURNING clause
        return await connection.ExecuteScalarAsync<int>(sql, new { user.Email, user.Name });
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE users
            SET email = @Email, name = @Name
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { user.Id, user.Email, user.Name });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Cascade delete will remove related accounts, transactions, and budgets
        const string sql = "DELETE FROM users WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}
