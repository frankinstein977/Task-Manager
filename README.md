# TodoTasks — Daily Technical Task Tracker

A sample **ASP.NET Core 8 Web API** built with **Clean Architecture**, **PostgreSQL** (via EF Core + Npgsql), the **Repository pattern**, and **Unit of Work**, with **xUnit** unit tests written in the **AAA (Arrange–Act–Assert)** pattern.

The business case: an engineering team tracks its day-to-day technical tasks (fixes, deployments, reviews, migrations). Each task has a title, description, priority, scheduled date, assignee, and a lifecycle status with enforced state-transition rules (e.g., a completed or cancelled task can no longer be edited).

---

## Solution Structure

```
TodoTasks.sln
├── src/
│   ├── TodoTasks.Domain/            # Enterprise business rules (no dependencies)
│   │   ├── Common/BaseEntity.cs
│   │   ├── Entities/TodoTask.cs     # Rich domain entity with encapsulated invariants
│   │   ├── Enums/                   # TodoTaskStatus, TaskPriority
│   │   └── Interfaces/              # IRepository<T>, ITodoTaskRepository, IUnitOfWork
│   ├── TodoTasks.Application/       # Application business rules (depends on Domain only)
│   │   ├── DTOs/                    # Request/response records
│   │   ├── Exceptions/              # NotFoundException, BusinessRuleViolationException
│   │   ├── Mappings/                # Entity → DTO mapping helpers
│   │   ├── Services/                # ITodoTaskService / TodoTaskService (use cases)
│   │   └── DependencyInjection.cs
│   ├── TodoTasks.Infrastructure/    # External concerns (EF Core, PostgreSQL)
│   │   ├── Persistence/             # DbContext, Fluent configurations, Migrations
│   │   ├── Repositories/            # Repository<T>, TodoTaskRepository, UnitOfWork
│   │   └── DependencyInjection.cs
│   └── TodoTasks.Api/               # Presentation / composition root
│       ├── Controllers/TodoTasksController.cs
│       ├── Middleware/ExceptionHandlingMiddleware.cs   # RFC 7807 problem details
│       └── Program.cs
├── tests/
│   └── TodoTasks.UnitTests/         # xUnit + Moq + FluentAssertions (+ EF InMemory)
│       ├── Domain/TodoTaskTests.cs
│       ├── Services/TodoTaskServiceTests.cs
│       └── Repositories/TodoTaskRepositoryTests.cs
└── docker-compose.yml               # PostgreSQL 16 for local development
```

## Architecture

Classic Clean Architecture (onion) with the **Dependency Rule**: dependencies point inward only.

```
┌─────────────────────────────────────────────────────┐
│  TodoTasks.Api (Controllers, Middleware, DI root)   │
├─────────────────────────────────────────────────────┤
│  TodoTasks.Infrastructure (EF Core, Npgsql, UoW)    │
├─────────────────────────────────────────────────────┤
│  TodoTasks.Application (Use cases, DTOs, exceptions)│
├─────────────────────────────────────────────────────┤
│  TodoTasks.Domain (Entities, enums, interfaces)     │
└─────────────────────────────────────────────────────┘
```

- **Domain** defines the abstractions (`IRepository<T>`, `ITodoTaskRepository`, `IUnitOfWork`) and a rich `TodoTask` entity that protects its own invariants (title validation, status-transition rules, overdue logic).
- **Application** implements the use cases in `TodoTaskService`. It depends only on the domain abstractions, so it is fully unit-testable with mocks.
- **Infrastructure** implements the abstractions with EF Core 8 + Npgsql. `UnitOfWork` shares a single `TodoTasksDbContext` across repositories, so every use case commits atomically via `SaveChangesAsync`.
- **Api** is the composition root: layer registration (`AddApplication()`, `AddInfrastructure()`), JSON enum serialization, Swagger, and a global exception-handling middleware returning RFC 7807 problem details.

### Design decisions

| Concern | Decision |
|---|---|
| Repository pattern | Generic `IRepository<T>` / `Repository<T>` base + task-specific `ITodoTaskRepository` for domain queries |
| Unit of Work | `IUnitOfWork` exposes repositories and one atomic `SaveChangesAsync`; registered `Scoped` |
| Domain entity | Private setters + behavior methods (`UpdateDetails`, `ChangeStatus`, `IsOverdue`); invalid transitions throw |
| Error handling | Domain `InvalidOperationException` → `BusinessRuleViolationException`; middleware maps exceptions → HTTP 400/404/500 with structured logging |
| Persistence | Fluent API configuration, snake_case columns, enum-to-string conversion, indexes on `scheduled_date` and `status` |
| Migrations | `InitialCreate` included; applied automatically at startup (`Database.Migrate()`) |
| Testing | AAA pattern; service layer tested with Moq mocks (Strict), repository queries tested against EF Core InMemory, domain rules tested directly |

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker (for PostgreSQL) or an existing PostgreSQL 14+ instance

### 1. Start PostgreSQL

```bash
docker compose up -d
```

This starts PostgreSQL 16 on `localhost:5432` with database `todo_tasks` (user/password: `postgres`/`postgres`). To use a different instance, edit `ConnectionStrings:TodoTasksDatabase` in `src/TodoTasks.Api/appsettings.json`.

### 2. Run the API

```bash
cd src/TodoTasks.Api
dotnet run
```

Migrations are applied automatically on startup. Swagger UI is available at `http://localhost:5000/swagger` (development environment).

### 3. Run the tests

```bash
dotnet test
```

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/todotasks` | List tasks; optional `?scheduledDate=YYYY-MM-DD` and `?status=` filters |
| `GET` | `/api/todotasks/overdue` | List overdue, unfinished tasks |
| `GET` | `/api/todotasks/{id}` | Get a single task |
| `POST` | `/api/todotasks` | Create a task |
| `PUT` | `/api/todotasks/{id}` | Update a task |
| `PATCH` | `/api/todotasks/{id}/status` | Transition status (`Pending`, `InProgress`, `Completed`, `Cancelled`) |
| `DELETE` | `/api/todotasks/{id}` | Delete a task |

### Example

```bash
curl -X POST http://localhost:5000/api/todotasks \
  -H "Content-Type: application/json" \
  -d '{
        "title": "Patch production server",
        "description": "Apply security updates and reboot",
        "priority": "High",
        "scheduledDate": "2026-08-13",
        "assignee": "alice"
      }'
```

## Adding New Migrations

The `dotnet-ef` tool is pinned in the local tool manifest:

```bash
dotnet tool restore
dotnet ef migrations add <MigrationName> \
  --project src/TodoTasks.Infrastructure \
  --startup-project src/TodoTasks.Api \
  --output-dir Persistence/Migrations
```

## Test Coverage (22 tests)

- **Domain (8)**: constructor validation, title length, status transitions, immutability after completion/cancellation, overdue logic.
- **Service (10)**: create/read/update/delete/status-change orchestration, `NotFoundException` on missing tasks, `BusinessRuleViolationException` on illegal transitions, and verification that the Unit of Work commits exactly once per use case.
- **Repository (4)**: date/status/overdue query filters and an add-and-read round-trip through `UnitOfWork` (EF Core InMemory provider).
