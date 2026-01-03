using Microsoft.Extensions.Configuration;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.AdoNet.Menu;
using FinanceTracker.AdoNet.Repositories;
using FinanceTracker.Database;

namespace FinanceTracker.AdoNet;

/// <summary>
/// Finance Tracker Console Application using raw ADO.NET.
///
/// This application demonstrates:
/// - Pure ADO.NET data access without any ORM
/// - Manual SQL command building with NpgsqlCommand
/// - Parameter binding for SQL injection prevention
/// - DataReader for efficient row-by-row result processing
/// - Manual object mapping from database results
///
/// ADO.NET Benefits:
/// - Maximum performance (no ORM overhead)
/// - Full control over every SQL statement
/// - Minimal dependencies
/// - Direct database interaction
///
/// Trade-offs vs Dapper/EF Core:
/// - More boilerplate code required
/// - Manual mapping for every query
/// - No compile-time query validation
/// - Higher maintenance burden
///
/// When to use ADO.NET:
/// - Performance-critical scenarios
/// - Simple CRUD with known queries
/// - Learning database fundamentals
/// - Legacy system maintenance
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Connection string not found in appsettings.json");
            Console.ResetColor();
            return;
        }

        MigrationRunner.MigrateUp(connectionString);
        var connectionFactory = new DbConnectionFactory(connectionString);

        // Create repositories
        var userRepo = new UserRepository(connectionFactory);
        var categoryRepo = new CategoryRepository(connectionFactory);
        var accountRepo = new AccountRepository(connectionFactory);
        var transactionRepo = new TransactionRepository(connectionFactory);
        var budgetRepo = new BudgetRepository(connectionFactory);
        var reportsRepo = new ReportsRepository(connectionFactory);

        // Create menus
        var userMenu = new UserMenu(userRepo);
        var categoryMenu = new CategoryMenu(categoryRepo);
        var accountMenu = new AccountMenu(accountRepo, userRepo);
        var transactionMenu = new TransactionMenu(transactionRepo, accountRepo, categoryRepo);
        var budgetMenu = new BudgetMenu(budgetRepo, userRepo, categoryRepo);
        var reportsMenu = new ReportsMenu(reportsRepo, userRepo);

        Console.Clear();
        Console.WriteLine("==========================================");
        Console.WriteLine("  Finance Tracker - ADO.NET Edition");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("This console app demonstrates raw ADO.NET");
        Console.WriteLine("for PostgreSQL database access.");
        Console.WriteLine("No ORM - pure SQL and DataReaders!");
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
                    case 7: await TestConnectionAsync(connectionFactory); break;
                    case 8:
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        MenuHelper.ShowError("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MenuHelper.ShowError($"Error: {ex.Message}");
                MenuHelper.WaitForKey();
            }
        }
    }

    private static async Task TestConnectionAsync(DbConnectionFactory connectionFactory)
    {
        Console.WriteLine("\nTesting database connection (raw ADO.NET)...");

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync();

            MenuHelper.ShowSuccess("Database connection successful!");

            // Get stats using raw SQL
            await using var command = new Npgsql.NpgsqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM users) as users,
                    (SELECT COUNT(*) FROM categories) as categories,
                    (SELECT COUNT(*) FROM accounts) as accounts,
                    (SELECT COUNT(*) FROM transactions) as transactions,
                    (SELECT COUNT(*) FROM budgets) as budgets",
                connection);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                Console.WriteLine("\nDatabase Statistics (via raw ADO.NET):");
                Console.WriteLine($"  Users:        {reader.GetInt64(0)}");
                Console.WriteLine($"  Categories:   {reader.GetInt64(1)}");
                Console.WriteLine($"  Accounts:     {reader.GetInt64(2)}");
                Console.WriteLine($"  Transactions: {reader.GetInt64(3)}");
                Console.WriteLine($"  Budgets:      {reader.GetInt64(4)}");
            }
        }
        catch (Exception ex)
        {
            MenuHelper.ShowError($"Connection failed: {ex.Message}");
        }

        MenuHelper.WaitForKey();
    }
}
