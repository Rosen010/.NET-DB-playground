using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class AccountMenu
{
    private readonly AccountService _accountService;
    private readonly UserService _userService;

    public AccountMenu(AccountService accountService, UserService userService)
    {
        _accountService = accountService;
        _userService = userService;
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
                case 1: await ListAccountsAsync(); break;
                case 2: await ListAccountsByUserAsync(); break;
                case 3: await ViewAccountAsync(); break;
                case 4: await CreateAccountAsync(); break;
                case 5: await UpdateAccountAsync(); break;
                case 6: await DeleteAccountAsync(); break;
                case 7: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListAccountsAsync()
    {
        var accounts = await _accountService.GetAllAsync();

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
        var accounts = await _accountService.GetByUserIdAsync(userId);

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
        var account = await _accountService.GetByIdWithUserAsync(id);

        if (account == null)
            MenuHelper.ShowError($"Account with ID {id} not found.");
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:         {account.Id}");
            Console.WriteLine($"User:       {account.User?.Name ?? "Unknown"} (ID: {account.UserId})");
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

        var users = await _userService.GetAllAsync();
        Console.WriteLine("Available users:");
        foreach (var u in users)
            Console.WriteLine($"  {u.Id}: {u.Name}");

        var account = new Account
        {
            UserId = MenuHelper.PromptInt("Enter user ID"),
            Name = MenuHelper.PromptString("Enter account name")
        };

        Console.WriteLine("Account type: 1.Checking 2.Savings 3.CreditCard 4.Cash 5.Investment");
        account.Type = (AccountType)MenuHelper.PromptInt("Enter choice (1-5)");
        account.Balance = MenuHelper.PromptDecimal("Enter initial balance");

        var currency = MenuHelper.PromptString("Enter currency (default USD)", required: false);
        account.Currency = string.IsNullOrEmpty(currency) ? "USD" : currency;

        try
        {
            var created = await _accountService.CreateAsync(account);
            MenuHelper.ShowSuccess($"Account created with ID: {created.Id}");
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
        var account = await _accountService.GetByIdAsync(id);

        if (account == null)
        {
            MenuHelper.ShowError($"Account with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current name: {account.Name}");
        var newName = MenuHelper.PromptString("Enter new name (or Enter to keep)", required: false);
        if (!string.IsNullOrEmpty(newName)) account.Name = newName;

        Console.WriteLine($"Current balance: {account.Balance:C}");
        Console.Write("Enter new balance (or Enter to keep): ");
        var balanceInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(balanceInput) && decimal.TryParse(balanceInput, out var newBalance))
            account.Balance = newBalance;

        try
        {
            await _accountService.UpdateAsync(account);
            MenuHelper.ShowSuccess("Account updated successfully.");
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

        Console.Write("Are you sure? This will delete all transactions (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _accountService.DeleteAsync(id);
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
