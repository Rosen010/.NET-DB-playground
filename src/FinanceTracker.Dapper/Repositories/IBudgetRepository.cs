using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Repository interface for Budget entity operations.
/// </summary>
public interface IBudgetRepository
{
    Task<IEnumerable<Budget>> GetAllAsync();
    Task<Budget?> GetByIdAsync(int id);
    Task<IEnumerable<Budget>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Budget budget);
    Task<bool> UpdateAsync(Budget budget);
    Task<bool> DeleteAsync(int id);
}
