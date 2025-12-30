namespace FinanceTracker.Dapper.Models;

/// <summary>
/// Represents spending data grouped by category for a specific period.
/// </summary>
public class SpendingByCategory
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
}
