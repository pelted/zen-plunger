# AGENTS.md

Guidance for Codex and other coding agents working in this repository.

## Project Context

Zen Plunger is a Windows x64 WPF application for a digital pinball cabinet focused on Pinball FX through Steam.

Read these files before making architectural changes:

- `README.md`
- `docs/architecture.md`
- `docs/work-queue.md`
- `docs/development-setup.md`

## Build and Verification

Use the repository root as the working directory.

```powershell
dotnet build ZenPlunger.slnx
```

Run the shell app when a UI smoke test is useful:

```powershell
dotnet run --project src\ZenPlunger.App\ZenPlunger.App.csproj
```

Prefer adding focused tests as core behavior grows. Command construction, file parsing, and configuration validation should be testable without launching external tools.

## Architecture Rules

- Keep `ZenPlunger.Core` platform-neutral.
- Put Windows, Steam, registry, process, display, and filesystem integration in `ZenPlunger.Platform.Windows`.
- Keep WPF views and app composition in `ZenPlunger.App`.
- Prefer small interfaces in Core over direct references to WPF, Win32, or external tools.
- Make external command construction testable separately from process execution.
- Keep user-facing diagnostics explicit and actionable.

## Documentation Rules

- Keep `README.md` focused on what the project does and basic usage.
- Put setup details in `docs/development-setup.md`.
- Put project shape and design decisions in `docs/architecture.md`.
- Track planned work in `docs/work-queue.md`.
- Update the work queue when completing or adding meaningful architecture work.

## Current Priorities

Follow `docs/work-queue.md`. The immediate direction is the Phase 1 MVP launcher:

- Tests for Steam launch command construction
- Table catalog abstraction implementation
- App composition cleanup
- Tray host behavior
- Basic fullscreen overlay
- Cabinet button input path

## Style

- Use C# nullable annotations and keep nullable warnings clean.
- Keep comments sparse and useful.
- Prefer clear names over broad abstractions.
- Do not add large dependencies until the need is concrete.
- Avoid unrelated formatting churn.

