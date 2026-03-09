# affolterNET.Data

Type-safe database access using Commands and Queries with [Dapper](https://github.com/DapperLib/Dapper). Supports **SQL Server** and **PostgreSQL**.

## Introduction

With this library you can access a SQL database in a type-safe manner using Commands and Queries. These are hand-crafted using all the power of SQL. There are, however, built-in helpers to work with a single table.

The included **DtoHelper** code generator reads your database schema and produces DTO classes with ready-made SQL strings (SELECT, INSERT, UPDATE, DELETE).

## Getting Started

1. **Create a database** using SQL Scripts and [DbUp](https://dbup.readthedocs.io/). Use `affolterNET.Data.DbUp.Services.UpdateService` to run migrations. Add a HistoryMode (all scripts or write-operations only) if needed.

2. **Generate DTOs** from the database using DtoHelper. The generated file goes into its own assembly where you can also put your Commands and Queries later.

3. **Create Commands and Queries** by inheriting from `CommandQueryBase`. Every Command and Query is a class containing SQL and parameters. Generated DTOs contain SQL strings to access their table. Use a folder named `Commands` for write-operations and `Queries` for reads.

4. **Use ISqlSessionHandler** to query the database:

```csharp
// Simple (auto-rollback on failure)
var cmd = new YourCustomCommand(parameter1);
var result = await sqlSessionHandler.QueryAsync(cmd);

// With explicit transaction
var session = sqlSessionHandler.CreateSqlSession();
session.Begin();
try
{
    var cmd = new YourCustomCommand(parameter1);
    var result = await sqlSessionHandler.QueryAsync(cmd);
    session.Commit();
    return result;
}
catch
{
    session.Rollback();
    throw;
}
```

## Database Support

| Feature | SQL Server | PostgreSQL |
|---|---|---|
| DTO Generation | `[Schema].[Table]` bracket quoting | `"schema"."table"` double-quote quoting |
| Naming Convention | PascalCase | snake_case (mapped to PascalCase properties) |
| DbUp Migrations | `DeployChanges.To.SqlDatabase()` | `DeployChanges.To.PostgresqlDatabase()` |
| Optimistic Concurrency | `rowversion` (automatic) | `integer` column + trigger |
| Connection | `Microsoft.Data.SqlClient` | `Npgsql` |

### PostgreSQL Setup

Use `.WithDialect(DatabaseDialect.PostgreSql)` in your `GeneratorCfg`:

```csharp
var cfg = new GeneratorCfg()
    .WithDialect(DatabaseDialect.PostgreSql)
    .WithConn(connectionString)
    .WithTargetFile(targetFile)
    // ... other config
```

For DbUp, pass `--postgresql` (or set `PostgreSql = true` on settings):

```csharp
settings.PostgreSql = true;
await updateService.UpdateDb(context, settings);
```

### PostgreSQL Version Column

PostgreSQL has no `rowversion` type. Use an integer column with a trigger instead:

```sql
CREATE TABLE my_schema.my_table(
    id uuid NOT NULL PRIMARY KEY,
    -- ... other columns
    version_timestamp integer NOT NULL DEFAULT 1
);

CREATE OR REPLACE FUNCTION my_schema.increment_version()
RETURNS trigger AS $$
BEGIN
    NEW.version_timestamp = OLD.version_timestamp + 1;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_my_table_version
    BEFORE UPDATE ON my_schema.my_table
    FOR EACH ROW EXECUTE FUNCTION my_schema.increment_version();
```

## Docker Setup

Start both SQL Server and PostgreSQL with Docker Compose:

```bash
cd db
docker compose up -d --build --wait
```

This starts:
- **SQL Server 2019** on port `1436` (user: `sa`)
- **PostgreSQL 17** on port `5436` (user: `postgres`, database: `example`)

Both use the password `Som3V3ryS3cretP4ssw0rd!`.

To generate all example DTOs:

```bash
cd db
./start.sh
```

## Example Projects

The repository includes example projects demonstrating different feature combinations for both SQL Server and PostgreSQL:

| Feature | SQL Server | PostgreSQL |
|---|---|---|
| Basic (with date columns) | Example | ExamplePg |
| Basic (history-enabled at runtime) | ExampleHistory | ExamplePgHistory |
| Insert/Update metadata | ExampleUserDate | ExamplePgUserDate |
| Optimistic concurrency | ExampleVersion | ExamplePgVersion |
| Version + metadata | ExampleVersionUserDate | ExamplePgVersionUserDate |
| Version + metadata + history | ExampleVersionUserDateHistory | ExamplePgVersionUserDateHistory |

Each example has two sub-projects:
- **`.Data`** — Class library containing the generated `Dto.cs`
- **`.Update`** — Console app with `dbup` and `gen` commands

## Different Modes

### Metadata

Every row can have four metadata columns for tracking who created/modified a record and when. Activate with:

```csharp
// SQL Server (PascalCase column names)
cfg.WithInsertDate(insDate => insDate == "InsertDate");
cfg.WithInsertUser(insUser => insUser == "InsertUser");
cfg.WithUpdateDate(updDate => updDate == "UpdateDate");
cfg.WithUpdateUser(updUser => updUser == "UpdateUser");

// PostgreSQL (snake_case column names)
cfg.WithInsertDate(insDate => insDate == "insert_date");
cfg.WithInsertUser(insUser => insUser == "insert_user");
cfg.WithUpdateDate(updDate => updDate == "update_date");
cfg.WithUpdateUser(updUser => updUser == "update_user");
```

### Optimistic Concurrency (Version)

Prevents overwriting changes when multiple users edit the same record simultaneously.

**SQL Server:** Uses `rowversion` data type (automatic).

**PostgreSQL:** Uses an `integer` column with a `BEFORE UPDATE` trigger.

```csharp
// SQL Server
cfg.WithVersion(version => version == "VersionTimestamp");

// PostgreSQL
cfg.WithVersion(version => version == "version_timestamp");
```

### History Mode

Records SQL commands for replay/audit purposes. Modes:

| Mode | Meaning |
|---|---|
| `EnumHistoryMode.None` | No SQL scripts recorded (default) |
| `EnumHistoryMode.All` | All SQL scripts recorded |
| `EnumHistoryMode.CommandsOnly` | Write-operations only (based on namespace) |
| `EnumHistoryMode.CommandsOnlyAndCheck` | Same as CommandsOnly, with console warnings for suspicious reads |

## GeneratorCfg Reference

| Method | Description |
|---|---|
| `.WithDialect(DatabaseDialect)` | Set database dialect (SqlServer or PostgreSql) |
| `.WithConn(string)` | Connection string |
| `.WithTargetFile(string)` | Output file path for generated DTOs |
| `.WithNamespace(string)` | Namespace for generated classes |
| `.WithSchemaExclusion(string)` | Exclude a schema from generation |
| `.WithTableNameExclusion(string)` | Exclude a specific table |
| `.WithContentsList(...)` | Generate static lookup class from table data |
| `.WithInsertDate(Func)` | Mark insert-date columns |
| `.WithInsertUser(Func)` | Mark insert-user columns |
| `.WithUpdateDate(Func)` | Mark update-date columns |
| `.WithUpdateUser(Func)` | Mark update-user columns |
| `.WithVersion(Func)` | Mark version/concurrency columns |
| `.WithBaseType(string)` | Base interface for DTOs (default: `IDtoBase`) |
| `.WithBaseViewType(string)` | Base interface for views (default: `IViewBase`) |

## Build and Test

```bash
dotnet build src/affolterNET.Data.sln
dotnet test src/affolterNET.Data.sln
```

## Troubleshooting

### Docker containers won't start
- Ensure Docker Desktop is running
- Check if ports 1436 (SQL Server) or 5436 (PostgreSQL) are already in use: `lsof -i :1436` / `lsof -i :5436`
- On Apple Silicon: SQL Server runs under emulation (linux/amd64) — the `platform mismatch` warning is expected and can be ignored

### Integration tests fail with connection errors
- Verify containers are running: `docker compose -f db/docker-compose.yml ps`
- Wait for SQL Server to fully initialize (~15 seconds after container start)
- Check that user secrets are configured with the correct connection strings (see Database Setup in CLAUDE.md)

### DTO generation produces unexpected output
- Ensure the database schema is up to date by running `dbup` before `gen`
- Check `GeneratorCfg` for schema/table exclusions that might filter out expected tables
- Verify the connection string points to the correct database

### Optimistic concurrency (version mismatch) errors
- `ConcurrencyException` is thrown when SaveById detects that the row was modified by another transaction
- Reload the entity from the database and retry the operation
- For PostgreSQL, ensure the `increment_version` trigger is created on versioned tables

### SQL Server on Apple Silicon (ARM)
- SQL Server 2019 runs under Rosetta emulation — performance may be lower than native
- If the container crashes on startup, increase Docker memory allocation to at least 4 GB

Thanks to Wolfgang for the [workaround](https://www.programmingwithwolfgang.com/azure-devops-publish-nuget/) when publishing NuGet packages from Azure DevOps.
