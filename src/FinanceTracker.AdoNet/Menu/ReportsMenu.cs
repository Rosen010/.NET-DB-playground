using FinanceTracker.AdoNet.Repositories;

namespace FinanceTracker.AdoNet.Menu;

public class ReportsMenu
{
    private readonly ReportsRepository _repo;
    private readonly UserRepository _userRepo;

    public ReportsMenu(ReportsRepository repo, UserRepository userRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var choice = MenuHelper.ShowMenu("Reports & Analytics (Raw ADO.NET)", new[]
            {
                "Monthly Spending by Category",
                "Account Balances Summary",
                "Budget Status",
                "Income vs Expenses",
                "Top Spending Categories",
                "Back"
            });

            switch (choice)
            {
                case 1: await ShowMonthlySpendingAsync(); break;
                case 2: await ShowAccountBalancesAsync(); break;
                case 3: await ShowBudgetStatusAsync(); break;
                case 4: await ShowIncomeExpensesAsync(); break;
                case 5: await ShowTopCategoriesAsync(); break;
                case 6: return;
                default: MenuHelper.ShowError("Invalid choice."); break;
            }
        }
    }

    private async Task ShowMonthlySpendingAsync()
    {
        Console.WriteLine("\n--- Monthly Spending (ADO.NET: Raw SQL with JOINs) ---\n");

        var year = MenuHelper.PromptInt("Year");
        var month = MenuHelper.PromptInt("Month (1-12)");

        var users = await _userRepo.GetAllAsync();
        Console.WriteLine("Users: " + string.Join(", ", users.Select(u => $"{u.Id}:{u.Name}")) + " (0=all)");
        var userIdInput = MenuHelper.PromptInt("User ID (0 for all)");
        int? userId = userIdInput == 0 ? null : userIdInput;

        var results = await _repo.GetMonthlySpendingByCategoryAsync(year, month, userId);

        Console.WriteLine($"\n=== Spending for {year}-{month:D2} ===\n");
        Console.WriteLine("Category             | Type     | Total         | Count");
        Console.WriteLine(new string('-', 65));

        decimal total = 0;
        foreach (var r in results)
        {
            Console.WriteLine($"{r.CategoryName,-20} | {r.CategoryType,-8} | {r.TotalAmount,13:N2} | {r.TransactionCount,5}");
            if (r.CategoryType == "Expense") total += r.TotalAmount;
        }
        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"TOTAL EXPENSES",-20} |          | {total,13:N2} |");

        MenuHelper.WaitForKey();
    }

    private async Task ShowAccountBalancesAsync()
    {
        Console.WriteLine("\n--- Account Balances (ADO.NET: GROUP BY query) ---\n");

        var results = await _repo.GetAccountBalanceSummaryAsync();

        Console.WriteLine("User                 | Type         | Count | Total Balance");
        Console.WriteLine(new string('-', 65));

        string? currentUser = null;
        decimal userTotal = 0;

        foreach (var r in results)
        {
            if (currentUser != null && currentUser != r.UserName)
            {
                Console.WriteLine($"{"",-20} | {"SUBTOTAL",-12} |       | {userTotal,13:N2}");
                Console.WriteLine(new string('-', 65));
                userTotal = 0;
            }
            Console.WriteLine($"{r.UserName,-20} | {r.AccountType,-12} | {r.AccountCount,5} | {r.TotalBalance,13:N2}");
            userTotal += r.TotalBalance;
            currentUser = r.UserName;
        }
        if (currentUser != null)
            Console.WriteLine($"{"",-20} | {"SUBTOTAL",-12} |       | {userTotal,13:N2}");

        MenuHelper.WaitForKey();
    }

    private async Task ShowBudgetStatusAsync()
    {
        Console.WriteLine("\n--- Budget Status (ADO.NET: Correlated subquery) ---\n");

        var year = MenuHelper.PromptInt("Year");
        var month = MenuHelper.PromptInt("Month (1-12)");

        var results = await _repo.GetBudgetStatusAsync(year, month);

        Console.WriteLine($"\n=== Budget Status for {year}-{month:D2} ===\n");
        Console.WriteLine("User          | Category             | Budget     | Spent      | % Used | Status");
        Console.WriteLine(new string('-', 90));

        foreach (var r in results)
        {
            var status = r.IsOverBudget ? "OVER!" : (r.PercentUsed > 80 ? "Warning" : "OK");
            var color = r.IsOverBudget ? ConsoleColor.Red : (r.PercentUsed > 80 ? ConsoleColor.Yellow : ConsoleColor.Green);

            Console.Write($"{r.UserName,-13} | {r.CategoryName,-20} | {r.BudgetAmount,10:N2} | {r.SpentAmount,10:N2} | {r.PercentUsed,5:N1}% | ");
            Console.ForegroundColor = color;
            Console.WriteLine(status);
            Console.ResetColor();
        }

        MenuHelper.WaitForKey();
    }

    private async Task ShowIncomeExpensesAsync()
    {
        Console.WriteLine("\n--- Income vs Expenses (ADO.NET: Conditional aggregation) ---\n");

        var start = MenuHelper.PromptDate("Start date");
        var end = MenuHelper.PromptDate("End date");

        var users = await _userRepo.GetAllAsync();
        Console.WriteLine("Users: " + string.Join(", ", users.Select(u => $"{u.Id}:{u.Name}")) + " (0=all)");
        var userIdInput = MenuHelper.PromptInt("User ID (0 for all)");
        int? userId = userIdInput == 0 ? null : userIdInput;

        var (income, expenses, net) = await _repo.GetIncomeExpenseSummaryAsync(start, end, userId);

        Console.WriteLine($"\n=== {start:yyyy-MM-dd} to {end:yyyy-MM-dd} ===\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Income:   {income,15:N2}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Expenses: {expenses,15:N2}");
        Console.ResetColor();

        Console.WriteLine($"            ---------------");

        Console.ForegroundColor = net >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  Net:      {net,15:N2}");
        Console.ResetColor();

        MenuHelper.WaitForKey();
    }

    private async Task ShowTopCategoriesAsync()
    {
        Console.WriteLine("\n--- Top Spending (ADO.NET: LIMIT query) ---\n");

        var users = await _userRepo.GetAllAsync();
        Console.WriteLine("Users: " + string.Join(", ", users.Select(u => $"{u.Id}:{u.Name}")));
        var userId = MenuHelper.PromptInt("User ID");

        Console.Write("Top N (default 5): ");
        var topInput = Console.ReadLine();
        var topCount = int.TryParse(topInput, out var tc) && tc > 0 ? tc : 5;

        var results = await _repo.GetTopSpendingCategoriesAsync(userId, topCount);

        Console.WriteLine($"\n=== Top {topCount} Spending Categories ===\n");
        Console.WriteLine("Rank | Category             | Total         | Count");
        Console.WriteLine(new string('-', 55));

        int rank = 1;
        foreach (var r in results)
        {
            Console.WriteLine($"{rank,4} | {r.CategoryName,-20} | {r.TotalAmount,13:N2} | {r.TransactionCount,5}");
            rank++;
        }

        MenuHelper.WaitForKey();
    }
}
