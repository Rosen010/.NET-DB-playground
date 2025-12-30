# PostgreSQL Learning Playground - Personal Finance Tracker

A .NET 8 playground project for learning PostgreSQL and comparing data access frameworks (Dapper, Entity Framework Core, and raw ADO.NET). This project implements a personal finance tracker with the same functionality across three different data access approaches.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [1. PostgreSQL Setup](#1-postgresql-setup)
  - [2. Clone and Build](#2-clone-and-build)
  - [3. Configure Connection String](#3-configure-connection-string)
  - [4. Run Database Migrations](#4-run-database-migrations)
  - [5. Run the Console Apps](#5-run-the-console-apps)
- [Running Tests](#running-tests)
- [Database Schema](#database-schema)
- [Framework Comparison](#framework-comparison)
- [Query Examples](#query-examples)

## Features

- **Domain Model**: Users, Accounts, Categories, Transactions, and Budgets
- **Three Data Access Implementations**: Same functionality with Dapper, EF Core, and ADO.NET
- **Complex Queries**: Monthly spending reports, budget tracking, income vs expenses analysis
- **Menu-Driven Console Apps**: Interactive exploration of each framework
- **Database Migrations**: Version-controlled schema with FluentMigrator
- **Integration Tests**: Testcontainers-based tests for Dapper repositories

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [PostgreSQL 14+](https://www.postgresql.org/download/) (or use Docker)
- [Docker](https://www.docker.com/get-started) (optional, for running tests)

### Installing PostgreSQL

**Option 1: Direct Installation**

- **Windows**: Download from [postgresql.org](https://www.postgresql.org/download/windows/) or use `winget install PostgreSQL.PostgreSQL`
- **macOS**: `brew install postgresql@16`
- **Linux**: `sudo apt install postgresql` (Debian/Ubuntu) or `sudo dnf install postgresql-server` (Fedora)

**Option 2: Using Docker**

```bash
docker run --name postgres-finance \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=finance_tracker \
  -p 5432:5432 \
  -d postgres:16-alpine
```

## Project Structure

```
FinanceTracker.sln
├── src/
│   ├── FinanceTracker.Domain/          # Shared entities and enums
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Account.cs
│   │   │   ├── Category.cs
│   │   │   ├── Transaction.cs
│   │   │   └── Budget.cs
│   │   └── Enums/
│   │       ├── AccountType.cs
│   │       ├── CategoryType.cs
│   │       └── BudgetPeriod.cs
│   │
│   ├── FinanceTracker.Database/        # FluentMigrator migrations
│   │   ├── MigrationRunner.cs
│   │   └── Migrations/
│   │       ├── M0001_InitialSchema.cs
│   │       └── M0002_SeedData.cs
│   │
│   ├── FinanceTracker.Dapper/          # Dapper console app
│   │   ├── Program.cs
│   │   ├── Data/                       # Connection factory, config
│   │   ├── Repositories/               # Repository implementations
│   │   ├── Models/                     # DTOs for queries
│   │   └── Menu/                       # Console menu handlers
│   │
│   ├── FinanceTracker.EFCore/          # EF Core console app
│   │   ├── Program.cs
│   │   ├── Data/                       # DbContext
│   │   ├── Services/                   # Service layer
│   │   ├── Models/                     # DTOs for queries
│   │   └── Menu/                       # Console menu handlers
│   │
│   ├── FinanceTracker.AdoNet/          # Raw ADO.NET console app
│   │   ├── Program.cs
│   │   ├── Data/                       # Connection factory, extensions
│   │   ├── Repositories/               # Repository implementations
│   │   ├── Models/                     # DTOs for queries
│   │   └── Menu/                       # Console menu handlers
│   │
│   └── FinanceTracker.Tests/           # Integration tests
│       ├── Infrastructure/             # Test fixtures
│       └── Dapper/                     # Repository tests
│
└── context/
    ├── context.md                      # Original requirements
    └── progress.md                     # Build progress tracker
```

## Getting Started

### 1. PostgreSQL Setup

Ensure PostgreSQL is running and create a database:

```sql
-- Connect to PostgreSQL as admin
psql -U postgres

-- Create the database
CREATE DATABASE finance_tracker;

-- (Optional) Create a dedicated user
CREATE USER finance_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE finance_tracker TO finance_user;

-- Connect to the new database and grant schema permissions
\c finance_tracker
GRANT ALL ON SCHEMA public TO finance_user;
```

### 2. Clone and Build

```bash
git clone <repository-url>
cd .NET-DB-playground

# Restore and build
dotnet restore
dotnet build
```

### 3. Configure Connection String

Each console app has an `appsettings.json` file. Update the connection string:

**src/FinanceTracker.Dapper/appsettings.json** (and similar for EFCore and AdoNet):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=finance_tracker;Username=postgres;Password=your_password"
  }
}
```

For development, you can also create `appsettings.Development.json` (gitignored) with your local credentials.

### 4. Run Database Migrations

The migrations are run automatically when any console app starts, but you can also run them manually:

```bash
# Using the Dapper app (migrations run on startup)
cd src/FinanceTracker.Dapper
dotnet run

# Or using EF Core app
cd src/FinanceTracker.EFCore
dotnet run
```

The migrations will:
1. Create all tables (users, accounts, categories, transactions, budgets)
2. Set up foreign keys with appropriate cascade rules
3. Create indexes for performance
4. Seed sample data (2 users, 15 categories, 7 accounts, 40+ transactions, 13 budgets)

### 5. Run the Console Apps

Each app provides the same menu-driven interface:

```bash
# Run Dapper implementation
cd src/FinanceTracker.Dapper
dotnet run

# Run EF Core implementation
cd src/FinanceTracker.EFCore
dotnet run

# Run ADO.NET implementation
cd src/FinanceTracker.AdoNet
dotnet run
```

**Main Menu Options:**
1. Manage Users
2. Manage Categories
3. Manage Accounts
4. Manage Transactions
5. Manage Budgets
6. Reports & Analytics
7. Test Database Connection
8. Exit

## Running Tests

The integration tests use Testcontainers to spin up a PostgreSQL container automatically. **Docker must be running.**

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~UserRepositoryTests"
```

## Database Schema

```
┌─────────────┐       ┌─────────────┐
│   users     │       │ categories  │
├─────────────┤       ├─────────────┤
│ id (PK)     │       │ id (PK)     │
│ email (UQ)  │       │ name        │
│ name        │       │ type        │
│ created_at  │       │ icon        │
└──────┬──────┘       │ color       │
       │              └──────┬──────┘
       │                     │
       │    ┌────────────────┼────────────────┐
       │    │                │                │
       ▼    ▼                ▼                ▼
┌─────────────┐       ┌─────────────┐  ┌─────────────┐
│  accounts   │       │transactions │  │   budgets   │
├─────────────┤       ├─────────────┤  ├─────────────┤
│ id (PK)     │       │ id (PK)     │  │ id (PK)     │
│ user_id(FK) │◄──────│ account_id  │  │ user_id(FK) │
│ name        │       │ category_id │  │ category_id │
│ type        │       │ amount      │  │ amount      │
│ balance     │       │ description │  │ period      │
│ currency    │       │ txn_date    │  │ start_date  │
│ created_at  │       │ created_at  │  └─────────────┘
└─────────────┘       └─────────────┘
```

**Key Constraints:**
- `users.email` - Unique constraint
- `categories(name, type)` - Unique constraint
- `accounts.user_id` - FK with CASCADE DELETE
- `transactions.account_id` - FK with CASCADE DELETE
- `transactions.category_id` - FK with NO ACTION
- `budgets.user_id` - FK with CASCADE DELETE
- `budgets.category_id` - FK with NO ACTION

## Framework Comparison

### Dapper

**Pros:**
- Minimal overhead, closest to raw SQL performance
- Full control over SQL queries
- Easy to optimize specific queries
- Small learning curve if you know SQL

**Cons:**
- Manual object mapping (mitigated with `MatchNamesWithUnderscores`)
- No change tracking
- More boilerplate for complex operations

**Best for:**
- Performance-critical applications
- Teams with strong SQL skills
- Read-heavy workloads
- Microservices with simple data access patterns

**Example:**
```csharp
const string sql = "SELECT id, email, name, created_at FROM users WHERE id = @Id";
return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
```

### Entity Framework Core

**Pros:**
- LINQ queries with compile-time checking
- Automatic change tracking
- Navigation properties for relationships
- Migrations support (alternative to FluentMigrator)
- Rich ecosystem

**Cons:**
- Steeper learning curve
- Can generate inefficient queries if not careful
- More memory overhead
- "Magic" can hide what's happening

**Best for:**
- Rapid application development
- Complex domain models with many relationships
- Teams less familiar with SQL
- Applications where development speed > raw performance

**Example:**
```csharp
return await _context.Users
    .Where(u => u.Id == id)
    .FirstOrDefaultAsync();
```

### Raw ADO.NET

**Pros:**
- Maximum performance (no ORM overhead)
- Complete control over every aspect
- Minimal dependencies
- Best for understanding database fundamentals

**Cons:**
- Significant boilerplate code
- Manual parameter binding
- Manual result mapping
- Error-prone, no compile-time safety
- Higher maintenance burden

**Best for:**
- Legacy system maintenance
- Extreme performance requirements
- Learning database fundamentals
- Very simple CRUD applications

**Example:**
```csharp
await using var command = new NpgsqlCommand(
    "SELECT id, email, name, created_at FROM users WHERE id = @Id", connection);
command.Parameters.AddWithValue("@Id", id);
await using var reader = await command.ExecuteReaderAsync();
```

### Performance Comparison (Approximate)

| Operation | Dapper | EF Core | ADO.NET |
|-----------|--------|---------|---------|
| Simple SELECT | ~1x | ~1.2-1.5x | ~1x |
| Bulk INSERT | ~1x | ~2-3x | ~1x |
| Complex JOIN | ~1x | ~1.5-2x* | ~1x |
| Memory Usage | Low | Higher | Lowest |

*EF Core can match Dapper with proper optimization (AsNoTracking, compiled queries)

### Recommendation

- **Start with EF Core** for most applications - productivity benefits outweigh performance costs
- **Use Dapper** for specific performance-critical queries or read-heavy services
- **Use ADO.NET** only when you need absolute control or are maintaining legacy code

## Query Examples

### Monthly Spending by Category (Dapper)

```csharp
const string sql = @"
    SELECT
        c.id AS CategoryId,
        c.name AS CategoryName,
        CASE c.type WHEN 1 THEN 'Expense' WHEN 2 THEN 'Income' END AS CategoryType,
        COALESCE(SUM(ABS(t.amount)), 0) AS TotalAmount,
        COUNT(t.id) AS TransactionCount
    FROM categories c
    LEFT JOIN transactions t ON t.category_id = c.id
        AND EXTRACT(YEAR FROM t.transaction_date) = @Year
        AND EXTRACT(MONTH FROM t.transaction_date) = @Month
    LEFT JOIN accounts a ON t.account_id = a.id
    WHERE (@UserId IS NULL OR a.user_id = @UserId)
    GROUP BY c.id, c.name, c.type
    HAVING COUNT(t.id) > 0
    ORDER BY TotalAmount DESC";
```

### Budget Status with Correlated Subquery (EF Core)

```csharp
var budgets = await _context.Budgets
    .Include(b => b.User)
    .Include(b => b.Category)
    .Where(b => b.Period == BudgetPeriod.Monthly)
    .Select(b => new BudgetStatus
    {
        BudgetId = b.Id,
        UserName = b.User.Name,
        CategoryName = b.Category.Name,
        BudgetAmount = b.Amount,
        SpentAmount = _context.Transactions
            .Where(t => t.Category.Id == b.CategoryId
                && t.Account.UserId == b.UserId
                && t.Amount < 0
                && t.TransactionDate.Year == year
                && t.TransactionDate.Month == month)
            .Sum(t => Math.Abs(t.Amount))
    })
    .ToListAsync();
```

### Income vs Expenses Summary (ADO.NET)

```csharp
const string sql = @"
    SELECT
        COALESCE(SUM(CASE WHEN t.amount > 0 THEN t.amount ELSE 0 END), 0) AS TotalIncome,
        COALESCE(SUM(CASE WHEN t.amount < 0 THEN ABS(t.amount) ELSE 0 END), 0) AS TotalExpenses,
        COALESCE(SUM(t.amount), 0) AS NetAmount
    FROM transactions t
    JOIN accounts a ON t.account_id = a.id
    WHERE t.transaction_date >= @StartDate
      AND t.transaction_date <= @EndDate
      AND (@UserId IS NULL OR a.user_id = @UserId)";

await using var command = new NpgsqlCommand(sql, connection);
command.Parameters.AddWithValue("@StartDate", startDate);
command.Parameters.AddWithValue("@EndDate", endDate);
command.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);
```

## License

This project is for educational purposes. Feel free to use it as a learning resource.
