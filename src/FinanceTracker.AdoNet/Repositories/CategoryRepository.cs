using Npgsql;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// Category repository using raw ADO.NET.
/// </summary>
public class CategoryRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CategoryRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var categories = new List<Category>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, name, type, icon, color FROM categories ORDER BY type, name",
            connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, name, type, icon, color FROM categories WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapCategory(reader);
        }

        return null;
    }

    public async Task<List<Category>> GetByTypeAsync(CategoryType type)
    {
        var categories = new List<Category>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, name, type, icon, color FROM categories WHERE type = @type ORDER BY name",
            connection);

        command.Parameters.AddWithValue("@type", (short)type);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<int> CreateAsync(Category category)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO categories (name, type, icon, color) VALUES (@name, @type, @icon, @color) RETURNING id",
            connection);

        command.Parameters.AddWithValue("@name", category.Name);
        command.Parameters.AddWithValue("@type", (short)category.Type);
        command.Parameters.AddWithValue("@icon", (object?)category.Icon ?? DBNull.Value);
        command.Parameters.AddWithValue("@color", (object?)category.Color ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "UPDATE categories SET name = @name, type = @type, icon = @icon, color = @color WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", category.Id);
        command.Parameters.AddWithValue("@name", category.Name);
        command.Parameters.AddWithValue("@type", (short)category.Type);
        command.Parameters.AddWithValue("@icon", (object?)category.Icon ?? DBNull.Value);
        command.Parameters.AddWithValue("@color", (object?)category.Color ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM categories WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Category MapCategory(NpgsqlDataReader reader)
    {
        return new Category
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = (CategoryType)reader.GetInt16(reader.GetOrdinal("type")),
            Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? null : reader.GetString(reader.GetOrdinal("icon")),
            Color = reader.IsDBNull(reader.GetOrdinal("color")) ? null : reader.GetString(reader.GetOrdinal("color"))
        };
    }
}
