using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Data;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for Account entity operations using EF Core.
/// Demonstrates eager loading with Include().
/// </summary>
public class AccountService
{
    private readonly FinanceDbContext _context;

    public AccountService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts
            .AsNoTracking()
            .OrderBy(a => a.UserId)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets an account with its User loaded.
    /// EF Core: Include() generates a JOIN to load related data.
    /// </summary>
    public async Task<Account?> GetByIdWithUserAsync(int id)
    {
        return await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    /// <summary>
    /// Gets all accounts for a user.
    /// EF Core: Where clause with foreign key comparison.
    /// </summary>
    public async Task<List<Account>> GetByUserIdAsync(int userId)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Account> CreateAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Updates just the balance without loading the full entity.
    /// EF Core: ExecuteUpdateAsync for efficient single-column updates.
    /// </summary>
    public async Task<bool> UpdateBalanceAsync(int id, decimal newBalance)
    {
        var rowsAffected = await _context.Accounts
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Balance, newBalance));

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
            return false;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return true;
    }
}
