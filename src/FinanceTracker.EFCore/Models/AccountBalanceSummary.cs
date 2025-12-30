namespace FinanceTracker.EFCore.Models;

/// <summary>
/// Represents a summary of account balances for a user.
/// </summary>
public class AccountBalanceSummary
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public int AccountCount { get; set; }
    public decimal TotalBalance { get; set; }
}
