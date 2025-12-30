using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Dapper.Models;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the reports repository.
///
/// This class demonstrates several advanced Dapper and PostgreSQL patterns:
/// - Complex JOINs across multiple tables
/// - GROUP BY with aggregate functions (SUM, COUNT)
/// - Conditional filtering with optional parameters
/// - Date range queries using PostgreSQL date functions
/// - CASE expressions for conditional logic
/// - LEFT JOINs to include records without matches
/// </summary>
public class ReportsRepository : IReportsRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ReportsRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpendingByCategory>> GetMonthlySpendingByCategoryAsync(
        int year, int month, int? userId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        // This query demonstrates:
        // - JOINs: transactions -> accounts -> categories
        // - Aggregate functions: SUM, COUNT
        // - PostgreSQL date functions: EXTRACT
        // - Optional filtering with conditional WHERE clause
        // - GROUP BY with multiple columns
        // - ORDER BY aggregate result
        const string sql = @"
            SELECT
                c.id AS CategoryId,
                c.name AS CategoryName,
                CASE c.type
                    WHEN 1 THEN 'Expense'
                    WHEN 2 THEN 'Income'
                END AS CategoryType,
                COALESCE(SUM(ABS(t.amount)), 0) AS TotalAmount,
                COUNT(t.id) AS TransactionCount
            FROM categories c
            LEFT JOIN transactions t ON t.category_id = c.id
                AND EXTRACT(YEAR FROM t.transaction_date) = @Year
                AND EXTRACT(MONTH FROM t.transaction_date) = @Month
            LEFT JOIN accounts a ON t.account_id = a.id
            WHERE (@UserId IS NULL OR a.user_id = @UserId)
            GROUP BY c.id, c.name, c.type
            HAVING COUNT(t.id) > 0
            ORDER BY TotalAmount DESC";

        return await connection.QueryAsync<SpendingByCategory>(sql, new { Year = year, Month = month, UserId = userId });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AccountBalanceSummary>> GetAccountBalanceSummaryAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // This query demonstrates:
        // - JOIN between users and accounts
        // - CASE expression for enum-to-string conversion
        // - GROUP BY with aggregates
        // - Multiple aggregate functions in one query
        const string sql = @"
            SELECT
                u.id AS UserId,
                u.name AS UserName,
                CASE a.type
                    WHEN 1 THEN 'Checking'
                    WHEN 2 THEN 'Savings'
                    WHEN 3 THEN 'Credit Card'
                    WHEN 4 THEN 'Cash'
                    WHEN 5 THEN 'Investment'
                END AS AccountType,
                COUNT(a.id) AS AccountCount,
                SUM(a.balance) AS TotalBalance
            FROM users u
            JOIN accounts a ON a.user_id = u.id
            GROUP BY u.id, u.name, a.type
            ORDER BY u.name, a.type";

        return await connection.QueryAsync<AccountBalanceSummary>(sql);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<BudgetStatus>> GetBudgetStatusAsync(int year, int month)
    {
        using var connection = _connectionFactory.CreateConnection();

        // This query demonstrates:
        // - Complex subquery for calculating spent amounts
        // - LEFT JOIN to include budgets with no spending
        // - COALESCE for handling NULL values
        // - Multiple JOINs (budgets -> users, budgets -> categories)
        // - Correlated subquery for period-specific spending
        const string sql = @"
            SELECT
                b.id AS BudgetId,
                u.id AS UserId,
                u.name AS UserName,
                c.name AS CategoryName,
                b.amount AS BudgetAmount,
                COALESCE(
                    (SELECT SUM(ABS(t.amount))
                     FROM transactions t
                     JOIN accounts a ON t.account_id = a.id
                     WHERE t.category_id = b.category_id
                       AND a.user_id = b.user_id
                       AND t.amount < 0
                       AND EXTRACT(YEAR FROM t.transaction_date) = @Year
                       AND EXTRACT(MONTH FROM t.transaction_date) = @Month
                    ), 0
                ) AS SpentAmount,
                CASE b.period
                    WHEN 1 THEN 'Monthly'
                    WHEN 2 THEN 'Yearly'
                END AS Period
            FROM budgets b
            JOIN users u ON b.user_id = u.id
            JOIN categories c ON b.category_id = c.id
            WHERE b.period = 1  -- Only monthly budgets for month comparison
            ORDER BY u.name, c.name";

        return await connection.QueryAsync<BudgetStatus>(sql, new { Year = year, Month = month });
    }

    /// <inheritdoc />
    public async Task<(decimal TotalIncome, decimal TotalExpenses, decimal NetAmount)> GetIncomeExpenseSummaryAsync(
        DateTime startDate, DateTime endDate, int? userId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        // This query demonstrates:
        // - Conditional aggregation with CASE and SUM
        // - Date range filtering
        // - Optional user filtering
        // - Returning multiple aggregates in one query
        const string sql = @"
            SELECT
                COALESCE(SUM(CASE WHEN t.amount > 0 THEN t.amount ELSE 0 END), 0) AS TotalIncome,
                COALESCE(SUM(CASE WHEN t.amount < 0 THEN ABS(t.amount) ELSE 0 END), 0) AS TotalExpenses,
                COALESCE(SUM(t.amount), 0) AS NetAmount
            FROM transactions t
            JOIN accounts a ON t.account_id = a.id
            WHERE t.transaction_date >= @StartDate
              AND t.transaction_date <= @EndDate
              AND (@UserId IS NULL OR a.user_id = @UserId)";

        var result = await connection.QueryFirstAsync<dynamic>(sql, new
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            UserId = userId
        });

        return (result.totalincome, result.totalexpenses, result.netamount);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpendingByCategory>> GetTopSpendingCategoriesAsync(int userId, int topCount = 5)
    {
        using var connection = _connectionFactory.CreateConnection();

        // This query demonstrates:
        // - LIMIT clause for top-N queries
        // - Filtering by user through account relationship
        // - Only expense transactions (amount < 0)
        const string sql = @"
            SELECT
                c.id AS CategoryId,
                c.name AS CategoryName,
                'Expense' AS CategoryType,
                SUM(ABS(t.amount)) AS TotalAmount,
                COUNT(t.id) AS TransactionCount
            FROM transactions t
            JOIN accounts a ON t.account_id = a.id
            JOIN categories c ON t.category_id = c.id
            WHERE a.user_id = @UserId
              AND t.amount < 0
            GROUP BY c.id, c.name
            ORDER BY TotalAmount DESC
            LIMIT @TopCount";

        return await connection.QueryAsync<SpendingByCategory>(sql, new { UserId = userId, TopCount = topCount });
    }
}
