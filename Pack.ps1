#requires -Version 5.1
<#
.SYNOPSIS
Creates a Thunderstore-ready zip containing both Mono and IL2CPP builds.

.DESCRIPTION
Packages the mod for Thunderstore distribution. Optionally updates version numbers
across all project files (manifest.json, Constants.cs, .csproj) before packaging.

.PARAMETER Version
The version number to set (e.g., "2.2.0"). If not provided, uses existing version from manifest.json.

.EXAMPLE
.\Pack.ps1
Creates package with current version

.EXAMPLE
.\Pack.ps1 -Version "2.2.0"
Updates version to 2.2.0 and creates package
#>
[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

# Read current version from manifest.json if not provided
if (-not $Version) {
    $manifestPath = Join-Path $root 'manifest.json'
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $Version = $manifest.version_number
    Write-Host "Using current version: $Version"
} else {
    Write-Host "Updating version to: $Version"
    
    # Validate version format (semantic versioning: X.Y.Z)
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Version must be in format X.Y.Z (e.g., 2.1.0)"
        exit 1
    }
    
    # Update manifest.json
    $manifestPath = Join-Path $root 'manifest.json'
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $manifest.version_number = $Version
    $manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath -Encoding UTF8 -NoNewline
    Write-Host "Updated manifest.json"
    
    # Update Constants.cs
    $constantsPath = Join-Path $root 'Helpers\Constants.cs'
    $constantsContent = Get-Content $constantsPath -Raw
    $constantsContent = $constantsContent -replace 'public const string ModVersion = ".*?";', "public const string ModVersion = `"$Version`";"
    Set-Content $constantsPath -Value $constantsContent -Encoding UTF8 -NoNewline
    Write-Host "Updated Helpers/Constants.cs"
    
    # Update .csproj (AssemblyVersion and FileVersion with .0 suffix)
    $csprojPath = Join-Path $root 'Small Corner Map.csproj'
    $csprojContent = Get-Content $csprojPath -Raw
    $assemblyVersion = "$Version.0"
    $csprojContent = $csprojContent -replace '<AssemblyVersion>.*?</AssemblyVersion>', "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
    $csprojContent = $csprojContent -replace '<FileVersion>.*?</FileVersion>', "<FileVersion>$assemblyVersion</FileVersion>"
    Set-Content $csprojPath -Value $csprojContent -Encoding UTF8 -NoNewline
    Write-Host "Updated Small Corner Map.csproj"
    
    Write-Host ""
    
    # Check if CHANGELOG.md has been updated for this version
    $changelogPath = Join-Path $root 'CHANGELOG.md'
    if (Test-Path $changelogPath) {
        $changelogContent = Get-Content $changelogPath -Raw
        if ($changelogContent -notmatch "## $Version") {
            Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
            Write-Host "⚠️  REMINDER: CHANGELOG.md does not appear to contain version $Version" -ForegroundColor Yellow
            Write-Host "   Opening CHANGELOG.md for you to add release notes..." -ForegroundColor Yellow
            Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
            Write-Host ""
            
            # Open CHANGELOG.md in default editor
            Start-Process -FilePath $changelogPath -Wait
            
            # Re-check if changelog was updated
            $changelogContent = Get-Content $changelogPath -Raw
            if ($changelogContent -notmatch "## $Version") {
                Write-Host "⚠️  CHANGELOG.md still does not contain version $Version" -ForegroundColor Yellow
                $response = Read-Host "Continue with packaging anyway? (y/N)"
                if ($response -ne 'y' -and $response -ne 'Y') {
                    Write-Host "Packaging cancelled. Please update CHANGELOG.md and run again." -ForegroundColor Cyan
                    exit 0
                }
            } else {
                Write-Host "✓ CHANGELOG.md has been updated with version $Version" -ForegroundColor Green
            }
            Write-Host ""
        }
    }
}

$packageName = 'SmallCornerMap'
$work = Join-Path $root 'dist'
if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Path $work | Out-Null

# Create package folder structure
$pkg = Join-Path $work $packageName
New-Item -ItemType Directory -Path $pkg | Out-Null
New-Item -ItemType Directory -Path (Join-Path $pkg 'Mods') | Out-Null

# Copy artifacts
Copy-Item (Join-Path $root 'manifest.json') $pkg
Copy-Item (Join-Path $root 'README.md') $pkg
Copy-Item (Join-Path $root 'LICENSE.txt') $pkg
if (Test-Path (Join-Path $root 'icon.png')) { Copy-Item (Join-Path $root 'icon.png') $pkg }

# DLL sources
$monoDll = Join-Path $root 'bin/Mono/netstandard2.1/Small_Corner_Map.Mono.dll'
$il2cppDll = Join-Path $root 'bin/IL2CPP/net6.0/Small_Corner_Map.Il2cpp.dll'
Copy-Item $monoDll (Join-Path $pkg 'Mods')
Copy-Item $il2cppDll (Join-Path $pkg 'Mods')

# Zip
$zipName = "$packageName-$Version.zip"
$zipPath = Join-Path $work $zipName
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($pkg, $zipPath)
Write-Host "Created package: $zipPath"

