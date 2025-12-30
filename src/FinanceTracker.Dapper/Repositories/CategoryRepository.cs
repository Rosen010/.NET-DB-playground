using Dapper;
using FinanceTracker.Dapper.Data;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Dapper.Repositories;

/// <summary>
/// Dapper implementation of the category repository.
/// Categories are shared across users (not user-specific).
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CategoryRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, name, type, icon, color
            FROM categories
            ORDER BY type, name";

        return await connection.QueryAsync<Category>(sql);
    }

    /// <inheritdoc />
    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT id, name, type, icon, color FROM categories WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetByTypeAsync(CategoryType type)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT id, name, type, icon, color
            FROM categories
            WHERE type = @Type
            ORDER BY name";

        // Cast enum to int for PostgreSQL
        return await connection.QueryAsync<Category>(sql, new { Type = (int)type });
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO categories (name, type, icon, color)
            VALUES (@Name, @Type, @Icon, @Color)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            category.Name,
            Type = (int)category.Type,
            category.Icon,
            category.Color
        });
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE categories
            SET name = @Name, type = @Type, icon = @Icon, color = @Color
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            category.Id,
            category.Name,
            Type = (int)category.Type,
            category.Icon,
            category.Color
        });

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        // This will fail if there are transactions or budgets referencing this category
        // due to FK constraints (ON DELETE NO ACTION)
        const string sql = "DELETE FROM categories WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }
}
