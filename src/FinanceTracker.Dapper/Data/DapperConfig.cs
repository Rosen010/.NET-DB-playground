using System.Reflection;
using Dapper;

namespace FinanceTracker.Dapper.Data;

/// <summary>
/// Configures Dapper to handle snake_case column names from PostgreSQL.
/// Maps database columns like "created_at" to C# properties like "CreatedAt".
/// </summary>
public static class DapperConfig
{
    /// <summary>
    /// Initializes Dapper configuration for PostgreSQL snake_case naming convention.
    /// Call this once at application startup.
    /// </summary>
    public static void Configure()
    {
        // Set up custom type mapping for snake_case to PascalCase conversion
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
