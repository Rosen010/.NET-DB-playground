using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Enums;
using FinanceTracker.EFCore.Data;
using FinanceTracker.EFCore.Models;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for complex reporting queries using EF Core LINQ.
///
/// This class demonstrates EF Core's powerful LINQ capabilities:
/// - GroupBy with aggregate functions (Sum, Count)
/// - Navigation property joins (automatic via Include or implicit)
/// - Projection to DTOs using Select
/// - Conditional filtering with Where
/// - Complex predicates with date functions
///
/// Comparison to Dapper:
/// - EF Core: Type-safe, compile-time checked queries
/// - Dapper: Raw SQL with full control
/// - EF Core translates LINQ to SQL automatically
/// </summary>
public class ReportsService
{
    private readonly FinanceDbContext _context;

    public ReportsService(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets spending grouped by category for a specific month.
    ///
    /// EF Core LINQ patterns demonstrated:
    /// - Where with date extraction (Year, Month properties)
    /// - GroupBy with projection
    /// - Aggregate functions (Sum, Count)
    /// - Conditional filtering with null parameter
    /// </summary>
    public async Task<List<SpendingByCategory>> GetMonthlySpendingByCategoryAsync(
        int year, int month, int? userId = null)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.TransactionDate.Year == year && t.TransactionDate.Month == month);

        // Optional user filter
        if (userId.HasValue)
        {
            query = query.Where(t => t.Account!.UserId == userId.Value);
        }

        var result = await query
            .GroupBy(t => new { t.CategoryId, t.Category!.Name, t.Category.Type })
            .Select(g => new SpendingByCategory
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryType = g.Key.Type == CategoryType.Expense ? "Expense" : "Income",
                TotalAmount = g.Sum(t => Math.Abs(t.Amount)),
                TransactionCount = g.Count()
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToListAsync();

        return result;
    }

    /// <summary>
    /// Gets account balance summaries grouped by user and account type.
    ///
    /// EF Core LINQ patterns demonstrated:
    /// - Multiple navigation properties in GroupBy
    /// - Enum to string conversion
    /// </summary>
    public async Task<List<AccountBalanceSummary>> GetAccountBalanceSummaryAsync()
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.User)
            .GroupBy(a => new { a.UserId, a.User!.Name, a.Type })
            .Select(g => new AccountBalanceSummary
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                AccountType = g.Key.Type.ToString(),
                AccountCount = g.Count(),
                TotalBalance = g.Sum(a => a.Balance)
            })
            .OrderBy(s => s.UserName)
            .ThenBy(s => s.AccountType)
            .ToListAsync();
    }

    /// <summary>
    /// Gets budget status with actual spending for a specific month.
    ///
    /// This is a complex query that demonstrates:
    /// - Joining multiple entities via navigation properties
    /// - Subquery for calculating spending (using Let or nested query)
    /// - Conditional aggregation
    /// </summary>
    public async Task<List<BudgetStatus>> GetBudgetStatusAsync(int year, int month)
    {
        // Get monthly budgets with their categories and users
        var budgets = await _context.Budgets
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Category)
            .Where(b => b.Period == BudgetPeriod.Monthly)
            .ToListAsync();

        // Get spending for the month grouped by user and category
        var spending = await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.TransactionDate.Year == year
                     && t.TransactionDate.Month == month
                     && t.Amount < 0) // Expenses only
            .GroupBy(t => new { t.Account!.UserId, t.CategoryId })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.CategoryId,
                TotalSpent = g.Sum(t => Math.Abs(t.Amount))
            })
            .ToListAsync();

        // Join in memory (for complex scenarios, sometimes it's cleaner)
        var result = budgets.Select(b =>
        {
            var spent = spending
                .FirstOrDefault(s => s.UserId == b.UserId && s.CategoryId == b.CategoryId);

            return new BudgetStatus
            {
                BudgetId = b.Id,
                UserId = b.UserId,
                UserName = b.User?.Name ?? "Unknown",
                CategoryName = b.Category?.Name ?? "Unknown",
                BudgetAmount = b.Amount,
                SpentAmount = spent?.TotalSpent ?? 0,
                Period = b.Period.ToString()
            };
        })
        .OrderBy(b => b.UserName)
        .ThenBy(b => b.CategoryName)
        .ToList();

        return result;
    }

    /// <summary>
    /// Gets total income and expenses for a date range.
    ///
    /// EF Core LINQ patterns demonstrated:
    /// - Conditional Sum using ternary operator
    /// - Date range filtering
    /// </summary>
    public async Task<(decimal TotalIncome, decimal TotalExpenses, decimal NetAmount)> GetIncomeExpenseSummaryAsync(
        DateTime startDate, DateTime endDate, int? userId = null)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.TransactionDate >= startDate.Date && t.TransactionDate <= endDate.Date);

        if (userId.HasValue)
        {
            query = query.Where(t => t.Account!.UserId == userId.Value);
        }

        // Calculate in a single query using conditional aggregation
        var summary = await query
            .GroupBy(t => 1) // Group all into one
            .Select(g => new
            {
                TotalIncome = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                TotalExpenses = g.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount)),
                NetAmount = g.Sum(t => t.Amount)
            })
            .FirstOrDefaultAsync();

        return (
            summary?.TotalIncome ?? 0,
            summary?.TotalExpenses ?? 0,
            summary?.NetAmount ?? 0
        );
    }

    /// <summary>
    /// Gets the top spending categories for a user.
    ///
    /// EF Core LINQ patterns demonstrated:
    /// - Take() for LIMIT
    /// - OrderByDescending for ranking
    /// </summary>
    public async Task<List<SpendingByCategory>> GetTopSpendingCategoriesAsync(int userId, int topCount = 5)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.Account!.UserId == userId && t.Amount < 0)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new SpendingByCategory
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryType = "Expense",
                TotalAmount = g.Sum(t => Math.Abs(t.Amount)),
                TransactionCount = g.Count()
            })
            .OrderByDescending(s => s.TotalAmount)
            .Take(topCount)
            .ToListAsync();
    }
}
