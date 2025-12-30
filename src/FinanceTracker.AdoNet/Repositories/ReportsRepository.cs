using Npgsql;
using NpgsqlTypes;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.AdoNet.Models;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// Reports repository using raw ADO.NET.
///
/// This class demonstrates complex SQL queries executed directly
/// without any ORM abstraction. Full control over the SQL but
/// requires manual result mapping.
///
/// ADO.NET Patterns Demonstrated:
/// - Complex JOINs and GROUP BY queries
/// - Conditional parameter handling
/// - Multiple result columns mapping
/// - Aggregate function results
/// </summary>
public class ReportsRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ReportsRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<SpendingByCategory>> GetMonthlySpendingByCategoryAsync(
        int year, int month, int? userId = null)
    {
        var results = new List<SpendingByCategory>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        // Build SQL with optional user filter
        var sql = @"
            SELECT
                c.id AS category_id,
                c.name AS category_name,
                CASE c.type WHEN 1 THEN 'Expense' WHEN 2 THEN 'Income' END AS category_type,
                COALESCE(SUM(ABS(t.amount)), 0) AS total_amount,
                COUNT(t.id) AS transaction_count
            FROM categories c
            LEFT JOIN transactions t ON t.category_id = c.id
                AND EXTRACT(YEAR FROM t.transaction_date) = @year
                AND EXTRACT(MONTH FROM t.transaction_date) = @month
            LEFT JOIN accounts a ON t.account_id = a.id";

        if (userId.HasValue)
        {
            sql += " WHERE a.user_id = @userId";
        }

        sql += @"
            GROUP BY c.id, c.name, c.type
            HAVING COUNT(t.id) > 0
            ORDER BY total_amount DESC";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@month", month);
        if (userId.HasValue)
        {
            command.Parameters.AddWithValue("@userId", userId.Value);
        }

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new SpendingByCategory
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name")),
                CategoryType = reader.GetString(reader.GetOrdinal("category_type")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("total_amount")),
                TransactionCount = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("transaction_count")))
            });
        }

        return results;
    }

    public async Task<List<AccountBalanceSummary>> GetAccountBalanceSummaryAsync()
    {
        var results = new List<AccountBalanceSummary>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT
                u.id AS user_id,
                u.name AS user_name,
                CASE a.type
                    WHEN 1 THEN 'Checking'
                    WHEN 2 THEN 'Savings'
                    WHEN 3 THEN 'Credit Card'
                    WHEN 4 THEN 'Cash'
                    WHEN 5 THEN 'Investment'
                END AS account_type,
                COUNT(a.id) AS account_count,
                SUM(a.balance) AS total_balance
            FROM users u
            JOIN accounts a ON a.user_id = u.id
            GROUP BY u.id, u.name, a.type
            ORDER BY u.name, a.type";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new AccountBalanceSummary
            {
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                UserName = reader.GetString(reader.GetOrdinal("user_name")),
                AccountType = reader.GetString(reader.GetOrdinal("account_type")),
                AccountCount = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("account_count"))),
                TotalBalance = reader.GetDecimal(reader.GetOrdinal("total_balance"))
            });
        }

        return results;
    }

    public async Task<List<BudgetStatus>> GetBudgetStatusAsync(int year, int month)
    {
        var results = new List<BudgetStatus>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT
                b.id AS budget_id,
                u.id AS user_id,
                u.name AS user_name,
                c.name AS category_name,
                b.amount AS budget_amount,
                COALESCE(
                    (SELECT SUM(ABS(t.amount))
                     FROM transactions t
                     JOIN accounts a ON t.account_id = a.id
                     WHERE t.category_id = b.category_id
                       AND a.user_id = b.user_id
                       AND t.amount < 0
                       AND EXTRACT(YEAR FROM t.transaction_date) = @year
                       AND EXTRACT(MONTH FROM t.transaction_date) = @month
                    ), 0
                ) AS spent_amount,
                CASE b.period WHEN 1 THEN 'Monthly' WHEN 2 THEN 'Yearly' END AS period
            FROM budgets b
            JOIN users u ON b.user_id = u.id
            JOIN categories c ON b.category_id = c.id
            WHERE b.period = 1
            ORDER BY u.name, c.name";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@year", year);
        command.Parameters.AddWithValue("@month", month);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new BudgetStatus
            {
                BudgetId = reader.GetInt32(reader.GetOrdinal("budget_id")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                UserName = reader.GetString(reader.GetOrdinal("user_name")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name")),
                BudgetAmount = reader.GetDecimal(reader.GetOrdinal("budget_amount")),
                SpentAmount = reader.GetDecimal(reader.GetOrdinal("spent_amount")),
                Period = reader.GetString(reader.GetOrdinal("period"))
            });
        }

        return results;
    }

    public async Task<(decimal TotalIncome, decimal TotalExpenses, decimal NetAmount)> GetIncomeExpenseSummaryAsync(
        DateTime startDate, DateTime endDate, int? userId = null)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT
                COALESCE(SUM(CASE WHEN t.amount > 0 THEN t.amount ELSE 0 END), 0) AS total_income,
                COALESCE(SUM(CASE WHEN t.amount < 0 THEN ABS(t.amount) ELSE 0 END), 0) AS total_expenses,
                COALESCE(SUM(t.amount), 0) AS net_amount
            FROM transactions t
            JOIN accounts a ON t.account_id = a.id
            WHERE t.transaction_date >= @startDate
              AND t.transaction_date <= @endDate";

        if (userId.HasValue)
        {
            sql += " AND a.user_id = @userId";
        }

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("@startDate", NpgsqlDbType.Date) { Value = startDate.Date });
        command.Parameters.Add(new NpgsqlParameter("@endDate", NpgsqlDbType.Date) { Value = endDate.Date });

        if (userId.HasValue)
        {
            command.Parameters.AddWithValue("@userId", userId.Value);
        }

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return (
                reader.GetDecimal(reader.GetOrdinal("total_income")),
                reader.GetDecimal(reader.GetOrdinal("total_expenses")),
                reader.GetDecimal(reader.GetOrdinal("net_amount"))
            );
        }

        return (0, 0, 0);
    }

    public async Task<List<SpendingByCategory>> GetTopSpendingCategoriesAsync(int userId, int topCount = 5)
    {
        var results = new List<SpendingByCategory>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT
                c.id AS category_id,
                c.name AS category_name,
                'Expense' AS category_type,
                SUM(ABS(t.amount)) AS total_amount,
                COUNT(t.id) AS transaction_count
            FROM transactions t
            JOIN accounts a ON t.account_id = a.id
            JOIN categories c ON t.category_id = c.id
            WHERE a.user_id = @userId
              AND t.amount < 0
            GROUP BY c.id, c.name
            ORDER BY total_amount DESC
            LIMIT @topCount";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@topCount", topCount);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new SpendingByCategory
            {
                CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name")),
                CategoryType = reader.GetString(reader.GetOrdinal("category_type")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("total_amount")),
                TransactionCount = Convert.ToInt32(reader.GetInt64(reader.GetOrdinal("transaction_count")))
            });
        }

        return results;
    }
}
