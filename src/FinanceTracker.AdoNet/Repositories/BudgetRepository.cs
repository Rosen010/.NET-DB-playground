using Npgsql;
using NpgsqlTypes;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// Budget repository using raw ADO.NET.
/// </summary>
public class BudgetRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public BudgetRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Budget>> GetAllAsync()
    {
        var budgets = new List<Budget>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, category_id, amount, period, start_date FROM budgets ORDER BY user_id, category_id",
            connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            budgets.Add(MapBudget(reader));
        }

        return budgets;
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, category_id, amount, period, start_date FROM budgets WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapBudget(reader);
        }

        return null;
    }

    public async Task<List<Budget>> GetByUserIdAsync(int userId)
    {
        var budgets = new List<Budget>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, category_id, amount, period, start_date FROM budgets WHERE user_id = @userId ORDER BY category_id",
            connection);

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            budgets.Add(MapBudget(reader));
        }

        return budgets;
    }

    public async Task<int> CreateAsync(Budget budget)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO budgets (user_id, category_id, amount, period, start_date) VALUES (@userId, @categoryId, @amount, @period, @startDate) RETURNING id",
            connection);

        command.Parameters.AddWithValue("@userId", budget.UserId);
        command.Parameters.AddWithValue("@categoryId", budget.CategoryId);
        command.Parameters.AddWithValue("@amount", budget.Amount);
        command.Parameters.AddWithValue("@period", (short)budget.Period);
        command.Parameters.Add(new NpgsqlParameter("@startDate", NpgsqlDbType.Date) { Value = budget.StartDate.Date });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Budget budget)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "UPDATE budgets SET category_id = @categoryId, amount = @amount, period = @period, start_date = @startDate WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", budget.Id);
        command.Parameters.AddWithValue("@categoryId", budget.CategoryId);
        command.Parameters.AddWithValue("@amount", budget.Amount);
        command.Parameters.AddWithValue("@period", (short)budget.Period);
        command.Parameters.Add(new NpgsqlParameter("@startDate", NpgsqlDbType.Date) { Value = budget.StartDate.Date });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM budgets WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Budget MapBudget(NpgsqlDataReader reader)
    {
        return new Budget
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
            Period = (BudgetPeriod)reader.GetInt16(reader.GetOrdinal("period")),
            StartDate = reader.GetDateTime(reader.GetOrdinal("start_date"))
        };
    }
}
