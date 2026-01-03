using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FinanceTracker.EFCore.Data;
using FinanceTracker.EFCore.Menu;
using FinanceTracker.EFCore.Services;
using FinanceTracker.Database;

namespace FinanceTracker.EFCore;

/// <summary>
/// Finance Tracker Console Application using Entity Framework Core.
///
/// This application demonstrates:
/// - EF Core DbContext with Fluent API configuration
/// - LINQ queries that translate to SQL
/// - Navigation properties and eager loading (Include)
/// - Change tracking and automatic SaveChanges
/// - PostgreSQL with snake_case naming convention
///
/// EF Core Benefits:
/// - Type-safe, compile-time checked queries
/// - Automatic change tracking
/// - Navigation properties for relationships
/// - Rich LINQ support
/// - Built-in migration support
///
/// Trade-offs vs Dapper:
/// - Higher memory usage due to change tracking
/// - Less control over generated SQL
/// - More complex configuration
/// - Better for complex domain models
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Connection string 'DefaultConnection' not found in appsettings.json");
            Console.ResetColor();
            return;
        }

        MigrationRunner.MigrateUp(connectionString);

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
        optionsBuilder.UseNpgsql(connectionString);    

        // Create DbContext and services
        await using var context = new FinanceDbContext(optionsBuilder.Options);

        var userService = new UserService(context);
        var categoryService = new CategoryService(context);
        var accountService = new AccountService(context);
        var transactionService = new TransactionService(context);
        var budgetService = new BudgetService(context);
        var reportsService = new ReportsService(context);

        // Create menu handlers
        var userMenu = new UserMenu(userService);
        var categoryMenu = new CategoryMenu(categoryService);
        var accountMenu = new AccountMenu(accountService, userService);
        var transactionMenu = new TransactionMenu(transactionService, accountService, categoryService);
        var budgetMenu = new BudgetMenu(budgetService, userService, categoryService);
        var reportsMenu = new ReportsMenu(reportsService, userService);

        // Main menu loop
        Console.Clear();
        Console.WriteLine("==========================================");
        Console.WriteLine("  Finance Tracker - EF Core Edition");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("This console app demonstrates Entity Framework");
        Console.WriteLine("Core for PostgreSQL database access.");
        Console.WriteLine();

        while (true)
        {
            var choice = MenuHelper.ShowMenu("Main Menu", new[]
            {
                "Manage Users",
                "Manage Categories",
                "Manage Accounts",
                "Manage Transactions",
                "Manage Budgets",
                "Reports & Analytics",
                "Test Database Connection",
                "Exit"
            });

            try
            {
                switch (choice)
                {
                    case 1: await userMenu.ShowAsync(); break;
                    case 2: await categoryMenu.ShowAsync(); break;
                    case 3: await accountMenu.ShowAsync(); break;
                    case 4: await transactionMenu.ShowAsync(); break;
                    case 5: await budgetMenu.ShowAsync(); break;
                    case 6: await reportsMenu.ShowAsync(); break;
                    case 7: await TestConnectionAsync(context); break;
                    case 8:
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        MenuHelper.ShowError("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MenuHelper.ShowError($"An error occurred: {ex.Message}");
                MenuHelper.WaitForKey();
            }
        }
    }

    private static async Task TestConnectionAsync(FinanceDbContext context)
    {
        Console.WriteLine();
        Console.WriteLine("Testing database connection...");

        try
        {
            // EF Core: CanConnectAsync tests the connection
            if (await context.Database.CanConnectAsync())
            {
                MenuHelper.ShowSuccess("Database connection successful!");

                // Get statistics using EF Core LINQ
                var userCount = await context.Users.CountAsync();
                var categoryCount = await context.Categories.CountAsync();
                var accountCount = await context.Accounts.CountAsync();
                var transactionCount = await context.Transactions.CountAsync();
                var budgetCount = await context.Budgets.CountAsync();

                Console.WriteLine();
                Console.WriteLine("Database Statistics (via EF Core):");
                Console.WriteLine($"  Users:        {userCount}");
                Console.WriteLine($"  Categories:   {categoryCount}");
                Console.WriteLine($"  Accounts:     {accountCount}");
                Console.WriteLine($"  Transactions: {transactionCount}");
                Console.WriteLine($"  Budgets:      {budgetCount}");
            }
            else
            {
                MenuHelper.ShowError("Could not connect to database.");
            }
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Connection failed: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
