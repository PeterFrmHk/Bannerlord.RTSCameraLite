#Requires -Version 5.1
<#
.SYNOPSIS
  Builds (unless skipped) and packages a deployable Bannerlord module into artifacts/Bannerlord.RTSCameraLite.

.DESCRIPTION
  Copies SubModule.xml, config/, and the full bin/Win64_Shipping_Client output (including PackageReference dependency DLLs).
  Does not copy TaleWorlds assemblies — they are reference-only in the project and are not emitted to build output.

.PARAMETER Configuration
  MSBuild configuration (default: Release).

.PARAMETER Clean
  Remove artifacts/Bannerlord.RTSCameraLite before packaging.

.PARAMETER NoZip
  Do not create artifacts/Bannerlord.RTSCameraLite.zip.

.PARAMETER SkipBuild
  Skip dotnet build; requires existing bin/Win64_Shipping_Client output.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/package-module.ps1 -Configuration Release -Clean
#>
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$Clean,

    [switch]$NoZip,

    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$moduleId = "Bannerlord.RTSCameraLite"
$artifactRoot = Join-Path $repoRoot "artifacts"
$staging = Join-Path $artifactRoot $moduleId
$binOut = Join-Path $repoRoot "bin\Win64_Shipping_Client"
$zipPath = Join-Path $artifactRoot "$moduleId.zip"

function Write-ModuleTree {
    param([string]$Path)
    $treeCmd = Join-Path $env:WINDIR "System32\tree.com"
    if (Test-Path -LiteralPath $treeCmd) {
        & $treeCmd /F $Path
    }
    else {
        Get-ChildItem -LiteralPath $Path -Recurse -Force | ForEach-Object { $_.FullName }
    }
}

if ($Clean -and (Test-Path -LiteralPath $staging)) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}

if (-not $SkipBuild) {
    Push-Location $repoRoot
    try {
        & dotnet restore | Write-Host
        & dotnet build -c $Configuration --no-incremental:$false | Write-Host
        if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
    }
    finally {
        Pop-Location
    }
}

if (-not (Test-Path -LiteralPath $binOut)) {
    throw "Build output not found: $binOut (build first or remove -NoBuild)."
}

$subModule = Join-Path $repoRoot "SubModule.xml"
$configSrc = Join-Path $repoRoot "config"
if (-not (Test-Path -LiteralPath $subModule)) { throw "Missing SubModule.xml at repo root." }
if (-not (Test-Path -LiteralPath $configSrc)) { throw "Missing config folder." }

New-Item -ItemType Directory -Path (Join-Path $staging "bin\Win64_Shipping_Client") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $staging "config") -Force | Out-Null

Copy-Item -LiteralPath $subModule -Destination (Join-Path $staging "SubModule.xml") -Force
Copy-Item -Path (Join-Path $configSrc "*") -Destination (Join-Path $staging "config") -Recurse -Force

# Full managed output: mod DLL + System.Text.Json etc. (not TaleWorlds — those are Private=false and not copied to output)
$binDest = Join-Path $staging "bin\Win64_Shipping_Client"
Get-ChildItem -LiteralPath $binOut -File -ErrorAction SilentlyContinue | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $binDest $_.Name) -Force
}

Write-Host ""
Write-Host "=== Package layout: $staging ===" -ForegroundColor Cyan
Write-ModuleTree -Path $staging
Write-Host ""

$dls = Get-ChildItem -LiteralPath (Join-Path $staging "bin\Win64_Shipping_Client") -Filter "*.dll" -File
Write-Host "DLL count in bin/Win64_Shipping_Client: $($dls.Count)" -ForegroundColor Green

if (-not $NoZip) {
    if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
    Compress-Archive -Path $staging -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "ZIP: $zipPath" -ForegroundColor Green
}

Write-Host "Done."
