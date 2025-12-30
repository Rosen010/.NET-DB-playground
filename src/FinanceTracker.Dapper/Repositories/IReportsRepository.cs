using FinanceTracker.Dapper.Models;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Repository interface for complex reporting queries.
/// These queries demonstrate Dapper's ability to handle
/// JOINs, GROUP BY, and aggregate functions.
/// </summary>
public interface IReportsRepository
{
    /// <summary>
    /// Gets spending grouped by category for a specific month.
    /// </summary>
    /// <param name="year">The year to query.</param>
    /// <param name="month">The month to query (1-12).</param>
    /// <param name="userId">Optional user ID to filter by.</param>
    Task<IEnumerable<SpendingByCategory>> GetMonthlySpendingByCategoryAsync(int year, int month, int? userId = null);

    /// <summary>
    /// Gets account balance summaries grouped by user and account type.
    /// </summary>
    Task<IEnumerable<AccountBalanceSummary>> GetAccountBalanceSummaryAsync();

    /// <summary>
    /// Gets budget status with actual spending for a specific month.
    /// </summary>
    /// <param name="year">The year to query.</param>
    /// <param name="month">The month to query (1-12).</param>
    Task<IEnumerable<BudgetStatus>> GetBudgetStatusAsync(int year, int month);

    /// <summary>
    /// Gets total income and expenses for a date range.
    /// </summary>
    Task<(decimal TotalIncome, decimal TotalExpenses, decimal NetAmount)> GetIncomeExpenseSummaryAsync(
        DateTime startDate, DateTime endDate, int? userId = null);

    /// <summary>
    /// Gets the top spending categories for a user.
    /// </summary>
    Task<IEnumerable<SpendingByCategory>> GetTopSpendingCategoriesAsync(int userId, int topCount = 5);
}
