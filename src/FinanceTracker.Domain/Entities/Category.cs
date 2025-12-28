using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Represents a transaction category (e.g., Groceries, Salary, Entertainment).
/// </summary>
public class Category
{
    /// <summary>
    /// Gets or sets the unique identifier for the category.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is an income or expense category.
    /// </summary>
    public CategoryType Type { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier for UI display.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the color code for UI display (e.g., "#FF5733").
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the collection of transactions in this category.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    /// <summary>
    /// Gets or sets the collection of budgets for this category.
    /// </summary>
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
