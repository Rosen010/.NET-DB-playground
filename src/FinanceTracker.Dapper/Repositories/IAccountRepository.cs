using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Repository interface for Account entity operations.
/// </summary>
public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Account account);
    Task<bool> UpdateAsync(Account account);
    Task<bool> UpdateBalanceAsync(int id, decimal newBalance);
    Task<bool> DeleteAsync(int id);
}
