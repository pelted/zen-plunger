# bootstrap-dev.ps1
# Run from an elevated PowerShell window.

$ErrorActionPreference = "Stop"

function Assert-RunningAsAdministrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)

    if (!$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this script from an elevated PowerShell window."
    }
}

function Assert-WingetAvailable {
    if (!(Get-Command winget -ErrorAction SilentlyContinue)) {
        throw "winget is required but was not found. Install App Installer from the Microsoft Store, then rerun this script."
    }
}

function Test-WingetPackageInstalled {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Id
    )

    winget list --id $Id --exact --disable-interactivity | Out-Null
    return $LASTEXITCODE -eq 0
}

function Test-WingetPackageUpgradeAvailable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Id
    )

    winget upgrade --id $Id --exact --disable-interactivity | Out-Null
    return $LASTEXITCODE -eq 0
}

function Install-WingetPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Id,
        [string]$Name = $Id,
        [string]$Source = "winget",
        [string]$Override
    )

    if (Test-WingetPackageInstalled -Id $Id) {
        Write-Host "$Name is already installed. Skipping."
        return
    }

    Write-Host "Installing $Name..."
    $wingetArgs = @(
        "install",
        "--id", $Id,
        "--exact",
        "--source", $Source,
        "--accept-package-agreements",
        "--accept-source-agreements",
        "--disable-interactivity"
    )

    if ($Override) {
        $wingetArgs += @("--override", $Override)
    }

    winget @wingetArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install $Name ($Id). winget exited with code $LASTEXITCODE."
    }
}

function Update-WingetPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Id,
        [string]$Name = $Id,
        [string]$Source = "winget",
        [string]$Override
    )

    if (!(Test-WingetPackageUpgradeAvailable -Id $Id)) {
        Write-Host "$Name is already up to date."
        return
    }

    Write-Host "Updating $Name..."
    $wingetArgs = @(
        "upgrade",
        "--id", $Id,
        "--exact",
        "--source", $Source,
        "--accept-package-agreements",
        "--accept-source-agreements",
        "--disable-interactivity"
    )

    if ($Override) {
        $wingetArgs += @("--override", $Override)
    }

    winget @wingetArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to update $Name ($Id). winget exited with code $LASTEXITCODE."
    }
}

function Ensure-WingetPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Id,
        [string]$Name = $Id,
        [string]$Source = "winget",
        [string]$Override
    )

    if (Test-WingetPackageInstalled -Id $Id) {
        Update-WingetPackage -Id $Id -Name $Name -Source $Source -Override $Override
        return
    }

    Install-WingetPackage -Id $Id -Name $Name -Source $Source -Override $Override
}

Assert-RunningAsAdministrator
Assert-WingetAvailable

# Core tools
Ensure-WingetPackage "Microsoft.DotNet.SDK.10" ".NET 10 SDK"
Ensure-WingetPackage "Git.Git" "Git"
Ensure-WingetPackage "GitHub.cli" "GitHub CLI"
Ensure-WingetPackage "Microsoft.PowerShell" "PowerShell 7"
Ensure-WingetPackage "Microsoft.WindowsTerminal" "Windows Terminal"
Ensure-WingetPackage "Microsoft.VisualStudioCode" "VS Code"
Ensure-WingetPackage "Microsoft.Sysinternals" "Sysinternals"
Ensure-WingetPackage "Codex" "OpenAI Codex app" -Source "msstore"

# Visual Studio with .NET desktop workload
Ensure-WingetPackage `
    "Microsoft.VisualStudio.2022.Community" `
    "Visual Studio 2022 Community" `
    -Override "--quiet --wait --add Microsoft.VisualStudio.Workload.ManagedDesktop --includeRecommended"

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
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
}

Write-Host ""
Write-Host "Bootstrap complete."
Write-Host "Restart Windows, then run:"
Write-Host "  dotnet --info"
Write-Host "  git --version"
Write-Host "  gh --version"
Write-Host "  gh auth status"
Write-Host ""
Write-Host "If GitHub CLI is not authenticated yet, run:"
Write-Host "  gh auth login"
