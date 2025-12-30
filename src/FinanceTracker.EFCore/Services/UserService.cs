using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Data;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for User entity operations using EF Core.
/// Demonstrates basic CRUD with DbContext and LINQ queries.
/// </summary>
public class UserService
{
    private readonly FinanceDbContext _context;

    public UserService(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all users ordered by name.
    /// EF Core: Uses LINQ OrderBy which translates to SQL ORDER BY.
    /// </summary>
    public async Task<List<User>> GetAllAsync()
    {
        // AsNoTracking() improves performance when we don't need to modify entities
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a user by ID.
    /// EF Core: Uses FindAsync which checks the local cache first.
    /// </summary>
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <summary>
    /// Gets a user by email.
    /// EF Core: FirstOrDefaultAsync translates to LIMIT 1.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Creates a new user.
    /// EF Core: Add marks entity as Added, SaveChanges generates INSERT.
    /// </summary>
    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // EF Core automatically populates the ID after SaveChanges
        return user;
    }

    /// <summary>
    /// Updates an existing user.
    /// EF Core: Update marks entity as Modified, SaveChanges generates UPDATE.
    /// </summary>
    public async Task<bool> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Deletes a user by ID.
    /// EF Core: Cascade delete will remove related accounts and budgets.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
