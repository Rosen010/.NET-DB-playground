using FluentMigrator;

namespace FinanceTracker.Database.Migrations;

/// <summary>
/// Seeds the database with sample data for testing and development.
/// Includes sample users, categories, accounts, transactions, and budgets.
/// </summary>
[Migration(2)]
public class M0002_SeedData : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        // ===========================================
        // USERS
        // ===========================================
        Insert.IntoTable("users")
            .Row(new { email = "john.doe@example.com", name = "John Doe" })
            .Row(new { email = "jane.smith@example.com", name = "Jane Smith" });

        // ===========================================
        // CATEGORIES - Expenses (type = 1)
        // ===========================================
        Insert.IntoTable("categories")
            .Row(new { name = "Groceries", type = 1, icon = "shopping-cart", color = "#4CAF50" })
            .Row(new { name = "Dining Out", type = 1, icon = "utensils", color = "#FF9800" })
            .Row(new { name = "Transportation", type = 1, icon = "car", color = "#2196F3" })
            .Row(new { name = "Utilities", type = 1, icon = "bolt", color = "#FFC107" })
            .Row(new { name = "Entertainment", type = 1, icon = "film", color = "#9C27B0" })
            .Row(new { name = "Healthcare", type = 1, icon = "heart", color = "#F44336" })
            .Row(new { name = "Shopping", type = 1, icon = "bag", color = "#E91E63" })
            .Row(new { name = "Housing", type = 1, icon = "home", color = "#795548" })
            .Row(new { name = "Insurance", type = 1, icon = "shield", color = "#607D8B" })
            .Row(new { name = "Personal Care", type = 1, icon = "user", color = "#00BCD4" });

        // ===========================================
        // CATEGORIES - Income (type = 2)
        // ===========================================
        Insert.IntoTable("categories")
            .Row(new { name = "Salary", type = 2, icon = "briefcase", color = "#4CAF50" })
            .Row(new { name = "Freelance", type = 2, icon = "laptop", color = "#8BC34A" })
            .Row(new { name = "Investments", type = 2, icon = "chart-line", color = "#00BCD4" })
            .Row(new { name = "Gifts", type = 2, icon = "gift", color = "#E91E63" })
            .Row(new { name = "Refunds", type = 2, icon = "undo", color = "#9E9E9E" });

        // ===========================================
        // ACCOUNTS - John Doe (user_id = 1)
        // ===========================================
        Insert.IntoTable("accounts")
            .Row(new { user_id = 1, name = "Primary Checking", type = 1, balance = 5250.00m, currency = "USD" })
            .Row(new { user_id = 1, name = "Emergency Savings", type = 2, balance = 15000.00m, currency = "USD" })
            .Row(new { user_id = 1, name = "Visa Credit Card", type = 3, balance = -1250.75m, currency = "USD" })
            .Row(new { user_id = 1, name = "Cash Wallet", type = 4, balance = 150.00m, currency = "USD" });

        // ===========================================
        // ACCOUNTS - Jane Smith (user_id = 2)
        // ===========================================
        Insert.IntoTable("accounts")
            .Row(new { user_id = 2, name = "Checking Account", type = 1, balance = 3800.50m, currency = "USD" })
            .Row(new { user_id = 2, name = "Savings Account", type = 2, balance = 22500.00m, currency = "USD" })
            .Row(new { user_id = 2, name = "Investment Portfolio", type = 5, balance = 45000.00m, currency = "USD" });

        // ===========================================
        // TRANSACTIONS - John Doe's Primary Checking (account_id = 1)
        // Spanning October - December 2024
        // ===========================================

        // October 2024 Transactions
        Insert.IntoTable("transactions")
            .Row(new { account_id = 1, category_id = 11, amount = 4500.00m, description = "Monthly salary", transaction_date = "2024-10-01" })
            .Row(new { account_id = 1, category_id = 1, amount = -125.50m, description = "Weekly groceries - Walmart", transaction_date = "2024-10-05" })
            .Row(new { account_id = 1, category_id = 4, amount = -85.00m, description = "Electric bill", transaction_date = "2024-10-08" })
            .Row(new { account_id = 1, category_id = 3, amount = -45.00m, description = "Gas station fill-up", transaction_date = "2024-10-10" })
            .Row(new { account_id = 1, category_id = 2, amount = -65.00m, description = "Dinner at Italian restaurant", transaction_date = "2024-10-12" })
            .Row(new { account_id = 1, category_id = 1, amount = -98.75m, description = "Weekly groceries - Costco", transaction_date = "2024-10-15" })
            .Row(new { account_id = 1, category_id = 5, amount = -15.99m, description = "Netflix subscription", transaction_date = "2024-10-18" })
            .Row(new { account_id = 1, category_id = 8, amount = -1500.00m, description = "Monthly rent", transaction_date = "2024-10-01" });

        // November 2024 Transactions
        Insert.IntoTable("transactions")
            .Row(new { account_id = 1, category_id = 11, amount = 4500.00m, description = "Monthly salary", transaction_date = "2024-11-01" })
            .Row(new { account_id = 1, category_id = 8, amount = -1500.00m, description = "Monthly rent", transaction_date = "2024-11-01" })
            .Row(new { account_id = 1, category_id = 1, amount = -142.30m, description = "Weekly groceries", transaction_date = "2024-11-03" })
            .Row(new { account_id = 1, category_id = 4, amount = -120.00m, description = "Gas and electric", transaction_date = "2024-11-05" })
            .Row(new { account_id = 1, category_id = 6, amount = -75.00m, description = "Doctor visit copay", transaction_date = "2024-11-08" })
            .Row(new { account_id = 1, category_id = 1, amount = -88.45m, description = "Weekly groceries", transaction_date = "2024-11-10" })
            .Row(new { account_id = 1, category_id = 2, amount = -45.00m, description = "Lunch with friends", transaction_date = "2024-11-15" })
            .Row(new { account_id = 1, category_id = 3, amount = -52.00m, description = "Gas station", transaction_date = "2024-11-18" })
            .Row(new { account_id = 1, category_id = 5, amount = -15.99m, description = "Netflix subscription", transaction_date = "2024-11-18" })
            .Row(new { account_id = 1, category_id = 7, amount = -250.00m, description = "Winter jacket", transaction_date = "2024-11-22" })
            .Row(new { account_id = 1, category_id = 14, amount = 100.00m, description = "Birthday gift from parents", transaction_date = "2024-11-25" });

        // December 2024 Transactions
        Insert.IntoTable("transactions")
            .Row(new { account_id = 1, category_id = 11, amount = 4500.00m, description = "Monthly salary", transaction_date = "2024-12-01" })
            .Row(new { account_id = 1, category_id = 8, amount = -1500.00m, description = "Monthly rent", transaction_date = "2024-12-01" })
            .Row(new { account_id = 1, category_id = 1, amount = -185.00m, description = "Holiday groceries", transaction_date = "2024-12-05" })
            .Row(new { account_id = 1, category_id = 4, amount = -150.00m, description = "Winter heating bill", transaction_date = "2024-12-08" })
            .Row(new { account_id = 1, category_id = 7, amount = -350.00m, description = "Christmas gifts", transaction_date = "2024-12-15" })
            .Row(new { account_id = 1, category_id = 2, amount = -120.00m, description = "Holiday dinner out", transaction_date = "2024-12-20" })
            .Row(new { account_id = 1, category_id = 5, amount = -15.99m, description = "Netflix subscription", transaction_date = "2024-12-18" });

        // ===========================================
        // TRANSACTIONS - John's Credit Card (account_id = 3)
        // ===========================================
        Insert.IntoTable("transactions")
            .Row(new { account_id = 3, category_id = 7, amount = -89.99m, description = "Amazon purchase", transaction_date = "2024-11-10" })
            .Row(new { account_id = 3, category_id = 5, amount = -60.00m, description = "Concert tickets", transaction_date = "2024-11-20" })
            .Row(new { account_id = 3, category_id = 7, amount = -199.99m, description = "Electronics purchase", transaction_date = "2024-12-01" });

        // ===========================================
        // TRANSACTIONS - Jane Smith's Checking (account_id = 5)
        // ===========================================

        // October 2024
        Insert.IntoTable("transactions")
            .Row(new { account_id = 5, category_id = 11, amount = 5500.00m, description = "Monthly salary", transaction_date = "2024-10-01" })
            .Row(new { account_id = 5, category_id = 12, amount = 800.00m, description = "Freelance web design", transaction_date = "2024-10-15" })
            .Row(new { account_id = 5, category_id = 1, amount = -175.00m, description = "Grocery shopping", transaction_date = "2024-10-08" })
            .Row(new { account_id = 5, category_id = 8, amount = -1800.00m, description = "Apartment rent", transaction_date = "2024-10-01" })
            .Row(new { account_id = 5, category_id = 9, amount = -200.00m, description = "Health insurance", transaction_date = "2024-10-05" })
            .Row(new { account_id = 5, category_id = 10, amount = -85.00m, description = "Gym membership", transaction_date = "2024-10-10" });

        // November 2024
        Insert.IntoTable("transactions")
            .Row(new { account_id = 5, category_id = 11, amount = 5500.00m, description = "Monthly salary", transaction_date = "2024-11-01" })
            .Row(new { account_id = 5, category_id = 12, amount = 1200.00m, description = "Freelance project completion", transaction_date = "2024-11-20" })
            .Row(new { account_id = 5, category_id = 8, amount = -1800.00m, description = "Apartment rent", transaction_date = "2024-11-01" })
            .Row(new { account_id = 5, category_id = 1, amount = -165.00m, description = "Grocery shopping", transaction_date = "2024-11-12" })
            .Row(new { account_id = 5, category_id = 9, amount = -200.00m, description = "Health insurance", transaction_date = "2024-11-05" })
            .Row(new { account_id = 5, category_id = 2, amount = -95.00m, description = "Birthday dinner", transaction_date = "2024-11-15" });

        // December 2024
        Insert.IntoTable("transactions")
            .Row(new { account_id = 5, category_id = 11, amount = 5500.00m, description = "Monthly salary", transaction_date = "2024-12-01" })
            .Row(new { account_id = 5, category_id = 13, amount = 450.00m, description = "Dividend payment", transaction_date = "2024-12-15" })
            .Row(new { account_id = 5, category_id = 8, amount = -1800.00m, description = "Apartment rent", transaction_date = "2024-12-01" })
            .Row(new { account_id = 5, category_id = 1, amount = -225.00m, description = "Holiday groceries", transaction_date = "2024-12-10" })
            .Row(new { account_id = 5, category_id = 7, amount = -400.00m, description = "Holiday shopping", transaction_date = "2024-12-18" });

        // ===========================================
        // BUDGETS - John Doe (user_id = 1)
        // Monthly budgets for expense categories
        // ===========================================
        Insert.IntoTable("budgets")
            .Row(new { user_id = 1, category_id = 1, amount = 500.00m, period = 1, start_date = "2024-10-01" })  // Groceries
            .Row(new { user_id = 1, category_id = 2, amount = 200.00m, period = 1, start_date = "2024-10-01" })  // Dining Out
            .Row(new { user_id = 1, category_id = 3, amount = 150.00m, period = 1, start_date = "2024-10-01" })  // Transportation
            .Row(new { user_id = 1, category_id = 4, amount = 200.00m, period = 1, start_date = "2024-10-01" })  // Utilities
            .Row(new { user_id = 1, category_id = 5, amount = 100.00m, period = 1, start_date = "2024-10-01" })  // Entertainment
            .Row(new { user_id = 1, category_id = 7, amount = 300.00m, period = 1, start_date = "2024-10-01" })  // Shopping
            .Row(new { user_id = 1, category_id = 8, amount = 1500.00m, period = 1, start_date = "2024-10-01" }); // Housing

        // ===========================================
        // BUDGETS - Jane Smith (user_id = 2)
        // Monthly budgets
        // ===========================================
        Insert.IntoTable("budgets")
            .Row(new { user_id = 2, category_id = 1, amount = 400.00m, period = 1, start_date = "2024-10-01" })  // Groceries
            .Row(new { user_id = 2, category_id = 2, amount = 150.00m, period = 1, start_date = "2024-10-01" })  // Dining Out
            .Row(new { user_id = 2, category_id = 7, amount = 250.00m, period = 1, start_date = "2024-10-01" })  // Shopping
            .Row(new { user_id = 2, category_id = 8, amount = 1800.00m, period = 1, start_date = "2024-10-01" }) // Housing
            .Row(new { user_id = 2, category_id = 9, amount = 250.00m, period = 1, start_date = "2024-10-01" })  // Insurance
            .Row(new { user_id = 2, category_id = 10, amount = 100.00m, period = 1, start_date = "2024-10-01" }); // Personal Care
    }

    /// <inheritdoc />
    public override void Down()
    {
        // Delete in reverse order of foreign key dependencies
        Delete.FromTable("budgets").AllRows();
        Delete.FromTable("transactions").AllRows();
        Delete.FromTable("accounts").AllRows();
        Delete.FromTable("categories").AllRows();
        Delete.FromTable("users").AllRows();
    }
}
