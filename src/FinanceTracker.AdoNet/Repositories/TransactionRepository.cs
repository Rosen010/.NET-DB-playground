using Npgsql;
using NpgsqlTypes;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// Transaction repository using raw ADO.NET.
/// Demonstrates date parameter handling.
/// </summary>
public class TransactionRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public TransactionRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Transaction>> GetRecentAsync(int count = 20)
    {
        var transactions = new List<Transaction>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"SELECT id, account_id, category_id, amount, description, transaction_date, created_at FROM transactions ORDER BY transaction_date DESC LIMIT @count",
            connection);

        command.Parameters.AddWithValue("@count", count);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, account_id, category_id, amount, description, transaction_date, created_at FROM transactions WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapTransaction(reader);
        }

        return null;
    }

    public async Task<List<Transaction>> GetByAccountIdAsync(int accountId)
    {
        var transactions = new List<Transaction>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, account_id, category_id, amount, description, transaction_date, created_at FROM transactions WHERE account_id = @accountId ORDER BY transaction_date DESC",
            connection);

        command.Parameters.AddWithValue("@accountId", accountId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = new List<Transaction>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, account_id, category_id, amount, description, transaction_date, created_at FROM transactions WHERE transaction_date >= @startDate AND transaction_date <= @endDate ORDER BY transaction_date DESC",
            connection);

        // ADO.NET: Use NpgsqlDbType for explicit type mapping
        command.Parameters.Add(new NpgsqlParameter("@startDate", NpgsqlDbType.Date) { Value = startDate.Date });
        command.Parameters.Add(new NpgsqlParameter("@endDate", NpgsqlDbType.Date) { Value = endDate.Date });

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    public async Task<int> CreateAsync(Transaction transaction)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO transactions (account_id, category_id, amount, description, transaction_date) VALUES (@accountId, @categoryId, @amount, @description, @transactionDate) RETURNING id",
            connection);

        command.Parameters.AddWithValue("@accountId", transaction.AccountId);
        command.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@description", (object?)transaction.Description ?? DBNull.Value);
        command.Parameters.Add(new NpgsqlParameter("@transactionDate", NpgsqlDbType.Date) { Value = transaction.TransactionDate.Date });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(Transaction transaction)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "UPDATE transactions SET account_id = @accountId, category_id = @categoryId, amount = @amount, description = @description, transaction_date = @transactionDate WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", transaction.Id);
        command.Parameters.AddWithValue("@accountId", transaction.AccountId);
        command.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@description", (object?)transaction.Description ?? DBNull.Value);
        command.Parameters.Add(new NpgsqlParameter("@transactionDate", NpgsqlDbType.Date) { Value = transaction.TransactionDate.Date });

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM transactions WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Transaction MapTransaction(NpgsqlDataReader reader)
    {
        return new Transaction
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            AccountId = reader.GetInt32(reader.GetOrdinal("account_id")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("category_id")),
            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            TransactionDate = reader.GetDateTime(reader.GetOrdinal("transaction_date")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}
