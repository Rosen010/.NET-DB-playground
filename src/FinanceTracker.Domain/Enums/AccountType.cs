namespace FinanceTracker.Domain.Enums;

/// <summary>
/// Represents the type of financial account.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A standard checking account for daily transactions.
    /// </summary>
    Checking = 1,

    /// <summary>
    /// A savings account for storing money with potential interest.
    /// </summary>
    Savings = 2,

    /// <summary>
    /// A credit card account representing borrowed funds.
    /// </summary>
    CreditCard = 3,

    /// <summary>
    /// Cash on hand, not held in a bank.
    /// </summary>
    Cash = 4,

    /// <summary>
    /// An investment account for stocks, bonds, etc.
    /// </summary>
    Investment = 5
}
