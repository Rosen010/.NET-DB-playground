namespace FinanceTracker.Domain.Entities;

/// <summary>
/// Represents a user of the finance tracking application.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of accounts belonging to this user.
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    /// <summary>
    /// Gets or sets the collection of budgets belonging to this user.
    /// </summary>
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
