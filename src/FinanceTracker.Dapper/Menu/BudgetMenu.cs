using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Dapper.Menu;

/// <summary>
/// Menu handler for Budget operations.
/// </summary>
public class BudgetMenu
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BudgetMenu(
        IBudgetRepository budgetRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _budgetRepository = budgetRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Budget Management", new[]
            {
                "List all budgets",
                "List budgets by user",
                "View budget by ID",
                "Create new budget",
                "Update budget",
                "Delete budget",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1:
                    await ListBudgetsAsync();
                    break;
                case 2:
                    await ListBudgetsByUserAsync();
                    break;
                case 3:
                    await ViewBudgetAsync();
                    break;
                case 4:
                    await CreateBudgetAsync();
                    break;
                case 5:
                    await UpdateBudgetAsync();
                    break;
                case 6:
                    await DeleteBudgetAsync();
                    break;
                case 7:
                    return;
                default:
                    MenuHelper.ShowError("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private async Task ListBudgetsAsync()
    {
        var budgets = await _budgetRepository.GetAllAsync();
        await DisplayBudgetsAsync(budgets);
        MenuHelper.WaitForKey();
    }

    private async Task ListBudgetsByUserAsync()
    {
        var userId = MenuHelper.PromptInt("Enter user ID");
        var budgets = await _budgetRepository.GetByUserIdAsync(userId);
        await DisplayBudgetsAsync(budgets);
        MenuHelper.WaitForKey();
    }

    private async Task DisplayBudgetsAsync(IEnumerable<Budget> budgets)
    {
        var categories = (await _categoryRepository.GetAllAsync()).ToDictionary(c => c.Id);
        var users = (await _userRepository.GetAllAsync()).ToDictionary(u => u.Id);

        Console.WriteLine();
        Console.WriteLine("ID    | User                 | Category             | Amount     | Period  | Start Date");
        Console.WriteLine(new string('-', 95));

        foreach (var budget in budgets)
        {
            var userName = users.TryGetValue(budget.UserId, out var user) ? user.Name : "Unknown";
            var categoryName = categories.TryGetValue(budget.CategoryId, out var cat) ? cat.Name : "Unknown";
            Console.WriteLine($"{budget.Id,-5} | {userName,-20} | {categoryName,-20} | {budget.Amount,10:N2} | {budget.Period,-7} | {budget.StartDate:yyyy-MM-dd}");
        }
    }

    private async Task ViewBudgetAsync()
    {
        var id = MenuHelper.PromptInt("Enter budget ID");
        var budget = await _budgetRepository.GetByIdAsync(id);

        if (budget == null)
        {
            MenuHelper.ShowError($"Budget with ID {id} not found.");
        }
        else
        {
            var user = await _userRepository.GetByIdAsync(budget.UserId);
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId);

            Console.WriteLine();
            Console.WriteLine($"ID:         {budget.Id}");
            Console.WriteLine($"User:       {user?.Name ?? "Unknown"} (ID: {budget.UserId})");
            Console.WriteLine($"Category:   {category?.Name ?? "Unknown"} (ID: {budget.CategoryId})");
            Console.WriteLine($"Amount:     {budget.Amount:C}");
            Console.WriteLine($"Period:     {budget.Period}");
            Console.WriteLine($"Start Date: {budget.StartDate:yyyy-MM-dd}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task CreateBudgetAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Creating new budget...");

        // Show available users
        var users = await _userRepository.GetAllAsync();
        Console.WriteLine("Available users:");
        foreach (var u in users)
        {
            Console.WriteLine($"  {u.Id}: {u.Name}");
        }

        var userId = MenuHelper.PromptInt("Enter user ID");

        // Show expense categories (budgets are typically for expenses)
        var categories = await _categoryRepository.GetByTypeAsync(CategoryType.Expense);
        Console.WriteLine("Available expense categories:");
        foreach (var c in categories)
        {
            Console.WriteLine($"  {c.Id}: {c.Name}");
        }

        var categoryId = MenuHelper.PromptInt("Enter category ID");
        var amount = MenuHelper.PromptDecimal("Enter budget amount");

        Console.WriteLine("Budget period:");
        Console.WriteLine("  1. Monthly");
        Console.WriteLine("  2. Yearly");
        var periodChoice = MenuHelper.PromptInt("Enter choice (1 or 2)");
        var period = periodChoice == 2 ? BudgetPeriod.Yearly : BudgetPeriod.Monthly;

        var startDate = MenuHelper.PromptDate("Enter start date");

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = categoryId,
            Amount = amount,
            Period = period,
            StartDate = startDate
        };

        try
        {
            var id = await _budgetRepository.CreateAsync(budget);
            MenuHelper.ShowSuccess($"Budget created with ID: {id}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to create budget: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task UpdateBudgetAsync()
    {
        var id = MenuHelper.PromptInt("Enter budget ID to update");
        var budget = await _budgetRepository.GetByIdAsync(id);

        if (budget == null)
        {
            MenuHelper.ShowError($"Budget with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current amount: {budget.Amount:C}");
        Console.Write("Enter new amount (or press Enter to keep current): ");
        var amountInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(amountInput) && decimal.TryParse(amountInput, out var newAmount))
        {
            budget.Amount = newAmount;
        }

        try
        {
            var success = await _budgetRepository.UpdateAsync(budget);
            if (success)
                MenuHelper.ShowSuccess("Budget updated successfully.");
            else
                MenuHelper.ShowError("Failed to update budget.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to update budget: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task DeleteBudgetAsync()
    {
        var id = MenuHelper.PromptInt("Enter budget ID to delete");

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
            var success = await _budgetRepository.DeleteAsync(id);
            if (success)
                MenuHelper.ShowSuccess("Budget deleted successfully.");
            else
                MenuHelper.ShowError($"Budget with ID {id} not found.");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Failed to delete budget: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
