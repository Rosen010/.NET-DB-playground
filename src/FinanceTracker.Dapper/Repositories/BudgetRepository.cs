using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the budget repository.
/// Budgets are user-specific limits for spending categories.
/// </summary>
public class BudgetRepository : IBudgetRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public BudgetRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Budget>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, category_id, amount, period, start_date
            FROM budgets
            ORDER BY user_id, category_id";

        return await connection.QueryAsync<Budget>(sql);
    }

    /// <inheritdoc />
    public async Task<Budget?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, category_id, amount, period, start_date
            FROM budgets
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Budget>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Budget>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, user_id, category_id, amount, period, start_date
            FROM budgets
            WHERE user_id = @UserId
            ORDER BY category_id";

        return await connection.QueryAsync<Budget>(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Budget budget)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO budgets (user_id, category_id, amount, period, start_date)
            VALUES (@UserId, @CategoryId, @Amount, @Period, @StartDate)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            budget.UserId,
            budget.CategoryId,
            budget.Amount,
            Period = (int)budget.Period,
            StartDate = budget.StartDate.Date
        });
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Budget budget)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE budgets
            SET category_id = @CategoryId,
                amount = @Amount,
                period = @Period,
                start_date = @StartDate
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            budget.Id,
            budget.CategoryId,
            budget.Amount,
            Period = (int)budget.Period,
            StartDate = budget.StartDate.Date
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM budgets WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}
