using FinanceTracker.EFCore.Services;

namespace FinanceTracker.EFCore.Menu;

public class ReportsMenu
{
    private readonly ReportsService _reportsService;
    private readonly UserService _userService;

    public ReportsMenu(ReportsService reportsService, UserService userService)
    {
        _reportsService = reportsService;
        _userService = userService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Reports & Analytics (EF Core LINQ)", new[]
            {
                "Monthly Spending by Category",
                "Account Balances Summary",
                "Budget Status (vs Actual Spending)",
                "Income vs Expenses Summary",
                "Top Spending Categories by User",
                "Back to main menu"
            });

            switch (choice)
            {
                case 1: await ShowMonthlySpendingAsync(); break;
                case 2: await ShowAccountBalancesAsync(); break;
                case 3: await ShowBudgetStatusAsync(); break;
                case 4: await ShowIncomeExpensesSummaryAsync(); break;
                case 5: await ShowTopSpendingCategoriesAsync(); break;
                case 6: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ShowMonthlySpendingAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Monthly Spending by Category Report");
        Console.WriteLine("EF Core: Uses LINQ GroupBy + Sum/Count aggregates");
        Console.WriteLine();

        var year = MenuHelper.PromptInt("Enter year (e.g., 2024)");
        var month = MenuHelper.PromptInt("Enter month (1-12)");

        var users = await _userService.GetAllAsync();
        Console.WriteLine("\nAvailable users (enter 0 for all):");
        foreach (var u in users)
            Console.WriteLine($"  {u.Id}: {u.Name}");

        var userIdInput = MenuHelper.PromptInt("Enter user ID (or 0 for all)");
        int? userId = userIdInput == 0 ? null : userIdInput;

        var results = await _reportsService.GetMonthlySpendingByCategoryAsync(year, month, userId);

        Console.WriteLine();
        Console.WriteLine($"=== Spending Report for {year}-{month:D2} ===");
        Console.WriteLine();
        Console.WriteLine("Category             | Type     | Total Amount  | # Transactions");
        Console.WriteLine(new string('-', 70));

        decimal totalSpending = 0;
        foreach (var item in results)
        {
            Console.WriteLine($"{item.CategoryName,-20} | {item.CategoryType,-8} | {item.TotalAmount,13:N2} | {item.TransactionCount,14}");
            if (item.CategoryType == "Expense")
                totalSpending += item.TotalAmount;
        }

        Console.WriteLine(new string('-', 70));
        Console.WriteLine($"{"TOTAL EXPENSES",-20} |          | {totalSpending,13:N2} |");

        MenuHelper.WaitForKey();
    }

    private async Task ShowAccountBalancesAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Account Balances Summary Report");
        Console.WriteLine("EF Core: Uses navigation properties + GroupBy");
        Console.WriteLine();

        var results = await _reportsService.GetAccountBalanceSummaryAsync();

        Console.WriteLine("User                 | Account Type | # Accounts | Total Balance");
        Console.WriteLine(new string('-', 70));

        string? currentUser = null;
        decimal userTotal = 0;

        foreach (var item in results)
        {
            if (currentUser != null && currentUser != item.UserName)
            {
                Console.WriteLine($"{"",-20} | {"SUBTOTAL",-12} |            | {userTotal,13:N2}");
                Console.WriteLine(new string('-', 70));
                userTotal = 0;
            }

            Console.WriteLine($"{item.UserName,-20} | {item.AccountType,-12} | {item.AccountCount,10} | {item.TotalBalance,13:N2}");
            userTotal += item.TotalBalance;
            currentUser = item.UserName;
        }

        if (currentUser != null)
            Console.WriteLine($"{"",-20} | {"SUBTOTAL",-12} |            | {userTotal,13:N2}");

