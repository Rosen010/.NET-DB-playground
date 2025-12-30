using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Dapper.Menu;

/// <summary>
/// Menu handler for Account operations.
/// </summary>
public class AccountMenu
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;

    public AccountMenu(IAccountRepository accountRepository, IUserRepository userRepository)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Account Management", new[]
            {
                "List all accounts",
                "List accounts by user",
                "View account by ID",
                "Create new account",
                "Update account",
                "Delete account",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1:
                    await ListAccountsAsync();
                    break;
                case 2:
                    await ListAccountsByUserAsync();
                    break;
                case 3:
                    await ViewAccountAsync();
                    break;
                case 4:
                    await CreateAccountAsync();
                    break;
                case 5:
                    await UpdateAccountAsync();
                    break;
                case 6:
                    await DeleteAccountAsync();
                    break;
                case 7:
                    return;
                default:
                    MenuHelper.ShowError("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private async Task ListAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("ID    | User ID | Name                 | Type        | Balance       | Currency");
        Console.WriteLine(new string('-', 85));

        foreach (var acc in accounts)
        {
            Console.WriteLine($"{acc.Id,-5} | {acc.UserId,-7} | {acc.Name,-20} | {acc.Type,-11} | {acc.Balance,13:N2} | {acc.Currency}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ListAccountsByUserAsync()
    {
        var userId = MenuHelper.PromptInt("Enter user ID");
        var accounts = await _accountRepository.GetByUserIdAsync(userId);

        Console.WriteLine();
        Console.WriteLine("ID    | Name                 | Type        | Balance       | Currency");
        Console.WriteLine(new string('-', 75));

        foreach (var acc in accounts)
        {
            Console.WriteLine($"{acc.Id,-5} | {acc.Name,-20} | {acc.Type,-11} | {acc.Balance,13:N2} | {acc.Currency}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ViewAccountAsync()
    {
        var id = MenuHelper.PromptInt("Enter account ID");
        var account = await _accountRepository.GetByIdAsync(id);

        if (account == null)
        {
            MenuHelper.ShowError($"Account with ID {id} not found.");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:         {account.Id}");
            Console.WriteLine($"User ID:    {account.UserId}");
            Console.WriteLine($"Name:       {account.Name}");
            Console.WriteLine($"Type:       {account.Type}");
            Console.WriteLine($"Balance:    {account.Balance:C}");
            Console.WriteLine($"Currency:   {account.Currency}");
            Console.WriteLine($"Created At: {account.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task CreateAccountAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Creating new account...");

        // Show available users
        var users = await _userRepository.GetAllAsync();
        Console.WriteLine("Available users:");
        foreach (var u in users)
        {
            Console.WriteLine($"  {u.Id}: {u.Name}");
        }

        var userId = MenuHelper.PromptInt("Enter user ID");
        var name = MenuHelper.PromptString("Enter account name");

        Console.WriteLine("Account type:");
        Console.WriteLine("  1. Checking");
        Console.WriteLine("  2. Savings");
        Console.WriteLine("  3. Credit Card");
        Console.WriteLine("  4. Cash");
        Console.WriteLine("  5. Investment");
        var typeChoice = MenuHelper.PromptInt("Enter choice (1-5)");
        var type = (AccountType)typeChoice;

        var balance = MenuHelper.PromptDecimal("Enter initial balance");
        var currency = MenuHelper.PromptString("Enter currency code (e.g., USD)", required: false);
        if (string.IsNullOrEmpty(currency)) currency = "USD";

        var account = new Account
        {
            UserId = userId,
            Name = name,
            Type = type,
            Balance = balance,
            Currency = currency
        };

        try
        {
            var id = await _accountRepository.CreateAsync(account);
            MenuHelper.ShowSuccess($"Account created with ID: {id}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to create account: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task UpdateAccountAsync()
    {
        var id = MenuHelper.PromptInt("Enter account ID to update");
        var account = await _accountRepository.GetByIdAsync(id);

        if (account == null)
        {
            MenuHelper.ShowError($"Account with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current name: {account.Name}");
        var newName = MenuHelper.PromptString("Enter new name (or press Enter to keep current)", required: false);
        if (!string.IsNullOrEmpty(newName)) account.Name = newName;

        Console.WriteLine($"Current balance: {account.Balance:C}");
        Console.Write("Enter new balance (or press Enter to keep current): ");
        var balanceInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(balanceInput) && decimal.TryParse(balanceInput, out var newBalance))
        {
            account.Balance = newBalance;
        }

        try
        {
            var success = await _accountRepository.UpdateAsync(account);
            if (success)
                MenuHelper.ShowSuccess("Account updated successfully.");
            else
                MenuHelper.ShowError("Failed to update account.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to update account: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task DeleteAccountAsync()
    {
        var id = MenuHelper.PromptInt("Enter account ID to delete");

        Console.Write("Are you sure? This will delete all related transactions (y/n): ");
        var confirm = Console.ReadLine()?.ToLower();

        if (confirm != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _accountRepository.DeleteAsync(id);
            if (success)
                MenuHelper.ShowSuccess("Account deleted successfully.");
            else
                MenuHelper.ShowError($"Account with ID {id} not found.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to delete account: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
