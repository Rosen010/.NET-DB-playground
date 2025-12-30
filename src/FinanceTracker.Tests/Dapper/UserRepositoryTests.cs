using FluentAssertions;
using FinanceTracker.Dapper.Repositories;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Tests.Infrastructure;

namespace FinanceTracker.Tests.Dapper;

/// <summary>
/// Integration tests for UserRepository using a real PostgreSQL database via Testcontainers.
/// These tests verify actual database operations including SQL execution and mapping.
/// </summary>
[Collection("PostgreSQL")]
public class UserRepositoryTests
{
    private readonly PostgresTestFixture _fixture;
    private readonly UserRepository _repository;

    public UserRepositoryTests(PostgresTestFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(fixture.ConnectionFactory);
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertUser_AndReturnGeneratedId()
    {
        // Arrange
        var user = new User
        {
            Email = $"test-{Guid.NewGuid()}@example.com",
            Name = "Test User"
        };

        // Act
        var id = await _repository.CreateAsync(user);

        // Assert
        id.Should().BeGreaterThan(0);

        // Verify the user was actually created
        var created = await _repository.GetByIdAsync(id);
        created.Should().NotBeNull();
        created!.Email.Should().Be(user.Email);
        created.Name.Should().Be(user.Name);
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = $"getbyid-{Guid.NewGuid()}@example.com",
            Name = "GetById Test User"
        };
        var id = await _repository.CreateAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Name);
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
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = $"getbyemail-{Guid.NewGuid()}@example.com";
        var user = new User
        {
            Email = email,
            Name = "GetByEmail Test User"
        };
        await _repository.CreateAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange - Create a couple of test users
        var user1 = new User { Email = $"all1-{Guid.NewGuid()}@example.com", Name = "All Test User 1" };
        var user2 = new User { Email = $"all2-{Guid.NewGuid()}@example.com", Name = "All Test User 2" };
        await _repository.CreateAsync(user1);
        await _repository.CreateAsync(user2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        var users = result.ToList();
        users.Should().HaveCountGreaterThanOrEqualTo(2);
        users.Should().Contain(u => u.Email == user1.Email);
        users.Should().Contain(u => u.Email == user2.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingUser_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = $"update-{Guid.NewGuid()}@example.com",
            Name = "Original Name"
        };
        var id = await _repository.CreateAsync(user);

        // Act
        user.Id = id;
        user.Name = "Updated Name";
        user.Email = $"updated-{Guid.NewGuid()}@example.com";
        var result = await _repository.UpdateAsync(user);

        // Assert
        result.Should().BeTrue();

        var updated = await _repository.GetByIdAsync(id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 99999,
            Email = "nonexistent@example.com",
            Name = "Non Existent"
        };

        // Act
        var result = await _repository.UpdateAsync(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = $"delete-{Guid.NewGuid()}@example.com",
            Name = "To Be Deleted"
        };
        var id = await _repository.CreateAsync(user);

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