        MenuHelper.WaitForKey();
    }

    private async Task ShowBudgetStatusAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Budget Status Report");
        Console.WriteLine("EF Core: Demonstrates multiple queries + in-memory join");
        Console.WriteLine();

        var year = MenuHelper.PromptInt("Enter year (e.g., 2024)");
        var month = MenuHelper.PromptInt("Enter month (1-12)");

        var results = await _reportsService.GetBudgetStatusAsync(year, month);

        Console.WriteLine();
        Console.WriteLine($"=== Budget Status for {year}-{month:D2} ===");
        Console.WriteLine();
        Console.WriteLine("User          | Category             | Budget     | Spent      | Remaining  | % Used | Status");
        Console.WriteLine(new string('-', 100));

        foreach (var item in results)
        {
            var status = item.IsOverBudget ? "OVER!" : (item.PercentUsed > 80 ? "Warning" : "OK");
            var statusColor = item.IsOverBudget ? ConsoleColor.Red : (item.PercentUsed > 80 ? ConsoleColor.Yellow : ConsoleColor.Green);

            Console.Write($"{item.UserName,-13} | {item.CategoryName,-20} | {item.BudgetAmount,10:N2} | {item.SpentAmount,10:N2} | {item.RemainingAmount,10:N2} | {item.PercentUsed,5:N1}% | ");

            Console.ForegroundColor = statusColor;
            Console.WriteLine(status);
            Console.ResetColor();
        }

        MenuHelper.WaitForKey();
    }

    private async Task ShowIncomeExpensesSummaryAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Income vs Expenses Summary");
        Console.WriteLine("EF Core: Uses conditional Where + Sum");
        Console.WriteLine();

        var startDate = MenuHelper.PromptDate("Enter start date");
        var endDate = MenuHelper.PromptDate("Enter end date");

        var users = await _userService.GetAllAsync();
        Console.WriteLine("\nAvailable users (enter 0 for all):");
        foreach (var u in users)
            Console.WriteLine($"  {u.Id}: {u.Name}");

        var userIdInput = MenuHelper.PromptInt("Enter user ID (or 0 for all)");
        int? userId = userIdInput == 0 ? null : userIdInput;

        var (totalIncome, totalExpenses, netAmount) = await _reportsService.GetIncomeExpenseSummaryAsync(startDate, endDate, userId);

        Console.WriteLine();
        Console.WriteLine($"=== Income vs Expenses: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} ===");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Total Income:   {totalIncome,15:N2}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Total Expenses: {totalExpenses,15:N2}");
        Console.ResetColor();

        Console.WriteLine($"  {"",17}---------------");

        Console.ForegroundColor = netAmount >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  Net Amount:     {netAmount,15:N2}");
        Console.ResetColor();

        if (netAmount >= 0 && totalIncome > 0)
            Console.WriteLine($"\n  You saved {(netAmount / totalIncome * 100):N1}% of your income!");
        else if (netAmount < 0)
            Console.WriteLine($"\n  Warning: You spent more than you earned.");

        MenuHelper.WaitForKey();
    }

    private async Task ShowTopSpendingCategoriesAsync()
    {
        Console.WriteLine();
        MenuHelper.ShowInfo("Top Spending Categories Report");
        Console.WriteLine("EF Core: Uses Take() for LIMIT");
        Console.WriteLine();

        var users = await _userService.GetAllAsync();
        Console.WriteLine("Available users:");
        foreach (var u in users)
            Console.WriteLine($"  {u.Id}: {u.Name}");

        var userId = MenuHelper.PromptInt("Enter user ID");

        Console.Write("How many top categories (default 5): ");
        var topCountInput = Console.ReadLine();
        var topCount = int.TryParse(topCountInput, out var tc) && tc > 0 ? tc : 5;

        var results = await _reportsService.GetTopSpendingCategoriesAsync(userId, topCount);

        Console.WriteLine();
        Console.WriteLine($"=== Top {topCount} Spending Categories ===");
        Console.WriteLine();
        Console.WriteLine("Rank | Category             | Total Spent   | # Transactions");
        Console.WriteLine(new string('-', 65));

        int rank = 1;
        foreach (var item in results)
        {
            Console.WriteLine($"{rank,4} | {item.CategoryName,-20} | {item.TotalAmount,13:N2} | {item.TransactionCount,14}");
            rank++;
        }

        MenuHelper.WaitForKey();
    }
}
