using Dapper;
using Microsoft.Extensions.Configuration;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Dapper.Menu;
using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Database;

namespace FinanceTracker.Dapper;

/// <summary>
/// Finance Tracker Console Application using Dapper for data access.
///
/// This application demonstrates:
/// - Dapper ORM for lightweight, high-performance data access
/// - Repository pattern for clean separation of concerns
/// - PostgreSQL database interaction
/// - Configuration management with appsettings.json
///
/// Dapper Benefits:
/// - Close to raw SQL performance
/// - Full control over queries
/// - Simple object mapping
/// - No change tracking overhead
///
/// Trade-offs:
/// - Manual SQL writing required
/// - No automatic migrations (using FluentMigrator separately)
/// - No lazy loading or relationship management
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure Dapper for PostgreSQL snake_case column names
        DapperConfig.Configure();

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

        // Create connection factory and repositories
        var connectionFactory = new DbConnectionFactory(connectionString);

        var userRepository = new UserRepository(connectionFactory);
        var categoryRepository = new CategoryRepository(connectionFactory);
        var accountRepository = new AccountRepository(connectionFactory);
        var transactionRepository = new TransactionRepository(connectionFactory);
        var budgetRepository = new BudgetRepository(connectionFactory);
        var reportsRepository = new ReportsRepository(connectionFactory);

        // Create menu handlers
        var userMenu = new UserMenu(userRepository);
        var categoryMenu = new CategoryMenu(categoryRepository);
        var accountMenu = new AccountMenu(accountRepository, userRepository);
        var transactionMenu = new TransactionMenu(transactionRepository, accountRepository, categoryRepository);
        var budgetMenu = new BudgetMenu(budgetRepository, userRepository, categoryRepository);
        var reportsMenu = new ReportsMenu(reportsRepository, userRepository);

        // Main menu loop
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("  Finance Tracker - Dapper Edition");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("This console app demonstrates Dapper ORM");
        Console.WriteLine("for PostgreSQL database access.");
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
                    case 1:
                        await userMenu.ShowAsync();
                        break;
                    case 2:
                        await categoryMenu.ShowAsync();
                        break;
                    case 3:
                        await accountMenu.ShowAsync();
                        break;
                    case 4:
                        await transactionMenu.ShowAsync();
                        break;
                    case 5:
                        await budgetMenu.ShowAsync();
                        break;
                    case 6:
                        await reportsMenu.ShowAsync();
                        break;
                    case 7:
                        await TestConnectionAsync(connectionFactory);
                        break;
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

    private static async Task TestConnectionAsync(DbConnectionFactory connectionFactory)
    {
        Console.WriteLine();
        Console.WriteLine("Testing database connection...");

        try
        {
            using var connection = connectionFactory.CreateConnection();
            connection.Open();

            MenuHelper.ShowSuccess("Database connection successful!");

            // Show some basic stats using Dapper
            var stats = await connection.QueryFirstAsync<dynamic>(@"
                SELECT
                    (SELECT COUNT(*) FROM users) as user_count,
                    (SELECT COUNT(*) FROM categories) as category_count,
                    (SELECT COUNT(*) FROM accounts) as account_count,
                    (SELECT COUNT(*) FROM transactions) as transaction_count,
                    (SELECT COUNT(*) FROM budgets) as budget_count");

            Console.WriteLine();
            Console.WriteLine("Database Statistics:");
            Console.WriteLine($"  Users:        {stats.user_count}");
            Console.WriteLine($"  Categories:   {stats.category_count}");
            Console.WriteLine($"  Accounts:     {stats.account_count}");
            Console.WriteLine($"  Transactions: {stats.transaction_count}");
            Console.WriteLine($"  Budgets:      {stats.budget_count}");
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Connection failed: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
