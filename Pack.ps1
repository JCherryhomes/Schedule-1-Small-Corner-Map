#requires -Version 5.1
<#!
Creates a Thunderstore-ready zip containing both Mono and IL2CPP builds.
Assumes builds already completed.
#>
$ErrorActionPreference = 'Stop'
$version = '2.1.0'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
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
$zipName = "$packageName-$version.zip"
$zipPath = Join-Path $work $zipName
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($pkg, $zipPath)
Write-Host "Created package: $zipPath"

