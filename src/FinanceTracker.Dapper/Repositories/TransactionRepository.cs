using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the transaction repository.
/// Demonstrates date range queries and ordering.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public TransactionRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, account_id, category_id, amount, description, transaction_date, created_at
            FROM transactions
            ORDER BY transaction_date DESC";

        return await connection.QueryAsync<Transaction>(sql);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, account_id, category_id, amount, description, transaction_date, created_at
            FROM transactions
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Transaction>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, account_id, category_id, amount, description, transaction_date, created_at
            FROM transactions
            WHERE account_id = @AccountId
            ORDER BY transaction_date DESC";

        return await connection.QueryAsync<Transaction>(sql, new { AccountId = accountId });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();

        // PostgreSQL date comparison with parameterized dates
        const string sql = @"
            SELECT id, account_id, category_id, amount, description, transaction_date, created_at
            FROM transactions
            WHERE transaction_date >= @StartDate AND transaction_date <= @EndDate
            ORDER BY transaction_date DESC";

        return await connection.QueryAsync<Transaction>(sql, new
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date
        });
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO transactions (account_id, category_id, amount, description, transaction_date)
            VALUES (@AccountId, @CategoryId, @Amount, @Description, @TransactionDate)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Amount,
            transaction.Description,
            TransactionDate = transaction.TransactionDate.Date
        });
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE transactions
            SET account_id = @AccountId,
                category_id = @CategoryId,
                amount = @Amount,
                description = @Description,
                transaction_date = @TransactionDate
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Amount,
            transaction.Description,
            TransactionDate = transaction.TransactionDate.Date
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM transactions WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}
