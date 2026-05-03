# Architecture

Zen Plunger is a Windows x64 cabinet launcher for Pinball FX. The application is organized around a small platform-neutral core and Windows-specific integration layers.

## Goals

1. Replace brittle cabinet launcher workflows with a focused Pinball FX experience.
2. Keep the primary user experience cabinet-friendly and button-driven.
3. Launch tables through the official Pinball FX Steam command line first.
4. Detect, validate, and eventually configure common cabinet dependencies.
5. Keep failure modes visible and recoverable.

## Non-Goals

- Supporting multiple emulators at the start
- Replacing all pinball ecosystem tools
- Building a generic frontend framework
- Depending on fragile UI automation for the MVP launch path

## Solution Layout

```text
src/
  ZenPlunger.App/
  ZenPlunger.Core/
  ZenPlunger.Platform.Windows/
```

### ZenPlunger.App

The WPF shell application. This project owns visible UI, app startup, tray-host behavior, and user interaction.

Initial responsibilities:

- Show a basic table selection UI
- Trigger launch requests
- Host future overlay and settings screens
- Compose core contracts with Windows implementations

### ZenPlunger.Core

Platform-neutral contracts and models. This project should avoid direct Windows, WPF, Steam, registry, filesystem-layout, or process-management assumptions unless represented as abstractions.

Current responsibilities:

- Table metadata models
- Versioned table catalog document model
- Launch request model
- Launcher interface
- Overlay controller interface
- Table catalog interface
- Table catalog store/import interfaces

### ZenPlunger.Platform.Windows

Windows-specific integration. This project owns process launching, Win32 interop, Steam integration, display inspection, file paths, registry access, hooks, DMD detection, and DOFLinx detection.

Current responsibilities:

- Build a Steam Pinball FX launch command
- Start the launch process through `steam.exe`
- Load, save, and import table catalog data from JSON files

## Runtime Components

```text
Tray Host
Overlay UI
Settings UI
Pinball FX Process Manager
Input Hook System
Display/Layout Manager
DMD Extensions Manager
DOFLinx Manager
Table Metadata Catalog
Logging and Diagnostics
```

## Pinball FX Launching

The MVP launch path is the supported Steam command line:

```text
steam.exe -applaunch 2328760 -Table <id> -GameMode <mode>
```

The future experimental path may keep Pinball FX running and use controlled focus/input behavior for faster switching. That path should remain a fallback or optional mode until it proves reliable.

## Cabinet Integration Areas

### Cabinet Mode

Zen Plunger should detect Pinball FX cabinet mode settings and eventually guide users through display, backglass, and DMD configuration.

### DMD Extensions

Zen Plunger should detect `DmdDevice64.dll`, validate configuration, and surface compatibility issues with Pinball FX cabinet mode.

### DOFLinx

Zen Plunger should detect a DOFLinx install, inspect `DOFLinx.INI`, validate related paths, and later assist with table-specific configuration.

## Table Catalog Storage

The first datastore is a versioned JSON document. JSON keeps early iteration simple and makes it easy to hand-author or import table metadata files.

The catalog shape is intentionally close to a future SQLite model:

- Table identity and display fields
- Optional collection/grouping
- Backglass asset path
- DMD asset path
- Per-asset screen placement

Future SQLite tables can map from this structure without changing the app-level catalog interfaces.

## Design Bias

- Keep core contracts small and testable.
- Put Windows details behind interfaces.
- Prefer explicit diagnostics over silent fallback.
- Make command construction testable without launching external processes.
- Keep the WPF shell thin until the core workflows are clear.
