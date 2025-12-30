using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class TransactionMenu
{
    private readonly TransactionService _transactionService;
    private readonly AccountService _accountService;
    private readonly CategoryService _categoryService;

    public TransactionMenu(
        TransactionService transactionService,
        AccountService accountService,
        CategoryService categoryService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        _categoryService = categoryService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Transaction Management", new[]
            {
                "List recent transactions",
                "List transactions by account",
                "List transactions by date range",
                "View transaction by ID",
                "Create new transaction",
                "Update transaction",
                "Delete transaction",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1: await ListRecentTransactionsAsync(); break;
                case 2: await ListTransactionsByAccountAsync(); break;
                case 3: await ListTransactionsByDateRangeAsync(); break;
                case 4: await ViewTransactionAsync(); break;
                case 5: await CreateTransactionAsync(); break;
                case 6: await UpdateTransactionAsync(); break;
                case 7: await DeleteTransactionAsync(); break;
                case 8: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListRecentTransactionsAsync()
    {
        var transactions = await _transactionService.GetRecentAsync(20);
        DisplayTransactions(transactions);
        MenuHelper.WaitForKey();
    }

    private async Task ListTransactionsByAccountAsync()
    {
        var accountId = MenuHelper.PromptInt("Enter account ID");
        var transactions = await _transactionService.GetByAccountIdAsync(accountId);
        DisplayTransactions(transactions);
        MenuHelper.WaitForKey();
    }

    private async Task ListTransactionsByDateRangeAsync()
    {
        var startDate = MenuHelper.PromptDate("Enter start date");
        var endDate = MenuHelper.PromptDate("Enter end date");
        var transactions = await _transactionService.GetByDateRangeAsync(startDate, endDate);
        DisplayTransactions(transactions);
        MenuHelper.WaitForKey();
    }

    private void DisplayTransactions(List<Transaction> transactions)
    {
        Console.WriteLine();
        Console.WriteLine("ID    | Date       | Category             | Amount       | Description");
        Console.WriteLine(new string('-', 85));

        foreach (var txn in transactions)
        {
            var categoryName = txn.Category?.Name ?? "Unknown";
            var amountStr = txn.Amount >= 0 ? $"+{txn.Amount:N2}" : $"{txn.Amount:N2}";
            Console.WriteLine($"{txn.Id,-5} | {txn.TransactionDate:yyyy-MM-dd} | {categoryName,-20} | {amountStr,12} | {txn.Description ?? "-"}");
        }
    }

    private async Task ViewTransactionAsync()
    {
        var id = MenuHelper.PromptInt("Enter transaction ID");
        var transaction = await _transactionService.GetByIdAsync(id);

        if (transaction == null)
            MenuHelper.ShowError($"Transaction with ID {id} not found.");
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:               {transaction.Id}");
            Console.WriteLine($"Account:          {transaction.Account?.Name ?? "Unknown"} (ID: {transaction.AccountId})");
            Console.WriteLine($"Category:         {transaction.Category?.Name ?? "Unknown"} (ID: {transaction.CategoryId})");
            Console.WriteLine($"Amount:           {transaction.Amount:C}");
            Console.WriteLine($"Description:      {transaction.Description ?? "(none)"}");
            Console.WriteLine($"Transaction Date: {transaction.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine($"Created At:       {transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task CreateTransactionAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Creating new transaction...");

        var accounts = await _accountService.GetAllAsync();
        Console.WriteLine("Available accounts:");
        foreach (var a in accounts)
            Console.WriteLine($"  {a.Id}: {a.Name} ({a.Balance:C})");

        var transaction = new Transaction
        {
            AccountId = MenuHelper.PromptInt("Enter account ID")
        };

        var categories = await _categoryService.GetAllAsync();
        Console.WriteLine("Available categories:");
        foreach (var c in categories)
            Console.WriteLine($"  {c.Id}: {c.Name} ({c.Type})");

        transaction.CategoryId = MenuHelper.PromptInt("Enter category ID");
        Console.WriteLine("Enter amount (positive=income, negative=expense):");
        transaction.Amount = MenuHelper.PromptDecimal("Amount");

        var desc = MenuHelper.PromptString("Enter description (optional)", required: false);
        transaction.Description = string.IsNullOrEmpty(desc) ? null : desc;
        transaction.TransactionDate = MenuHelper.PromptDate("Enter transaction date");

        try
        {
            var created = await _transactionService.CreateAsync(transaction);
            MenuHelper.ShowSuccess($"Transaction created with ID: {created.Id}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to create transaction: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task UpdateTransactionAsync()
    {
        var id = MenuHelper.PromptInt("Enter transaction ID to update");
        var transaction = await _transactionService.GetByIdAsync(id);

        if (transaction == null)
        {
            MenuHelper.ShowError($"Transaction with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current amount: {transaction.Amount:C}");
        Console.Write("Enter new amount (or Enter to keep): ");
        var amountInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(amountInput) && decimal.TryParse(amountInput, out var newAmount))
            transaction.Amount = newAmount;

        Console.WriteLine($"Current description: {transaction.Description ?? "(none)"}");
        var newDesc = MenuHelper.PromptString("Enter new description (or Enter to keep)", required: false);
        if (!string.IsNullOrEmpty(newDesc)) transaction.Description = newDesc;

        Console.WriteLine($"Current date: {transaction.TransactionDate:yyyy-MM-dd}");
        Console.Write("Enter new date (or Enter to keep): ");
        var dateInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(dateInput) && DateTime.TryParse(dateInput, out var newDate))
            transaction.TransactionDate = newDate;

        try
        {
            await _transactionService.UpdateAsync(transaction);
            MenuHelper.ShowSuccess("Transaction updated successfully.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to update transaction: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task DeleteTransactionAsync()
    {
        var id = MenuHelper.PromptInt("Enter transaction ID to delete");

        Console.Write("Are you sure? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _transactionService.DeleteAsync(id);
            if (success)
                MenuHelper.ShowSuccess("Transaction deleted successfully.");
            else
                MenuHelper.ShowError($"Transaction with ID {id} not found.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to delete transaction: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
