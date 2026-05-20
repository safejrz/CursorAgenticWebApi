# GenAI workflow (assignment section)

This document satisfies the “Generative AI tools” portion of [Requirements.md](../Requirements.md): prompt, representative output, and how suggestions were validated and corrected.

## 1. Prompt used (example)

> You are helping me build a **.NET 8 / net10.0** solution for a small **task management** API and SPA. Constraints:
>
> - **Clean Architecture**: `Domain`, `Application`, `Infrastructure`, and two hosts: `Identity.Api` (register, login, JWT issue, profile) and `Tasks.Api` (CRUD for tasks scoped to the authenticated user).
> - **Storage**: SQLite with **ADO.NET / Microsoft.Data.Sqlite** only — **no Entity Framework, Dapper, or MediatR**.
> - **Tasks**: entity with `Title`, `Description`, `WorkTaskStatus` (Pending, InProgress, Done, Cancelled), `DueDateUtc`, audit timestamps; belongs to `User`.
> - **Auth**: password hashing, JWT bearer on Tasks API; Identity issues tokens consistent with Tasks validation.
> - **Tests**: xUnit tests for application services, repositories, and WebApplicationFactory integration tests for both APIs.
> - **Frontend**: React + TypeScript + Vite calling both APIs; store JWT in `localStorage`; responsive layout for login/register and task CRUD.
>
> Scaffold projects, SQL initialization, DI registration, CORS for `http://localhost:5173`, and seeded demo user `demo@example.com` / `DemoPass1!` with a few sample tasks.

## 2. Representative AI output (abridged)

Typical generators return a **single** `Program.cs` with DbContext or minimal APIs inlined, a flat folder layout, and sometimes **one** API host. They often propose **EF Core** or **Dapper** despite exclusions, or a **single** SQLite file per service without explaining shared state for demo.

## 3. Validation and corrections

| Area | What we checked | Correction |
|------|-----------------|------------|
| **Banned stack** | Search packages and code for EF/Dapper/MediatR | Removed any stray references; kept raw SQL + `SqliteConnection`. |
| **Two APIs + JWT** | Identity must mint tokens Tasks accepts | Aligned `Jwt:Issuer`, audience, signing key in configuration; integration tests build a matching test token for Tasks. |
| **Naming / compile** | `TaskStatus` enum collided with `System.Threading.Tasks.TaskStatus` under implicit usings | Renamed domain enum to **`WorkTaskStatus`** and updated DTOs, SQL mapping, and tests. |
| **Project wiring** | `dotnet build` / `dotnet test` | Restored **`ProjectReference`** entries on API projects; added missing **`using`** for infrastructure types; made **`Program`** public for `WebApplicationFactory<Program>` accessibility. |
| **Seeding** | Demo must work outside pure luck | Seeding runs in **Development** or when **`Demo:SeedIfEmpty`** is true; documented in README. |
| **Security** | Dev signing key in repo | Acceptable for assignment/demo; README calls out changing keys for real deployment. |

## 4. Edge cases and validation rules

- **Tasks:** empty title rejected in application service; user cannot read or mutate another user’s task (repository scoped by `UserId`).
- **Auth:** duplicate email on register; bad password on login; expired or malformed JWT returns 401 on Tasks routes.
- **Data:** `PRAGMA foreign_keys = ON` when opening SQLite connections; schema created via bootstrap script on startup.

## 5. What we kept from the model vs rewrote

- **Kept:** overall layering, endpoint shapes, JWT + bearer pattern, React flow (login → list → form).
- **Rewrote:** anything that violated EF/Dapper bans, enum naming collisions, `.csproj` / `InternalsVisibleTo` mistakes, and seeding conditions after integration test failures.

This file is meant to accompany slides or oral explanation during the presentation.
