using System.Data;

namespace FinanceTracker.AdoNet.Data;

/// <summary>
/// Extension methods for IDataReader to simplify null handling.
///
/// ADO.NET requires explicit null checking with IsDBNull before reading values.
/// These extensions encapsulate that pattern for cleaner code.
/// </summary>
public static class DataReaderExtensions
{
    /// <summary>
    /// Gets a string value or null if DBNull.
    /// </summary>
    public static string? GetStringOrNull(this IDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    /// <summary>
    /// Gets a string value or null by column name.
    /// </summary>
    public static string? GetStringOrNull(this IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    /// <summary>
    /// Gets an int value by column name.
    /// </summary>
    public static int GetInt32(this IDataReader reader, string columnName)
    {
        return reader.GetInt32(reader.GetOrdinal(columnName));
    }

    /// <summary>
    /// Gets a short value by column name.
    /// </summary>
    public static short GetInt16(this IDataReader reader, string columnName)
    {
        return reader.GetInt16(reader.GetOrdinal(columnName));
    }

    /// <summary>
    /// Gets a decimal value by column name.
    /// </summary>
    public static decimal GetDecimal(this IDataReader reader, string columnName)
    {
        return reader.GetDecimal(reader.GetOrdinal(columnName));
    }

    /// <summary>
    /// Gets a DateTime value by column name.
    /// </summary>
    public static DateTime GetDateTime(this IDataReader reader, string columnName)
    {
        return reader.GetDateTime(reader.GetOrdinal(columnName));
    }

    /// <summary>
    /// Gets a string value by column name.
    /// </summary>
    public static string GetString(this IDataReader reader, string columnName)
    {
        return reader.GetString(reader.GetOrdinal(columnName));
    }
}
