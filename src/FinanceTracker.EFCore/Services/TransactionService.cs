using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Data;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for Transaction entity operations using EF Core.
/// Demonstrates date filtering and eager loading.
/// </summary>
public class TransactionService
{
    private readonly FinanceDbContext _context;

    public TransactionService(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets recent transactions with Category included.
    /// EF Core: Take() translates to LIMIT.
    /// </summary>
    public async Task<List<Transaction>> GetRecentAsync(int count = 20)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Transaction>> GetByAccountIdAsync(int accountId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets transactions within a date range.
    /// EF Core: Date comparisons translate to SQL date comparisons.
    /// </summary>
    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.TransactionDate >= startDate.Date && t.TransactionDate <= endDate.Date)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }
}
