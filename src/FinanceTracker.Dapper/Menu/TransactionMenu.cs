using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Menu;

/// <summary>
/// Menu handler for Transaction operations.
/// </summary>
public class TransactionMenu
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionMenu(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
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
                case 1:
                    await ListRecentTransactionsAsync();
                    break;
                case 2:
                    await ListTransactionsByAccountAsync();
                    break;
                case 3:
                    await ListTransactionsByDateRangeAsync();
                    break;
                case 4:
                    await ViewTransactionAsync();
                    break;
                case 5:
                    await CreateTransactionAsync();
                    break;
                case 6:
                    await UpdateTransactionAsync();
                    break;
                case 7:
                    await DeleteTransactionAsync();
                    break;
                case 8:
                    return;
                default:
                    MenuHelper.ShowError("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private async Task ListRecentTransactionsAsync()
    {
        var transactions = (await _transactionRepository.GetAllAsync()).Take(20);

        await DisplayTransactionsAsync(transactions);
        MenuHelper.WaitForKey();
    }

    private async Task ListTransactionsByAccountAsync()
    {
        var accountId = MenuHelper.PromptInt("Enter account ID");
        var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);

        await DisplayTransactionsAsync(transactions);
        MenuHelper.WaitForKey();
    }

    private async Task ListTransactionsByDateRangeAsync()
    {
        var startDate = MenuHelper.PromptDate("Enter start date");
        var endDate = MenuHelper.PromptDate("Enter end date");

        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

        await DisplayTransactionsAsync(transactions);
        MenuHelper.WaitForKey();
    }

    private async Task DisplayTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        // Load categories for display
        var categories = (await _categoryRepository.GetAllAsync()).ToDictionary(c => c.Id);

        Console.WriteLine();
        Console.WriteLine("ID    | Date       | Category             | Amount       | Description");
        Console.WriteLine(new string('-', 85));

        foreach (var txn in transactions)
        {
            var categoryName = categories.TryGetValue(txn.CategoryId, out var cat) ? cat.Name : "Unknown";
            var amountStr = txn.Amount >= 0 ? $"+{txn.Amount:N2}" : $"{txn.Amount:N2}";
            Console.WriteLine($"{txn.Id,-5} | {txn.TransactionDate:yyyy-MM-dd} | {categoryName,-20} | {amountStr,12} | {txn.Description ?? "-"}");
        }
    }

    private async Task ViewTransactionAsync()
    {
        var id = MenuHelper.PromptInt("Enter transaction ID");
        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction == null)
        {
            MenuHelper.ShowError($"Transaction with ID {id} not found.");
        }
        else
        {
            var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
            var account = await _accountRepository.GetByIdAsync(transaction.AccountId);

            Console.WriteLine();
            Console.WriteLine($"ID:               {transaction.Id}");
            Console.WriteLine($"Account:          {account?.Name ?? "Unknown"} (ID: {transaction.AccountId})");
            Console.WriteLine($"Category:         {category?.Name ?? "Unknown"} (ID: {transaction.CategoryId})");
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

        // Show available accounts
        var accounts = await _accountRepository.GetAllAsync();
        Console.WriteLine("Available accounts:");
        foreach (var a in accounts)
        {
            Console.WriteLine($"  {a.Id}: {a.Name} (Balance: {a.Balance:C})");
        }

        var accountId = MenuHelper.PromptInt("Enter account ID");

        // Show available categories
        var categories = await _categoryRepository.GetAllAsync();
        Console.WriteLine("Available categories:");
        foreach (var c in categories)
        {
            Console.WriteLine($"  {c.Id}: {c.Name} ({c.Type})");
        }

        var categoryId = MenuHelper.PromptInt("Enter category ID");

        Console.WriteLine("Enter amount (positive for income, negative for expense):");
        var amount = MenuHelper.PromptDecimal("Amount");

        var description = MenuHelper.PromptString("Enter description (optional)", required: false);
        var transactionDate = MenuHelper.PromptDate("Enter transaction date");

        var transaction = new Transaction
        {
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = amount,
            Description = string.IsNullOrEmpty(description) ? null : description,
            TransactionDate = transactionDate
        };

        try
        {
            var id = await _transactionRepository.CreateAsync(transaction);
            MenuHelper.ShowSuccess($"Transaction created with ID: {id}");
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
        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction == null)
        {
            MenuHelper.ShowError($"Transaction with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current amount: {transaction.Amount:C}");
        Console.Write("Enter new amount (or press Enter to keep current): ");
        var amountInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(amountInput) && decimal.TryParse(amountInput, out var newAmount))
        {
            transaction.Amount = newAmount;
        }

        Console.WriteLine($"Current description: {transaction.Description ?? "(none)"}");
        var newDesc = MenuHelper.PromptString("Enter new description (or press Enter to keep current)", required: false);
        if (!string.IsNullOrEmpty(newDesc)) transaction.Description = newDesc;

        Console.WriteLine($"Current date: {transaction.TransactionDate:yyyy-MM-dd}");
        Console.Write("Enter new date yyyy-MM-dd (or press Enter to keep current): ");
        var dateInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(dateInput) && DateTime.TryParse(dateInput, out var newDate))
        {
            transaction.TransactionDate = newDate;
        }

        try
        {
            var success = await _transactionRepository.UpdateAsync(transaction);
            if (success)
                MenuHelper.ShowSuccess("Transaction updated successfully.");
            else
                MenuHelper.ShowError("Failed to update transaction.");
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
        var confirm = Console.ReadLine()?.ToLower();

        if (confirm != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _transactionRepository.DeleteAsync(id);
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
