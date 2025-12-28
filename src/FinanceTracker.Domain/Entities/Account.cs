using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Represents a financial account (checking, savings, credit card, etc.).
/// </summary>
public class Account
{
    /// <summary>
    /// Gets or sets the unique identifier for the account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who owns this account.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the account name (e.g., "Main Checking", "Emergency Fund").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of account.
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// Gets or sets the current balance of the account.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the currency code (e.g., "USD", "EUR").
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the date and time when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who owns this account.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the collection of transactions for this account.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
