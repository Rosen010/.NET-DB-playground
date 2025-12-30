using Npgsql;
using FinanceTracker.AdoNet.Data;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.AdoNet.Repositories;

/// <summary>
/// User repository using raw ADO.NET.
///
/// This implementation demonstrates:
/// - Manual SQL command creation
/// - Parameter binding with NpgsqlParameter
/// - DataReader for result mapping
/// - Proper resource disposal with using statements
///
/// ADO.NET Trade-offs:
/// + Full control over SQL
/// + No ORM overhead
/// + Best performance potential
/// - Verbose boilerplate code
/// - Manual mapping required
/// - No compile-time query validation
/// </summary>
public class UserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, email, name, created_at FROM users ORDER BY name",
            connection);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, email, name, created_at FROM users WHERE id = @id",
            connection);

        // ADO.NET: Parameters prevent SQL injection
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, email, name, created_at FROM users WHERE email = @email",
            connection);

        command.Parameters.AddWithValue("@email", email);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<int> CreateAsync(User user)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO users (email, name) VALUES (@email, @name) RETURNING id",
            connection);

        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@name", user.Name);

        // ExecuteScalarAsync returns the first column of the first row
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "UPDATE users SET email = @email, name = @name WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", user.Id);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@name", user.Name);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "DELETE FROM users WHERE id = @id",
            connection);

        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Maps a data reader row to a User entity.
    /// ADO.NET requires manual mapping for each column.
    /// </summary>
    private static User MapUser(NpgsqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}
