# bootstrap-dev.ps1
# Run from an elevated PowerShell window.

$ErrorActionPreference = "Stop"

function Install-WingetPackage {
    param(
        [string]$Id,
        [string]$Name = $Id,
        [string]$Source = "winget"
    )

    Write-Host "Installing $Name..."
    winget install --id $Id --exact --source $Source `
        --accept-package-agreements `
        --accept-source-agreements
}

# Core tools
Install-WingetPackage "Microsoft.DotNet.SDK.10" ".NET 10 SDK"
Install-WingetPackage "Git.Git" "Git"
Install-WingetPackage "Microsoft.PowerShell" "PowerShell 7"
Install-WingetPackage "Microsoft.WindowsTerminal" "Windows Terminal"
Install-WingetPackage "Microsoft.VisualStudioCode" "VS Code"
Install-WingetPackage "Microsoft.Sysinternals" "Sysinternals"
Install-WingetPackage "Codex" "OpenAI Codex app" -Source "msstore"

# Visual Studio with .NET desktop workload
winget install --id Microsoft.VisualStudio.2022.Community --exact --source winget `
    --accept-package-agreements `
    --accept-source-agreements `
    --override "--quiet --wait --add Microsoft.VisualStudio.Workload.ManagedDesktop --includeRecommended"

# Project folders
$folders = @(
    "C:\Dev",
    "C:\Dev\ZenPinballLauncher",
    "C:\Pinball",
    "C:\Pinball\DOFLinx",
    "C:\Pinball\DMDext",
    "C:\Pinball\Logs"
)

foreach ($folder in $folders) {
    if (!(Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder | Out-Null
    }
}

Write-Host ""
Write-Host "Bootstrap complete."
Write-Host "Restart Windows, then run:"
Write-Host "  dotnet --info"
Write-Host "  git --version"
