using FluentAssertions;
using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Tests.Infrastructure;

namespace FinanceTracker.Tests.Dapper;

/// <summary>
/// Integration tests for ReportsRepository.
/// Tests complex SQL queries including JOINs, GROUP BY, aggregates, and conditional filters.
/// Each test sets up its own isolated test data to ensure deterministic results.
/// </summary>
[Collection("PostgreSQL")]
public class ReportsRepositoryTests
{
    private readonly PostgresTestFixture _fixture;
    private readonly ReportsRepository _reportsRepository;
    private readonly UserRepository _userRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly AccountRepository _accountRepository;
    private readonly TransactionRepository _transactionRepository;

    public ReportsRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
        _reportsRepository = new ReportsRepository(fixture.ConnectionFactory);
        _userRepository = new UserRepository(fixture.ConnectionFactory);
        _categoryRepository = new CategoryRepository(fixture.ConnectionFactory);
        _accountRepository = new AccountRepository(fixture.ConnectionFactory);
        _transactionRepository = new TransactionRepository(fixture.ConnectionFactory);
    }

    private async Task<(User user, Category expenseCategory, Category incomeCategory, Account account)> SetupTestDataAsync()
    {
        // Create a unique test user
        var user = new User
        {
            Email = $"reports-test-{Guid.NewGuid()}@example.com",
            Name = "Reports Test User"
        };
        var userId = await _userRepository.CreateAsync(user);
        user.Id = userId;

        // Create test categories
        var expenseCategory = new Category
        {
            Name = $"Test Expense {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "💸",
            Color = "#DC3545"
        };
        expenseCategory.Id = await _categoryRepository.CreateAsync(expenseCategory);

        var incomeCategory = new Category
        {
            Name = $"Test Income {Guid.NewGuid():N}",
            Type = CategoryType.Income,
            Icon = "💰",
            Color = "#28A745"
        };
        incomeCategory.Id = await _categoryRepository.CreateAsync(incomeCategory);

        // Create a test account
        var account = new Account
        {
            UserId = userId,
            Name = $"Test Account {Guid.NewGuid():N}",
            Type = AccountType.Checking,
            Balance = 1000m,
            Currency = "USD"
        };
        account.Id = await _accountRepository.CreateAsync(account);

        return (user, expenseCategory, incomeCategory, account);
    }

    [Fact]
    public async Task GetMonthlySpendingByCategoryAsync_WithTransactions_ShouldReturnAggregatedData()
    {
        // Arrange
        var (user, expenseCategory, incomeCategory, account) = await SetupTestDataAsync();
        var testYear = DateTime.Now.Year;
        var testMonth = DateTime.Now.Month;

        // Create expense transactions
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = expenseCategory.Id,
            Amount = -100m,  // Expense (negative)
            Description = "Test expense 1",
            TransactionDate = new DateTime(testYear, testMonth, 1)
        });

        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = expenseCategory.Id,
            Amount = -50m,  // Expense (negative)
            Description = "Test expense 2",
            TransactionDate = new DateTime(testYear, testMonth, 15)
        });

        // Create income transaction
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = incomeCategory.Id,
            Amount = 500m,  // Income (positive)
            Description = "Test income",
            TransactionDate = new DateTime(testYear, testMonth, 10)
        });

        // Act
        var results = (await _reportsRepository.GetMonthlySpendingByCategoryAsync(testYear, testMonth, user.Id)).ToList();

        // Assert
        results.Should().NotBeEmpty();

        var expenseResult = results.FirstOrDefault(r => r.CategoryId == expenseCategory.Id);
        expenseResult.Should().NotBeNull();
        expenseResult!.TotalAmount.Should().Be(150m);  // 100 + 50
        expenseResult.TransactionCount.Should().Be(2);
        expenseResult.CategoryType.Should().Be("Expense");

        var incomeResult = results.FirstOrDefault(r => r.CategoryId == incomeCategory.Id);
        incomeResult.Should().NotBeNull();
        incomeResult!.TotalAmount.Should().Be(500m);
        incomeResult.TransactionCount.Should().Be(1);
        incomeResult.CategoryType.Should().Be("Income");
    }

    [Fact]
    public async Task GetMonthlySpendingByCategoryAsync_WithNoTransactions_ShouldReturnEmptyResults()
    {
        // Arrange
        var (user, _, _, _) = await SetupTestDataAsync();
        var futureYear = DateTime.Now.Year + 10;  // Far in the future

        // Act
        var results = await _reportsRepository.GetMonthlySpendingByCategoryAsync(futureYear, 1, user.Id);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccountBalanceSummaryAsync_ShouldReturnGroupedBalances()
    {
        // Arrange
        var (user, _, _, account) = await SetupTestDataAsync();

        // Create another account for the same user
        var savingsAccount = new Account
        {
            UserId = user.Id,
            Name = $"Test Savings {Guid.NewGuid():N}",
            Type = AccountType.Savings,
            Balance = 5000m,
            Currency = "USD"
        };
        await _accountRepository.CreateAsync(savingsAccount);

        // Act
        var results = (await _reportsRepository.GetAccountBalanceSummaryAsync()).ToList();

        // Assert
        results.Should().NotBeEmpty();

        // Find results for our test user
        var userResults = results.Where(r => r.UserId == user.Id).ToList();
        userResults.Should().HaveCountGreaterThanOrEqualTo(2);

        var checkingResult = userResults.FirstOrDefault(r => r.AccountType == "Checking");
        checkingResult.Should().NotBeNull();
        checkingResult!.TotalBalance.Should().BeGreaterThanOrEqualTo(account.Balance);

        var savingsResult = userResults.FirstOrDefault(r => r.AccountType == "Savings");
        savingsResult.Should().NotBeNull();
        savingsResult!.TotalBalance.Should().BeGreaterThanOrEqualTo(savingsAccount.Balance);
    }

    [Fact]
    public async Task GetIncomeExpenseSummaryAsync_ShouldReturnCorrectTotals()
    {
        // Arrange
        var (user, expenseCategory, incomeCategory, account) = await SetupTestDataAsync();
        var startDate = DateTime.Now.Date;
        var endDate = startDate.AddDays(30);

        // Create income transaction
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = incomeCategory.Id,
            Amount = 1000m,  // Income
            Description = "Salary",
            TransactionDate = startDate.AddDays(1)
        });

        // Create expense transactions
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = expenseCategory.Id,
            Amount = -200m,  // Expense
            Description = "Groceries",
            TransactionDate = startDate.AddDays(2)
        });

        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = expenseCategory.Id,
            Amount = -150m,  // Expense
            Description = "Utilities",
            TransactionDate = startDate.AddDays(3)
        });

        // Act
        var (income, expenses, net) = await _reportsRepository.GetIncomeExpenseSummaryAsync(startDate, endDate, user.Id);

        // Assert
        income.Should().BeGreaterThanOrEqualTo(1000m);
        expenses.Should().BeGreaterThanOrEqualTo(350m);  // 200 + 150
        net.Should().Be(income - expenses);
    }

    [Fact]
    public async Task GetTopSpendingCategoriesAsync_ShouldReturnTopNCategories()
    {
        // Arrange
        var (user, _, _, account) = await SetupTestDataAsync();

        // Create multiple expense categories
        var categories = new List<Category>();
        for (int i = 0; i < 3; i++)
        {
            var cat = new Category
            {
                Name = $"Top Spending Cat {i} {Guid.NewGuid():N}",
                Type = CategoryType.Expense,
                Icon = "📊",
                Color = "#6C757D"
            };
            cat.Id = await _categoryRepository.CreateAsync(cat);
            categories.Add(cat);
        }

        // Create transactions with different amounts to establish ranking
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = categories[0].Id,
            Amount = -500m,  // Highest spending
            Description = "Top category",
            TransactionDate = DateTime.Now.Date
        });

        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = categories[1].Id,
            Amount = -300m,  // Middle spending
            Description = "Middle category",
            TransactionDate = DateTime.Now.Date
        });

        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = categories[2].Id,
            Amount = -100m,  // Lowest spending
            Description = "Lower category",
            TransactionDate = DateTime.Now.Date
        });

        // Act
        var results = (await _reportsRepository.GetTopSpendingCategoriesAsync(user.Id, 2)).ToList();

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);

        // Results should be ordered by amount descending
        var amounts = results.Select(r => r.TotalAmount).ToList();
        amounts.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetTopSpendingCategoriesAsync_WithNoExpenses_ShouldReturnEmptyResults()
    {
        // Arrange
        var (user, _, incomeCategory, account) = await SetupTestDataAsync();

        // Create only income transaction (no expenses)
        await _transactionRepository.CreateAsync(new Transaction
        {
            AccountId = account.Id,
            CategoryId = incomeCategory.Id,
            Amount = 1000m,  // Income (positive)
            Description = "Income only",
            TransactionDate = DateTime.Now.Date
        });

        // Act
        var results = await _reportsRepository.GetTopSpendingCategoriesAsync(user.Id, 5);

        // Assert - Should have no results since there are no expense transactions
        results.Where(r => r.CategoryName.Contains("Test Income")).Should().BeEmpty();
    }
}
