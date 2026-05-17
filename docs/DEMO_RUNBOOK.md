# Demo runbook (cold start and rehearsal)

Use this for **Saturday–Sunday** rehearsal and as a backup if the live demo misbehaves.

## Cold start checklist (about 5 minutes)

1. Close all old API and Vite terminals (avoid port conflicts).
2. Optional **clean database:** delete `data/app.db` if you want a fresh seed (both APIs must be stopped).
3. Start **Identity.Api** (`dotnet run` in `src/Identity.Api`) — wait for “Now listening on …:5001”.
4. Start **Tasks.Api** (`dotnet run` in `src/Tasks.Api`) — wait for “…:5002”.
5. Start **frontend** (`npm run dev` in `frontend`) — open the URL Vite prints (default `http://localhost:5173`).
6. If APIs are **not** in Development but you need the demo user, set `Demo:SeedIfEmpty` to `true` in both `appsettings.json` files **or** pass `--environment Development` when running the APIs.

**Smoke:** log in with `demo@example.com` / `DemoPass1!`, open tasks, create one row, edit status, delete it, log out, register a throwaway user, log in again.

## Timed outline (10–15 minutes)

| Minutes | Content |
|--------|---------|
| 0–1 | One-sentence product goal; reference [USER_STORY.md](USER_STORY.md). |
| 1–4 | Live demo: login → list seeded tasks → create → update → delete → register second user (optional). |
| 4–8 | Architecture: two APIs (identity vs tasks), shared SQLite file for local demo, Clean Architecture folders, no EF/Dapper. |
| 8–11 | Tests: `dotnet test` (mention integration vs unit). |
| 11–14 | GenAI: prompt + one correction (enum collision / project references) from [GENAI_REFLECTION.md](GENAI_REFLECTION.md). |
| 14–15 | Q&A buffer. |

## Submission lock (optional git)

When the stack is green and docs are final:

```powershell
git tag -a demo-ready -m "First release / demo candidate"
```

Adjust tag name to match your course requirements.

## Backup if the network or machine fails

- Run through the same steps on a second machine if available.
- Keep a short **screen recording** of the golden path (login + one CRUD cycle) as last resort.
