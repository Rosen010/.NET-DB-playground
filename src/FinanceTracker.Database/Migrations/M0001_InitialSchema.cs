using FluentMigrator;

namespace FinanceTracker.Database.Migrations;

/// <summary>
/// Initial migration that creates the core schema for the finance tracker.
/// Creates Users, Categories, Accounts, Transactions, and Budgets tables.
/// </summary>
[Migration(1)]
public class M0001_InitialSchema : Migration
{
    /// <inheritdoc />
    public override void Up()
    {
        // Users table
        Create.Table("users")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        // Categories table (no FK to users - shared categories)
        Create.Table("categories")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("type").AsInt16().NotNullable() // 1 = Expense, 2 = Income
            .WithColumn("icon").AsString(50).Nullable()
            .WithColumn("color").AsString(7).Nullable(); // Hex color like #FF5733

        // Unique constraint on category name + type combination
        Create.Index("ix_categories_name_type")
            .OnTable("categories")
            .OnColumn("name").Ascending()
            .OnColumn("type").Ascending()
            .WithOptions().Unique();

        // Accounts table
        Create.Table("accounts")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("name").AsString(100).NotNullable()
            .WithColumn("type").AsInt16().NotNullable() // AccountType enum
            .WithColumn("balance").AsDecimal(18, 2).NotNullable().WithDefaultValue(0)
            .WithColumn("currency").AsString(3).NotNullable().WithDefaultValue("USD")
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.ForeignKey("fk_accounts_user_id")
            .FromTable("accounts").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.Index("ix_accounts_user_id")
            .OnTable("accounts")
            .OnColumn("user_id");

        // Transactions table
        Create.Table("transactions")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("account_id").AsInt32().NotNullable()
            .WithColumn("category_id").AsInt32().NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("description").AsString(500).Nullable()
            .WithColumn("transaction_date").AsDate().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.ForeignKey("fk_transactions_account_id")
            .FromTable("transactions").ForeignColumn("account_id")
            .ToTable("accounts").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_transactions_category_id")
            .FromTable("transactions").ForeignColumn("category_id")
            .ToTable("categories").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.None); // Don't allow deleting categories with transactions

        Create.Index("ix_transactions_account_id")
            .OnTable("transactions")
            .OnColumn("account_id");

        Create.Index("ix_transactions_category_id")
            .OnTable("transactions")
            .OnColumn("category_id");

        // Index for querying transactions by date range (common query pattern)
        Create.Index("ix_transactions_date")
            .OnTable("transactions")
            .OnColumn("transaction_date").Descending();

        // Budgets table
        Create.Table("budgets")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("category_id").AsInt32().NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("period").AsInt16().NotNullable() // 1 = Monthly, 2 = Yearly
            .WithColumn("start_date").AsDate().NotNullable();

        Create.ForeignKey("fk_budgets_user_id")
            .FromTable("budgets").ForeignColumn("user_id")
            .ToTable("users").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.Cascade);

        Create.ForeignKey("fk_budgets_category_id")
            .FromTable("budgets").ForeignColumn("category_id")
            .ToTable("categories").PrimaryColumn("id")
            .OnDelete(System.Data.Rule.None); // Don't allow deleting categories with budgets

        Create.Index("ix_budgets_user_id")
            .OnTable("budgets")
            .OnColumn("user_id");

        Create.Index("ix_budgets_category_id")
            .OnTable("budgets")
            .OnColumn("category_id");

        // Unique constraint: one budget per user/category/period combination
        Create.Index("ix_budgets_user_category_period")
            .OnTable("budgets")
            .OnColumn("user_id").Ascending()
            .OnColumn("category_id").Ascending()
            .OnColumn("period").Ascending()
            .WithOptions().Unique();
    }

    /// <inheritdoc />
    public override void Down()
    {
        Delete.Table("budgets");
        Delete.Table("transactions");
        Delete.Table("accounts");
        Delete.Table("categories");
        Delete.Table("users");
    }
}
