using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class BudgetMenu
{
    private readonly BudgetService _budgetService;
    private readonly UserService _userService;
    private readonly CategoryService _categoryService;

    public BudgetMenu(
        BudgetService budgetService,
        UserService userService,
        CategoryService categoryService)
    {
        _budgetService = budgetService;
        _userService = userService;
        _categoryService = categoryService;
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
                case 1: await ListBudgetsAsync(); break;
                case 2: await ListBudgetsByUserAsync(); break;
                case 3: await ViewBudgetAsync(); break;
                case 4: await CreateBudgetAsync(); break;
                case 5: await UpdateBudgetAsync(); break;
                case 6: await DeleteBudgetAsync(); break;
                case 7: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ListBudgetsAsync()
    {
        var budgets = await _budgetService.GetAllAsync();

        Console.WriteLine();
        Console.WriteLine("ID    | User                 | Category             | Amount     | Period  | Start Date");
        Console.WriteLine(new string('-', 95));

        foreach (var b in budgets)
        {
            Console.WriteLine($"{b.Id,-5} | {b.User?.Name ?? "Unknown",-20} | {b.Category?.Name ?? "Unknown",-20} | {b.Amount,10:N2} | {b.Period,-7} | {b.StartDate:yyyy-MM-dd}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ListBudgetsByUserAsync()
    {
        var userId = MenuHelper.PromptInt("Enter user ID");
        var budgets = await _budgetService.GetByUserIdAsync(userId);

        Console.WriteLine();
        Console.WriteLine("ID    | Category             | Amount     | Period  | Start Date");
        Console.WriteLine(new string('-', 70));

        foreach (var b in budgets)
        {
            Console.WriteLine($"{b.Id,-5} | {b.Category?.Name ?? "Unknown",-20} | {b.Amount,10:N2} | {b.Period,-7} | {b.StartDate:yyyy-MM-dd}");
        }

        MenuHelper.WaitForKey();
    }

    private async Task ViewBudgetAsync()
    {
        var id = MenuHelper.PromptInt("Enter budget ID");
        var budget = await _budgetService.GetByIdAsync(id);

        if (budget == null)
            MenuHelper.ShowError($"Budget with ID {id} not found.");
        else
        {
            Console.WriteLine();
            Console.WriteLine($"ID:         {budget.Id}");
            Console.WriteLine($"User:       {budget.User?.Name ?? "Unknown"} (ID: {budget.UserId})");
            Console.WriteLine($"Category:   {budget.Category?.Name ?? "Unknown"} (ID: {budget.CategoryId})");
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

        var users = await _userService.GetAllAsync();
        Console.WriteLine("Available users:");
        foreach (var u in users)
            Console.WriteLine($"  {u.Id}: {u.Name}");

        var budget = new Budget
        {
            UserId = MenuHelper.PromptInt("Enter user ID")
        };

        var categories = await _categoryService.GetByTypeAsync(CategoryType.Expense);
        Console.WriteLine("Available expense categories:");
        foreach (var c in categories)
            Console.WriteLine($"  {c.Id}: {c.Name}");

        budget.CategoryId = MenuHelper.PromptInt("Enter category ID");
        budget.Amount = MenuHelper.PromptDecimal("Enter budget amount");

        Console.WriteLine("Budget period: 1. Monthly  2. Yearly");
        budget.Period = MenuHelper.PromptInt("Enter choice") == 2 ? BudgetPeriod.Yearly : BudgetPeriod.Monthly;
        budget.StartDate = MenuHelper.PromptDate("Enter start date");

        try
        {
            var created = await _budgetService.CreateAsync(budget);
            MenuHelper.ShowSuccess($"Budget created with ID: {created.Id}");
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
        var budget = await _budgetService.GetByIdAsync(id);

        if (budget == null)
        {
            MenuHelper.ShowError($"Budget with ID {id} not found.");
            MenuHelper.WaitForKey();
            return;
        }

        Console.WriteLine($"Current amount: {budget.Amount:C}");
        Console.Write("Enter new amount (or Enter to keep): ");
        var amountInput = Console.ReadLine();
        if (!string.IsNullOrEmpty(amountInput) && decimal.TryParse(amountInput, out var newAmount))
            budget.Amount = newAmount;

        try
        {
            await _budgetService.UpdateAsync(budget);
            MenuHelper.ShowSuccess("Budget updated successfully.");
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
        if (Console.ReadLine()?.ToLower() != "y")
        {
            MenuHelper.ShowInfo("Deletion cancelled.");
            MenuHelper.WaitForKey();
            return;
        }

        try
        {
            var success = await _budgetService.DeleteAsync(id);
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
