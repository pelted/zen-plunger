# Zen Plunger

## Overview

This project aims to build a modern, streamlined launcher and frontend for a digital pinball cabinet, focused on Zen Studios Pinball FX (Steam).

The goal is to replace complex and brittle solutions like PinUP Popper with a clean, reliable, and performant system that:

- Works naturally with a physical pinball cabinet
- Minimizes setup complexity
- Keeps Pinball FX running where possible
- Integrates cleanly with DMD and DOF systems
- Provides a fast, responsive overlay-based UI

---

## Getting Started

To set up the development environment on a fresh Windows machine, run the included bootstrap script from an **elevated PowerShell window**:

```powershell
.\bootstrap-dev.ps1
```

This will install all required tools via `winget` and create the expected folder structure. See the [Development Environment](#development-environment) section for the full list of what gets installed.

> **Note:** After the script completes, restart Windows before running `dotnet --info` or `git --version` to confirm the tools are on your PATH.

---

## Reference Material

- Pinball FX Cabinet Mode Documentation  
  https://www.pinballfx.com/?page_id=7754

- DMD Extensions (freezy) – Pinball FX support  
  https://github.com/freezy/dmd-extensions#pinball-fx

- DOFLinx Documentation  
  https://doflinx.github.io/docs/

- Existing solution (NOT used, for comparison only):  
  https://www.nailbuster.com/wikipinup/doku.php?id=start

---

## High-Level Goals

### 1. Replace PinUP Popper
- Avoid complex configuration
- Eliminate fragile scripting and edge cases
- Provide a purpose-built experience for Pinball FX

### 2. Cabinet-Friendly UX
- Fullscreen overlay UI
- Triggered by cabinet button
- Navigable via flipper/buttons (not mouse/keyboard)

### 3. Fast Table Launching
- Use official Pinball FX command-line launching
- Explore keeping FX running in background for faster switching

### 4. Integrated System Management
- Detect and configure:
  - Pinball FX cabinet mode settings
  - DMD Extensions (freezy)
  - DOFLinx
- Provide guided setup and validation

---

## Architecture Overview

    Zen Pinball Launcher
    ├─ Tray Host (always running)
    ├─ Overlay UI (fullscreen, transparent)
    ├─ Settings UI (desktop/admin)
    ├─ Pinball FX Process Manager
    ├─ Input Hook System (cabinet buttons)
    ├─ Display/Layout Manager
    ├─ DMD Extensions Manager
    ├─ DOFLinx Manager
    ├─ Table Metadata Database
    └─ Logging / Diagnostics

---

## Technology Stack

### Core
- Language: C#
- Framework: .NET 10 (LTS) (or .NET 8 LTS fallback)
- Platform: Windows x64 only

### UI
- WPF (Windows Presentation Foundation)
  - Strong Win32 interop
  - Reliable overlay support
  - Mature ecosystem

### System Integration
- Win32 APIs via P/Invoke
- Process and window management
- Global input hooks (keyboard / raw input)

### Data & Config
- JSON / INI parsing
- Optional SQLite for table metadata

---

## Key Design Decisions

### Tray Application Model

The app will run primarily as a system tray application:

- Always running in background
- Handles:
  - Input listening
  - Process monitoring
  - Overlay triggering

### Overlay-Based Launcher

- Fullscreen transparent overlay
- Appears on demand via cabinet button
- Displays table selection UI
- Operates independently of Pinball FX UI

---

## Pinball FX Integration Strategy

### Supported Mode (Primary)

Launch tables directly using command-line:

    steam.exe -applaunch 2328760 -Table <id> -GameMode <mode>

This is the most stable and officially supported method.

### Experimental Mode (Future)

- Keep Pinball FX running
- Use overlay to select tables
- Attempt in-app navigation via:
  - Input automation
  - Window focus control

Fallback to full relaunch if needed.

---

## External System Integration

### DMD Extensions (freezy)

- Detect DmdDevice64.dll
- Validate configuration
- Assist with setup
- Ensure compatibility with Pinball FX cabinet mode

### DOFLinx

- Detect installation
- Validate:
  - DOFLinx.INI
  - DOFLinxTrigger.dll
- Configure:
  - PATH_FX
  - PATH_FX_B2S
- Support per-table customization (future)

---

## Development Environment

### Required Tools

Installed via PowerShell bootstrap script:

- Visual Studio 2022 (Desktop workload)
- .NET SDK (10 or 8)
- Git
- PowerShell 7
- Windows Terminal
- VS Code
- Sysinternals
- OpenAI Codex app (Windows)

### Directory Layout

    C:\Dev\ZenPinballLauncher\
    C:\Pinball\
    C:\Pinball\DOFLinx\
    C:\Pinball\DMDext\
    C:\Pinball\Logs\

---

## Project Phases

### Phase 1 — MVP Launcher

- Tray app
- Basic overlay UI
- Table selection
- Launch Pinball FX via Steam
- Process monitoring

### Phase 2 — Cabinet Setup Assistant

- Detect display configuration
- Configure:
  - Playfield
  - Backglass
  - DMD
- Edit Pinball FX Settings.ini
- Validate cabinet mode setup

### Phase 3 — System Integration

- DMD Extensions detection/config
- DOFLinx integration
- Config validation tools
- Diagnostics UI

### Phase 4 — Advanced Features

- Fast table switching (experimental)
- Input automation
- Per-table configuration
- Enhanced UI/UX

---

## Non-Goals (for now)

- Supporting multiple emulators (focus is Pinball FX first)
- Replacing all pinball ecosystem tools
- Over-abstracting configuration

---

## Guiding Principles

- Reliability over cleverness
- Simple setup
- Cabinet-first UX
- Use official integrations whenever possible
- Fail gracefully (fallback to safe behavior)

---

## Next Steps

1. Create WPF project (x64, .NET 10 or 8)
2. Implement tray app skeleton
3. Build basic overlay window
4. Hook input to toggle overlay
5. Launch Pinball FX table from UI

---

This document will evolve as the project progresses.
