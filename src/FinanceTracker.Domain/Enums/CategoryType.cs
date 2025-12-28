namespace FinanceTracker.Domain.Enums;

/// <summary>
/// Represents whether a category is for income or expenses.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// An expense category (money going out).
    /// </summary>
    Expense = 1,

    /// <summary>
    /// An income category (money coming in).
    /// </summary>
    Income = 2
}
