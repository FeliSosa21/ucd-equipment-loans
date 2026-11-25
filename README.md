# UCD — University Equipment Loan System

Web application to manage equipment loans at a university: students and staff browse the catalog and request items, and administrators approve loans, register returns, manage inventory and users, review an audit log and export reports to Excel.

Built as the semester project for **Diseño de Aplicaciones 1** (Application Design 1) at Universidad ORT Uruguay, 2026, by a team of three students.

## Features

- **Accounts and roles:** sign-up, sign-in, account activation and six user types with their own permissions and loan limits.
- **Catalog and inventory:** equipment types, catalogs and individual physical units, each with its own status.
- **Loans:** request, approve or reject, active loans and returns.
- **Audit log:** records who did what and when, with filters by date and user.
- **Excel export** of system data with ClosedXML.

## Tech stack

- **C# / .NET 9**
- **Blazor Server** for the UI
- **Entity Framework Core 9** with **SQL Server** (Azure SQL Edge in Docker) and code-first migrations
- **MSTest**: more than 1,100 unit and integration tests; EF InMemory for persistence tests

## Architecture

Layered solution in which each layer only depends on the layers below it:

```
UI (Blazor Server)  →  Servicios (use cases)  →  Dominio (entities and business rules)
        ↓                     ↓
       DTOs                  Data (EF Core: DbContext, configurations, repositories, migrations)
```

| Project | Responsibility |
|---|---|
| `Dominio` | Entities and business rules; throws `DominioException`. |
| `Data` | Persistence with EF Core: `SqlContext`, entity configurations, repositories and migrations. |
| `DTOs` | Data passed between services and the UI. |
| `Servicios` | Use cases; authorization, translating domain errors and returning DTOs. |
| `UI` | Blazor Server pages; no business logic and no knowledge of the domain. |

Every project has its own test project (`*.Tests`).

## Practices

- **TDD**: red, green and refactor, with one commit per step.
- **Git flow** with feature branches and pull requests reviewed by another team member.
- **GRASP and SOLID**: each rule lives in the class that owns the data.
- Time is injected with `TimeProvider`, so tests are deterministic.

## My contribution

- User type management: rules, presentation data and loan limits.
- Audit log: domain, service, filters and UI.
- User repository with Entity Framework, built by hand with TDD: `RepositorioUsuarioSql`, `UsuarioConfiguracion` and their tests.

## Running it locally

Requirements: [.NET 9 SDK](https://dotnet.microsoft.com/download) and [Docker](https://www.docker.com/).

```bash
# 1. Start SQL Server (from the repository root)
docker compose up -d

# 2. Apply the migrations (from UCD/)
cd UCD
dotnet ef database update --project Data --startup-project UI

# 3. Run the app
dotnet run --project UI

# Run the tests
dotnet test
```

The connection string and the database password are for local development only. Set your own with environment variables or `dotnet user-secrets`.

## Team

- Felipe Sosa
- Máximo Bertini
- Agustín Ormazabal
