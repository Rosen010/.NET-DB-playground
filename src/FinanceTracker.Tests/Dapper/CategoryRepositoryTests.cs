using FluentAssertions;
using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Tests.Infrastructure;

namespace FinanceTracker.Tests.Dapper;

/// <summary>
/// Integration tests for CategoryRepository.
/// Tests CRUD operations and filtering by category type.
/// </summary>
[Collection("PostgreSQL")]
public class CategoryRepositoryTests
{
    private readonly PostgresTestFixture _fixture;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
        _repository = new CategoryRepository(fixture.ConnectionFactory);
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertCategory_AndReturnGeneratedId()
    {
        // Arrange
        var category = new Category
        {
            Name = $"Test Category {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "🧪",
            Color = "#FF5733"
        };

        // Act
        var id = await _repository.CreateAsync(category);

        // Assert
        id.Should().BeGreaterThan(0);

        var created = await _repository.GetByIdAsync(id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(category.Name);
        created.Type.Should().Be(CategoryType.Expense);
        created.Icon.Should().Be("🧪");
        created.Color.Should().Be("#FF5733");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = $"GetById Category {Guid.NewGuid():N}",
            Type = CategoryType.Income,
            Icon = "💵",
            Color = "#28A745"
        };
        var id = await _repository.CreateAsync(category);

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be(category.Name);
        result.Type.Should().Be(CategoryType.Income);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTypeAsync_ShouldReturnOnlyCategoriesOfSpecifiedType()
    {
        // Arrange - Create one of each type with unique names
        var expenseCategory = new Category
        {
            Name = $"Expense Type Test {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "📤",
            Color = "#DC3545"
        };
        var incomeCategory = new Category
        {
            Name = $"Income Type Test {Guid.NewGuid():N}",
            Type = CategoryType.Income,
            Icon = "📥",
            Color = "#28A745"
        };
        await _repository.CreateAsync(expenseCategory);
        await _repository.CreateAsync(incomeCategory);

        // Act
        var expenseCategories = (await _repository.GetByTypeAsync(CategoryType.Expense)).ToList();
        var incomeCategories = (await _repository.GetByTypeAsync(CategoryType.Income)).ToList();

        // Assert
        expenseCategories.Should().OnlyContain(c => c.Type == CategoryType.Expense);
        incomeCategories.Should().OnlyContain(c => c.Type == CategoryType.Income);

        expenseCategories.Should().Contain(c => c.Name == expenseCategory.Name);
        incomeCategories.Should().Contain(c => c.Name == incomeCategory.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var category1 = new Category
        {
            Name = $"All Test 1 {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "📦",
            Color = "#6C757D"
        };
        var category2 = new Category
        {
            Name = $"All Test 2 {Guid.NewGuid():N}",
            Type = CategoryType.Income,
            Icon = "📈",
            Color = "#17A2B8"
        };
        await _repository.CreateAsync(category1);
        await _repository.CreateAsync(category2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var categories = result.ToList();
        categories.Should().HaveCountGreaterThanOrEqualTo(2);
        categories.Should().Contain(c => c.Name == category1.Name);
        categories.Should().Contain(c => c.Name == category2.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCategory_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var category = new Category
        {
            Name = $"Update Test {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "🔧",
            Color = "#FFC107"
        };
        var id = await _repository.CreateAsync(category);

        // Act
        category.Id = id;
        category.Name = $"Updated Name {Guid.NewGuid():N}";
        category.Type = CategoryType.Income;
        category.Icon = "💰";
        category.Color = "#28A745";
        var result = await _repository.UpdateAsync(category);

        // Assert
        result.Should().BeTrue();

        var updated = await _repository.GetByIdAsync(id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(category.Name);
        updated.Type.Should().Be(CategoryType.Income);
        updated.Icon.Should().Be("💰");
        updated.Color.Should().Be("#28A745");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentCategory_ShouldReturnFalse()
    {
        // Arrange
        var category = new Category
        {
            Id = 99999,
            Name = "Non Existent",
            Type = CategoryType.Expense,
            Icon = "❓",
            Color = "#000000"
        };

        // Act
        var result = await _repository.UpdateAsync(category);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var category = new Category
        {
            Name = $"Delete Test {Guid.NewGuid():N}",
            Type = CategoryType.Expense,
            Icon = "🗑️",
            Color = "#DC3545"
        };
        var id = await _repository.CreateAsync(category);

        // Act
        var result = await _repository.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();

        var deleted = await _repository.GetByIdAsync(id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }
}
