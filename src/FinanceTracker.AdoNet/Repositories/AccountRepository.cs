using Npgsql;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// Account repository using raw ADO.NET.
/// </summary>
public class AccountRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public AccountRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Account>> GetAllAsync()
    {
        var accounts = new List<Account>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, name, type, balance, currency, created_at FROM accounts ORDER BY user_id, name",
            connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            accounts.Add(MapAccount(reader));
        }

        return accounts;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, name, type, balance, currency, created_at FROM accounts WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapAccount(reader);
        }

        return null;
    }

    public async Task<List<Account>> GetByUserIdAsync(int userId)
    {
        var accounts = new List<Account>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, user_id, name, type, balance, currency, created_at FROM accounts WHERE user_id = @userId ORDER BY name",
            connection);

        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            accounts.Add(MapAccount(reader));
        }

        return accounts;
    }

    public async Task<int> CreateAsync(Account account)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO accounts (user_id, name, type, balance, currency) VALUES (@userId, @name, @type, @balance, @currency) RETURNING id",
            connection);

        command.Parameters.AddWithValue("@userId", account.UserId);
        command.Parameters.AddWithValue("@name", account.Name);
        command.Parameters.AddWithValue("@type", (short)account.Type);
        command.Parameters.AddWithValue("@balance", account.Balance);
        command.Parameters.AddWithValue("@currency", account.Currency);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Account account)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "UPDATE accounts SET name = @name, type = @type, balance = @balance, currency = @currency WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", account.Id);
        command.Parameters.AddWithValue("@name", account.Name);
        command.Parameters.AddWithValue("@type", (short)account.Type);
        command.Parameters.AddWithValue("@balance", account.Balance);
        command.Parameters.AddWithValue("@currency", account.Currency);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM accounts WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Account MapAccount(NpgsqlDataReader reader)
    {
        return new Account
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Type = (AccountType)reader.GetInt16(reader.GetOrdinal("type")),
            Balance = reader.GetDecimal(reader.GetOrdinal("balance")),
            Currency = reader.GetString(reader.GetOrdinal("currency")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}
