using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.EFCore.Data;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for Budget entity operations using EF Core.
/// </summary>
public class BudgetService
{
    private readonly FinanceDbContext _context;

    public BudgetService(FinanceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all budgets with User and Category loaded.
    /// EF Core: Multiple Include() calls generate multiple JOINs.
    /// </summary>
    public async Task<List<Budget>> GetAllAsync()
    {
        return await _context.Budgets
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Category)
            .OrderBy(b => b.UserId)
            .ThenBy(b => b.CategoryId)
            .ToListAsync();
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        return await _context.Budgets
            .Include(b => b.User)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Budget>> GetByUserIdAsync(int userId)
    {
        return await _context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.CategoryId)
            .ToListAsync();
    }

    public async Task<Budget> CreateAsync(Budget budget)
    {
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<bool> UpdateAsync(Budget budget)
    {
        _context.Budgets.Update(budget);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget == null)
            return false;

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();
        return true;
    }
}
