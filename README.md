# CursorAgenticWebApi

Full-stack task management sample: **.NET 8** Web APIs (identity + tasks), **SQLite** (no Entity Framework / Dapper / Mediator), **Clean Architecture** layers, and a **React + Vite** SPA. Assignment brief: [Requirements.md](Requirements.md).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (LTS recommended)

## Quick start (local demo)

The APIs default to a shared SQLite file under `data/app.db` (created automatically). **Start processes in this order:**

1. **Identity API** — `http://localhost:5001`
2. **Tasks API** — `http://localhost:5002`
3. **Frontend** — `http://localhost:5173`

### Terminal 1 — Identity API

```powershell
cd src\Identity.Api
dotnet run
```

### Terminal 2 — Tasks API

```powershell
cd src\Tasks.Api
dotnet run
```

### Terminal 3 — Frontend

```powershell
cd frontend
npm ci
npm run dev
```

Optional: set API base URLs if not using defaults (PowerShell):

```powershell
$env:VITE_IDENTITY_API_URL = "http://localhost:5001"
$env:VITE_TASKS_API_URL = "http://localhost:5002"
npm run dev
```

### Demo credentials (seeded)

After the database is created, the demo user is seeded when **Development** is active, or when `Demo:SeedIfEmpty` is `true` (see [Seeding](#seeding)).

| Field    | Value            |
|----------|------------------|
| Email    | `demo@example.com` |
| Password | `DemoPass1!`     |

You can also **register** a new account from the SPA; tasks are scoped per user.

### CORS

Both APIs allow the SPA origin from `Frontend:Origin` (default `http://localhost:5173`). Change it in `src/*/appsettings.json` or via configuration if your Vite port differs.

## Verify the build

```powershell
dotnet test
cd frontend
npm ci
npm run build
npm run lint
```

## Seeding

- **Development:** demo user and sample tasks are inserted if the database has no user with `demo@example.com`.
- **Other environments:** set `Demo:SeedIfEmpty` to `true` in `appsettings.json`, environment variables, or command line so the same seeder runs (useful for a packaged demo without switching to Development).

To **reset** the demo database, stop both APIs and delete `data/app.db`, then start again.

## Optional: publish layout

**Backend (example):**

```powershell
dotnet publish src\Identity.Api\Identity.Api.csproj -c Release -o publish\identity
dotnet publish src\Tasks.Api\Tasks.Api.csproj -c Release -o publish\tasks
```

**Frontend:**

```powershell
cd frontend
npm run build
```

Serve `frontend/dist` with any static host; set `VITE_*` URLs at build time to point to your deployed API origins.

## Documentation for submission / demo

| Document | Purpose |
|----------|---------|
| [docs/USER_STORY.md](docs/USER_STORY.md) | Informal user story for presentations |
| [docs/GENAI_REFLECTION.md](docs/GENAI_REFLECTION.md) | GenAI prompt, sample output, validation notes |
| [docs/DEMO_RUNBOOK.md](docs/DEMO_RUNBOOK.md) | Cold-start checklist and timed demo outline |

## Solution layout

- `src/Domain` — entities and enums  
- `src/Application` — use cases, DTOs, validation  
- `src/Infrastructure` — SQLite access, JWT helpers, seeding  
- `src/Identity.Api` — register, login, profile (`/api/...`)  
- `src/Tasks.Api` — task CRUD (`/api/tasks`)  
- `frontend` — React SPA  
- `tests` — unit and integration tests  
