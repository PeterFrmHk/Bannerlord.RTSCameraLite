#Requires -Version 5.1
<#
.SYNOPSIS
  Read-only audit of a Bannerlord.RTSCameraLite deployment under the game Modules folder.

.PARAMETER GameRoot
  Mount & Blade II Bannerlord installation root (folder containing Modules).

.PARAMETER ModuleId
  Module folder name (default: Bannerlord.RTSCameraLite).

.PARAMETER RepoRoot
  Optional repo root containing bin/Win64_Shipping_Client for dependency comparison.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1
#>
[CmdletBinding()]
param(
    [string]$GameRoot = "",

    [string]$ModuleId = "Bannerlord.RTSCameraLite",

    [string]$RepoRoot = ""
)

$ErrorActionPreference = "Continue"

function Find-GameRoot {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path -LiteralPath (Join-Path $Explicit "Modules"))) {
        return (Resolve-Path $Explicit).Path
    }
    $envRoot = $env:BANNERLORD_INSTALL
    if ($envRoot -and (Test-Path -LiteralPath (Join-Path $envRoot "Modules"))) {
        return (Resolve-Path $envRoot).Path
    }
    $candidates = @(
        "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord",
        "C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord"
    )
    foreach ($c in $candidates) {
        if (Test-Path -LiteralPath (Join-Path $c "Modules")) {
            return $c
        }
    }
    $vdf = Join-Path ${env:ProgramFiles(x86)} "Steam\steamapps\libraryfolders.vdf"
    if (Test-Path -LiteralPath $vdf) {
        $txt = Get-Content -LiteralPath $vdf -Raw -ErrorAction SilentlyContinue
        if ($txt) {
            [regex]::Matches($txt, '"path"\s+"([^"]+)"') | ForEach-Object {
                $lib = $_.Groups[1].Value -replace '\\\\', '\'
                $g = Join-Path $lib "steamapps\common\Mount & Blade II Bannerlord"
                if (Test-Path -LiteralPath (Join-Path $g "Modules")) {
                    return $g
                }
            }
        }
    }
    return $null
}

function Test-One {
    param(
        [string]$Name,
        [object]$Ok,
        [string]$Detail = ""
    )
    $status = if ($Ok -eq $true) { "PASS" } elseif ($Ok -eq $false) { "FAIL" } else { "WARN" }
    $color = switch ($status) {
        "PASS" { "Green" }
        "WARN" { "Yellow" }
        default { "Red" }
    }
    Write-Host ("[{0}] {1} {2}" -f $status, $Name, $Detail) -ForegroundColor $color
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
}

$resolved = Find-GameRoot -Explicit $GameRoot
if (-not $resolved) {
    Test-One -Name "Game root resolved" -Ok $false -Detail "Set -GameRoot or install Bannerlord / BANNERLORD_INSTALL"
    exit 1
}

Test-One -Name "Game root resolved" -Ok $true -Detail $resolved

$modPath = Join-Path $resolved "Modules\$ModuleId"
$subXml = Join-Path $modPath "SubModule.xml"
$cfgJson = Join-Path $modPath "config\commander_config.json"
$binPath = Join-Path $modPath "bin\Win64_Shipping_Client"
$mainDll = Join-Path $binPath "$ModuleId.dll"

Test-One -Name "Module folder exists" -Ok (Test-Path -LiteralPath $modPath) -Detail $modPath
Test-One -Name "SubModule.xml at module root" -Ok (Test-Path -LiteralPath $subXml)
Test-One -Name "config/commander_config.json" -Ok (Test-Path -LiteralPath $cfgJson)
Test-One -Name "bin/Win64_Shipping_Client exists" -Ok (Test-Path -LiteralPath $binPath)
Test-One -Name "Main mod DLL present" -Ok (Test-Path -LiteralPath $mainDll)

# Official dependencies from SubModule template (same IDs as repo)
$deps = @("Native", "SandBoxCore", "Sandbox", "StoryMode", "CustomBattle")
foreach ($d in $deps) {
    $p = Join-Path $resolved "Modules\$d\SubModule.xml"
    Test-One -Name "Official dep: $d" -Ok (Test-Path -LiteralPath $p)
}

# SubModule.xml Id / DLLName
if (Test-Path -LiteralPath $subXml) {
    try {
        [xml]$x = Get-Content -LiteralPath $subXml -Encoding UTF8
        $id = $x.Module.Id.value
        $sub = $x.Module.SubModules.SubModule
        if ($sub -is [Array]) { $sub = $sub[0] }
        $dllName = $sub.DLLName.value
        Test-One -Name "SubModule Id matches module" -Ok ($id -eq $ModuleId) -Detail "Id=$id"
        Test-One -Name "DLLName is mod assembly" -Ok ($dllName -eq "$ModuleId.dll") -Detail "DLLName=$dllName"
    }
    catch {
        Test-One -Name "SubModule.xml parse" -Ok $false -Detail $_.Exception.Message
    }
}

# Compare DLL set to build output (if present)
$buildBin = Join-Path $RepoRoot "bin\Win64_Shipping_Client"
if (Test-Path -LiteralPath $buildBin) {
    $expected = Get-ChildItem -LiteralPath $buildBin -Filter "*.dll" -File | ForEach-Object { $_.Name }
    $deployed = if (Test-Path -LiteralPath $binPath) {
        @(Get-ChildItem -LiteralPath $binPath -Filter "*.dll" -File | ForEach-Object { $_.Name })
    } else { @() }
    foreach ($e in $expected) {
        $ok = $deployed -contains $e
        Test-One -Name "Deployed DLL matches build: $e" -Ok $ok
    }
    $extra = $deployed | Where-Object { $expected -notcontains $_ }
    foreach ($ex in $extra) {
        Test-One -Name "Extra DLL in deploy (not in current build output)" -Ok $null -Detail $ex
    }
}
else {
    Test-One -Name "Build output comparison" -Ok $null -Detail "No repo bin/Win64_Shipping_Client; run dotnet build"
}

# Runtime safety: EnableMissionRuntimeHooks
if (Test-Path -LiteralPath $cfgJson) {
    try {
        $raw = Get-Content -LiteralPath $cfgJson -Raw
        if ($raw -match '"EnableMissionRuntimeHooks"\s*:\s*true') {
            Test-One -Name "EnableMissionRuntimeHooks (default should be false for load-safe)" -Ok $null -Detail "Currently true - experimental hooks on"
        }
        elseif ($raw -match '"EnableMissionRuntimeHooks"\s*:\s*false') {
            Test-One -Name "EnableMissionRuntimeHooks false (load-safe default)" -Ok $true
        }
        else {
            Test-One -Name "EnableMissionRuntimeHooks explicit" -Ok $null -Detail "Key missing or unrecognized"
        }
    }
    catch { }
}

Write-Host ""
Write-Host "Audit complete (read-only)."
