# CLAUDE.md

## Project Overview

Type-safe database access library built on Dapper, supporting SQL Server and PostgreSQL. Includes a code generator (DtoHelper) that reads database schema and produces DTO classes with SQL strings.

## Tech Stack

- **Language:** C# 12 / .NET 10.0
- **ORM:** Dapper (micro-ORM)
- **Databases:** SQL Server 2019, PostgreSQL 17
- **Migrations:** DbUp
- **Code Generation:** Roslyn (Microsoft.CodeAnalysis)
- **Testing:** xUnit, Docker Compose
- **Logging:** Serilog

## Architecture

```
src/
├── affolterNET.Data/              # Core library (NuGet package)
│   ├── Commands/                  # Built-in commands (SaveEntity, DeleteEntity)
│   ├── Queries/                   # Built-in queries (LoadEntity)
│   ├── Models/Filters/            # Query filter system (Text, Number, Date filters)
│   ├── Extensions/                # Column quoting, type extensions
│   ├── Interfaces/                # IDtoBase, IQuery<T>, ICommand, ISqlSessionHandler
│   ├── SessionHandler/            # Connection/transaction lifecycle, history logging
│   └── Result/                    # DataResult<T> wrapper
├── affolterNET.Data.DtoHelper/    # Code generator (NuGet tool)
│   ├── CodeGen/                   # Roslyn-based generators (Select, Insert, Update, Delete, SaveById)
│   ├── Database/                  # Schema readers (SqlServer, PostgreSQL), Table/Column models
│   └── Dialect/                   # ISqlDialect implementations (quoting, type mapping, SQL formatting)
├── affolterNET.Data.TestHelpers/  # Test infrastructure (NuGet package)
│   ├── Builders/                  # SelectBuilder, UpdateBuilder, SoftDeleteBuilder
│   └── SessionHandler/            # TestSqlSessionHandler (no-commit wrapper)
├── affolterNET.Data.DbUp/        # Migration runner (NuGet package)
├── Example*/                      # 6 SQL Server example projects
└── ExamplePg*/                    # 6 PostgreSQL example projects
```

## Key Patterns

- **Command/Query separation:** Write operations implement `ICommand`, reads implement `IQuery<T>`. Both extend `CommandQueryBase<T>`.
- **ISqlDialect:** Abstracts SQL Server vs PostgreSQL differences (quoting, type mapping, upsert syntax).
- **Column name mapping:** PostgreSQL uses `"col_name"|PropertyName` pipe-delimited format to map snake_case DB columns to PascalCase C# properties.
- **QuoteStyle:** `Brackets` for SQL Server `[col]`, `DoubleQuotes` for PostgreSQL `"col"`.
- **SaveById:** Atomic upsert — SQL Server uses IF EXISTS, PostgreSQL uses CTE with ON CONFLICT.

## Development Commands

```bash
# Start databases (Docker), run migrations, generate DTOs
cd db && bash start.sh

# Run all tests (requires running databases)
dotnet test src/affolterNET.Data.sln

# Build only
dotnet build src/affolterNET.Data.sln
```

## Database Setup

Docker Compose in `db/` starts:
- SQL Server on port **1436** (sa / Som3V3ryS3cretP4ssw0rd!)
- PostgreSQL on port **5436** (postgres / Som3V3ryS3cretP4ssw0rd!)

Connection strings are stored in user secrets (ID: `83694dd8-458d-4674-af47-af19a35a4527`):
- `CONNSTRING` — SQL Server
- `CONNSTRING_PG` — PostgreSQL

## Coding Conventions

- **Private fields:** `_camelCase` with underscore prefix
- **Parameters:** PascalCase for public, camelCase for local
- **Naming:** C# standard (PascalCase classes/methods, _camelCase fields)
- **Nullable reference types:** Enabled
- **File-scoped namespaces:** Used in newer files, block-scoped in older ones

## Example Projects

Each example demonstrates a feature combination (basic, history, user/date audit, version/optimistic concurrency). SQL Server and PostgreSQL variants exist for each. Structure per example:
- `*.Update/` — DbUp migration scripts + DtoHelper generator config
- `*.Data/` — Generated `Dto.cs`
- `*.IntegrationTest/` — xUnit integration tests
