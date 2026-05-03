# Zen Plunger

Zen Plunger is a modern launcher and frontend for a digital pinball cabinet focused on Zen Studios Pinball FX through Steam.

The project is meant to replace complex, brittle cabinet launcher setups with a focused Windows application that is easier to configure, easier to reason about, and friendlier to physical cabinet controls.

## What It Does

Zen Plunger will provide:

- A cabinet-first launcher UI for selecting Pinball FX tables
- A background tray host that can stay resident while the cabinet is running
- A fullscreen overlay experience triggered by cabinet buttons
- Direct table launching through the supported Pinball FX Steam command line
- Guided detection and setup for Pinball FX cabinet mode, DMD Extensions, and DOFLinx
- Logging and diagnostics for cabinet setup issues

The current application is an early shell. It has a WPF project, core launch contracts, a Windows Steam launcher implementation, and a starter table list in the app window.

## How To Use It

Build the solution:

```powershell
dotnet build ZenPlunger.slnx
```

Run the WPF shell:

```powershell
dotnet run --project src\ZenPlunger.App\ZenPlunger.App.csproj
```

At this stage, the shell can show a small starter table list and send a Steam launch request for the selected table. It is not yet a production cabinet launcher.

## Documentation

- [Development setup](docs/development-setup.md)
- [Architecture](docs/architecture.md)
- [Table catalog](docs/table-catalog.md)
- [Work queue](docs/work-queue.md)

## Repository Structure

```text
ZenPlunger.slnx
Directory.Build.props
bootstrap-dev.ps1
src/
  ZenPlunger.App/                WPF shell application
  ZenPlunger.Core/               Platform-neutral contracts and models
  ZenPlunger.Platform.Windows/   Windows and Steam integration
docs/
  architecture.md
  development-setup.md
  table-catalog.md
  work-queue.md
data/
  tables.sample.json
```

## Guiding Principles

- Reliability over cleverness
- Simple setup
- Cabinet-first UX
- Official integrations first
- Graceful failure with clear diagnostics
