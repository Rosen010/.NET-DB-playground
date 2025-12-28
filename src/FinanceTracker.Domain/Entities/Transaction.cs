namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Represents a financial transaction (income or expense).
/// </summary>
public class Transaction
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the account this transaction belongs to.
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the category for this transaction.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// Positive values represent income, negative values represent expenses.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the transaction description or memo.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date when the transaction occurred.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the transaction record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the account this transaction belongs to.
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Gets or sets the category of this transaction.
    /// </summary>
    public Category? Category { get; set; }
}
