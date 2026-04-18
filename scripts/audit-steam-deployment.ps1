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

.PARAMETER ModuleRoot
  Optional full path to the module folder root (e.g. artifacts/Bannerlord.RTSCameraLite after package-module.ps1).
  When set, layout checks use this path instead of GameRoot\Modules\<ModuleId>. GameRoot is still used for official deps and Workshop Harmony discovery.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1 -GameRoot "C:\...\Mount & Blade II Bannerlord" -ModuleRoot "C:\...\Bannerlord.RTSCameraLite\artifacts\Bannerlord.RTSCameraLite"
#>
[CmdletBinding()]
param(
    [string]$GameRoot = "",

    [string]$ModuleId = "Bannerlord.RTSCameraLite",

    [string]$RepoRoot = "",

    [string]$ModuleRoot = ""
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

if (-not [string]::IsNullOrWhiteSpace($ModuleRoot)) {
    if (-not (Test-Path -LiteralPath $ModuleRoot)) {
        Test-One -Name "ModuleRoot exists" -Ok $false -Detail $ModuleRoot
        exit 1
    }

    $modPath = (Resolve-Path -LiteralPath $ModuleRoot).Path
    Test-One -Name "Audit mode" -Ok $true -Detail "ModuleRoot (staging or custom): $modPath"
}
else {
    $modPath = Join-Path $resolved "Modules\$ModuleId"
}

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

# Bannerlord.Harmony (Workshop / optional manual): at least one path should resolve for launcher satisfaction
$harmonyLocal = Join-Path $resolved "Modules\Bannerlord.Harmony\SubModule.xml"
$steamApps = Split-Path (Split-Path $resolved -Parent) -Parent
$workshopRoot = Join-Path $steamApps "workshop\content\261550"
if (-not (Test-Path -LiteralPath $workshopRoot)) {
    $workshopRoot = Join-Path ${env:ProgramFiles(x86)} "Steam\steamapps\workshop\content\261550"
}
$harmonyWs = Join-Path $workshopRoot "2859188632\SubModule.xml"
$harmonyOk = (Test-Path -LiteralPath $harmonyLocal) -or (Test-Path -LiteralPath $harmonyWs)
if (-not $harmonyOk) {
    Test-One -Name "Bannerlord.Harmony module present (Modules or Workshop 2859188632)" -Ok $false -Detail "Install BUTR Bannerlord.Harmony (Workshop)"
}
else {
    $detail = if (Test-Path -LiteralPath $harmonyLocal) { $harmonyLocal } else { $harmonyWs }
    Test-One -Name "Bannerlord.Harmony module present" -Ok $true -Detail $detail
}

if (Test-Path -LiteralPath $subXml) {
    try {
        [xml]$sx = Get-Content -LiteralPath $subXml -Encoding UTF8
        $hasHarmonyDep = $false
        foreach ($n in @($sx.Module.DependedModules.DependedModule)) {
            if ($n.Id -eq "Bannerlord.Harmony") { $hasHarmonyDep = $true; break }
        }
        Test-One -Name "SubModule.xml declares DependedModule Bannerlord.Harmony" -Ok $hasHarmonyDep
    }
    catch {
        Test-One -Name "SubModule Harmony dep parse" -Ok $false -Detail $_.Exception.Message
    }
}

# SubModule.xml Id / DLLName + Workshop-style layout (compare to steamapps\workshop\content\261550\* patterns)
if (Test-Path -LiteralPath $subXml) {
    try {
        [xml]$x = Get-Content -LiteralPath $subXml -Encoding UTF8
        $id = $x.Module.Id.value
        $displayName = if ($x.Module.Name) { $x.Module.Name.value } else { "" }
        $sub = $x.Module.SubModules.SubModule
        if ($sub -is [Array]) { $sub = $sub[0] }
        $dllName = $sub.DLLName.value
        Test-One -Name "SubModule has Name" -Ok (-not [string]::IsNullOrWhiteSpace($displayName)) -Detail $displayName
        Test-One -Name "SubModule Id matches module" -Ok ($id -eq $ModuleId) -Detail "Id=$id"
        Test-One -Name "DLLName is mod assembly" -Ok ($dllName -eq "$ModuleId.dll") -Detail "DLLName=$dllName"
        $leaf = Split-Path -Leaf $modPath
        Test-One -Name "Folder name matches Module Id (261550 convention)" -Ok ($leaf -eq $ModuleId) -Detail "Folder=$leaf"

        $dllCount = (Get-ChildItem -LiteralPath $binPath -Filter "*.dll" -File -ErrorAction SilentlyContinue | Measure-Object).Count
        Test-One -Name "bin/Win64_Shipping_Client has managed DLLs" -Ok ($dllCount -gt 0) -Detail "Count=$dllCount"

        $subInBin = Join-Path $binPath "SubModule.xml"
        Test-One -Name "SubModule.xml not under bin (261550 layout)" -Ok (-not (Test-Path -LiteralPath $subInBin)) -Detail $subInBin
    }
    catch {
        Test-One -Name "SubModule.xml parse" -Ok $false -Detail $_.Exception.Message
    }
}

# Reference: sample Workshop item layout (Harmony) when present - same top-level SubModule + bin pattern as our package
$refHarmonyRoot = Join-Path $workshopRoot "2859188632"
if (Test-Path -LiteralPath (Join-Path $refHarmonyRoot "SubModule.xml")) {
    Test-One -Name "Workshop reference (2859188632 Harmony) layout present" -Ok $true -Detail $refHarmonyRoot
}
else {
    Test-One -Name "Workshop reference (2859188632) not on disk" -Ok $null -Detail "Cannot cross-check 261550 layout on this machine"
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
    if ($deployed -contains "0Harmony.dll") {
        Test-One -Name "0Harmony.dll must not ship with mod (use Bannerlord.Harmony)" -Ok $false
    }
    else {
        Test-One -Name "0Harmony.dll absent from deploy (expected)" -Ok $true
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

        if ($raw -match '"EnableHarmonyPatches"\s*:\s*true') {
            Test-One -Name "EnableHarmonyPatches (scaffold default should be false)" -Ok $null -Detail "Currently true"
        }
        elseif ($raw -match '"EnableHarmonyPatches"\s*:\s*false') {
            Test-One -Name "EnableHarmonyPatches false (expected for deployable default)" -Ok $true
        }
        else {
            Test-One -Name "EnableHarmonyPatches key" -Ok $null -Detail "Missing or unrecognized (migration may add)"
        }
    }
    catch { }
}

Write-Host ""
Write-Host "Audit complete (read-only)."
