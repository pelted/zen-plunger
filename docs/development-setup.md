# Development Setup

This document describes how to prepare a Windows development machine for Zen Plunger.

## Bootstrap Script

Run the included bootstrap script from an elevated PowerShell window:

```powershell
.\bootstrap-dev.ps1
```

The script installs or updates required tools with `winget` and creates the expected local cabinet development folders. It is intended to be idempotent, so it can be rerun as the tool list changes.

After the script completes, restart Windows before checking newly installed command line tools on your PATH.

## Required Tools

Installed by the bootstrap script:

- Visual Studio 2022 Community with the .NET desktop workload
- .NET SDK 10
- Git
- GitHub CLI (`gh`)
- PowerShell 7
- Windows Terminal
- VS Code
- Sysinternals
- OpenAI Codex app for Windows

## Verification

After rebooting, verify the core tools:

```powershell
dotnet --info
git --version
gh --version
gh auth status
```

If GitHub CLI is not authenticated yet, run:

```powershell
gh auth login
gh auth setup-git
```

## Local Folder Layout

The bootstrap script creates these folders:

```text
C:\Dev
C:\Dev\ZenPinballLauncher
C:\Pinball
C:\Pinball\DOFLinx
C:\Pinball\DMDext
C:\Pinball\Logs
```

## Build

From the repository root:

```powershell
dotnet build ZenPlunger.slnx
```

## Run

From the repository root:

```powershell
dotnet run --project src\ZenPlunger.App\ZenPlunger.App.csproj
```

