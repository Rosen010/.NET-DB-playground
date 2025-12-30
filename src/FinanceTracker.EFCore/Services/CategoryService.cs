using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.EFCore.Data;

namespace FinanceTracker.EFCore.Services;

/// <summary>
/// Service for Category entity operations using EF Core.
/// </summary>
public class CategoryService
{
    private readonly FinanceDbContext _context;

    public CategoryService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    /// <summary>
    /// Gets categories by type (Expense or Income).
    /// EF Core: Where clause translates to SQL WHERE.
    /// </summary>
    public async Task<List<Category>> GetByTypeAsync(CategoryType type)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
