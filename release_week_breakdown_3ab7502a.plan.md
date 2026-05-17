Breakdown: rest of week until Sunday (today = Tuesday 12 May 2026)

Use this as a work breakdown structure (WBS). Adjust hours to your availability; order matters more than exact days.

Tuesday (today) — Baseline and gaps





Run dotnet test on the solution and npm run build (and npm run lint) in frontend on a clean clone or after git clean of build artifacts — record any failures.



Smoke the golden path: start Identity + Tasks APIs, npm run dev, login with seeded demo user, create/edit/delete a task, logout, register a new user.



Decide demo environment: always Development (simplest) vs seeding in other environments — document the choice in the README.

Wednesday — Documentation and release mechanics





Add a repository-root README: prerequisites (.NET SDK version, Node), how to create data/ (or rely on auto path in Program.cs), order to start APIs, frontend env vars (VITE_IDENTITY_API_URL, VITE_TASKS_API_URL), and demo credentials from DataSeeder.



Optional: one dotnet publish / vite build section for a "release" folder layout you will actually use in the demo.

Thursday — GenAI and story (submission content)





Draft the GenAI section: original prompt, trimmed representative output, bullet list of what you changed and why (auth, validation, edge cases).



Write the one-paragraph user story (persona, goal, acceptance in plain language) matching the task app.

Friday — Hardening





Fix any issues found Tuesday/Wednesday; re-run tests and builds.



If demo is not Development-only, implement or document seeding strategy explicitly (small code change vs "always run with Development" in instructions).



Quick accessibility/responsive pass on the SPA if time (requirements call for responsive, user-friendly UI).

Saturday — Demo dry run





Full timed rehearsal (10–15 minutes): story → live app → point to architecture (two APIs, layers, tests) → GenAI reflection.



Second run from cold start (no running terminals) to match demo day.

Sunday — Buffer and lock





Final README/deck sync; tag or branch for "submission" if you use version control that way.



Light pass only — no risky refactors. Optional: screenshot or short screen recording as backup if live demo fails.

Success criteria for Sunday night





README lets a reviewer run stack + demo login without asking you questions.



GenAI + user story artifacts exist where you will submit them.



You have completed at least one cold-start demo run with no surprises (ports, CORS, empty DB).

