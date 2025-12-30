namespace FinanceTracker.EFCore.Models;

/// <summary>
/// Represents the status of a budget compared to actual spending.
/// </summary>
public class BudgetStatus
{
    public int BudgetId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => BudgetAmount - SpentAmount;
    public decimal PercentUsed => BudgetAmount > 0 ? (SpentAmount / BudgetAmount) * 100 : 0;
    public string Period { get; set; } = string.Empty;
    public bool IsOverBudget => SpentAmount > BudgetAmount;
}
