using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Represents a budget for a specific category.
/// </summary>
public class Budget
{
    /// <summary>
    /// Gets or sets the unique identifier for the budget.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who owns this budget.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the category this budget applies to.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the budget amount limit.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the budget period (monthly or yearly).
    /// </summary>
    public BudgetPeriod Period { get; set; }

    /// <summary>
    /// Gets or sets the start date of the budget.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the user who owns this budget.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the category this budget applies to.
    /// </summary>
    public Category? Category { get; set; }
}
