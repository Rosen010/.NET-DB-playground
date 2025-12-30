using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the account repository.
/// Demonstrates parameterized queries and decimal handling.
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public AccountRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, name, type, balance, currency, created_at
            FROM accounts
            ORDER BY user_id, name";

        return await connection.QueryAsync<Account>(sql);
    }

    /// <inheritdoc />
    public async Task<Account?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, name, type, balance, currency, created_at
            FROM accounts
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Account>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, name, type, balance, currency, created_at
            FROM accounts
            WHERE user_id = @UserId
            ORDER BY name";

        return await connection.QueryAsync<Account>(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO accounts (user_id, name, type, balance, currency)
            VALUES (@UserId, @Name, @Type, @Balance, @Currency)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            account.UserId,
            account.Name,
            Type = (int)account.Type,
            account.Balance,
            account.Currency
        });
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE accounts
            SET name = @Name, type = @Type, balance = @Balance, currency = @Currency
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            account.Id,
            account.Name,
            Type = (int)account.Type,
            account.Balance,
            account.Currency
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateBalanceAsync(int id, decimal newBalance)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "UPDATE accounts SET balance = @Balance WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Balance = newBalance });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Cascade delete will remove related transactions
        const string sql = "DELETE FROM accounts WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}
