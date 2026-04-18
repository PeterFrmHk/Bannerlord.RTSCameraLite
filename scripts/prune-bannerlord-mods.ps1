#requires -Version 5.1
<#
.SYNOPSIS
  Dry-run by default: scans Bannerlord Modules and optionally Workshop 261550 for
  quarantine candidates. With -Execute, moves folders to quarantine roots (no permanent deletes).

.DESCRIPTION
  - Never touches official TaleWorlds module folders (by folder name and by Module Id).
  - Never quarantines Bannerlord.Harmony when the Workshop copy looks valid (DLL present).
  - Local manual modules: prefers quarantine to GameRoot\Modules_DISABLED_<timestamp>\.
  - Workshop: only processed when -IncludeWorkshop is set; Steam-owned content is moved only
    to GameRoot\Workshop261550_QUARANTINE_<timestamp>\ (optional backup of entire folder first).

.PARAMETER Execute
  Perform moves. Without this switch, only prints the plan.

.PARAMETER IncludeWorkshop
  Include orphan/broken Workshop folder detection. Required to quarantine Workshop paths.

.PARAMETER GameRoot
  Mount & Blade II Bannerlord installation root.

.PARAMETER WorkshopRoot
  Steam Workshop content path for app 261550.

.NOTES
  Run from an elevated shell if GameRoot is under Program Files and moves fail.
#>
[CmdletBinding()]
param(
    [string] $GameRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord',
    [string] $WorkshopRoot = 'C:\Program Files (x86)\Steam\steamapps\workshop\content\261550',
    [switch] $Execute,
    [switch] $IncludeWorkshop
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-OfficialSets {
    # Folder names under Modules (case-insensitive) that must never be quarantined.
    $folder = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($n in @(
            'Native', 'SandBoxCore', 'Sandbox', 'StoryMode', 'CustomBattle',
            'Multiplayer', 'BirthAndDeath', 'NavalDLC', 'FastMode'
        )) { [void]$folder.Add($n) }
    # Module Id values from SubModule.xml for the same products (extra safety if folder renamed).
    $moduleId = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($n in @(
            'Native', 'SandBoxCore', 'Sandbox', 'StoryMode', 'CustomBattle',
            'Multiplayer', 'BirthAndDeath', 'NavalDLC', 'FastMode'
        )) { [void]$moduleId.Add($n) }
    return [PSCustomObject]@{ FolderNames = $folder; ModuleIds = $moduleId }
}

function Test-IsOfficialModule {
    param(
        [string] $FolderName,
        [string] $ModuleId,
        [object] $Official
    )
    if ($Official.FolderNames.Contains($FolderName)) { return $true }
    if ($ModuleId -and $Official.ModuleIds.Contains($ModuleId)) { return $true }
    return $false
}

function Read-SubModuleMeta {
    param([string] $SubModuleXmlPath)
    $result = [PSCustomObject]@{
        ModuleId     = $null
        DisplayName  = $null
        DllNames     = [string[]]@()
        SubModulesEmpty = $false
        ParseError   = $null
    }
    try {
        [xml] $xml = Get-Content -LiteralPath $SubModuleXmlPath -Encoding UTF8
        $m = $xml.Module
        if (-not $m) { throw 'No <Module> root' }
        if ($m.Id) { $result.ModuleId = [string]$m.Id.value }
        if ($m.Name) { $result.DisplayName = [string]$m.Name.value }
        $sms = $m.SubModules
        if (-not $sms -or -not $sms.SubModule) {
            $result.SubModulesEmpty = $true
            return $result
        }
        $list = @($sms.SubModule)
        foreach ($sm in $list) {
            if ($sm.DLLName) {
                $dll = [string]$sm.DLLName.value
                if ($dll) { $result.DllNames += $dll }
            }
        }
    }
    catch {
        $result.ParseError = $_.Exception.Message
    }
    return $result
}

function Get-WorkshopModuleIds {
    param([string] $WorkshopRootPath)
    $map = @{} # Id -> first path seen
    if (-not (Test-Path -LiteralPath $WorkshopRootPath)) { return $map }
    Get-ChildItem -LiteralPath $WorkshopRootPath -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $item = $_
        $xmlPath = Join-Path $item.FullName 'SubModule.xml'
        if (-not (Test-Path -LiteralPath $xmlPath)) { return }
        $meta = Read-SubModuleMeta -SubModuleXmlPath $xmlPath
        if ($meta.ModuleId -and -not $map.ContainsKey($meta.ModuleId)) {
            $map[$meta.ModuleId] = $item.FullName
        }
    }
    return $map
}

function Test-DllsPresent {
    param(
        [string] $ModuleRoot,
        [string[]] $DllNames
    )
    $bin = Join-Path $ModuleRoot 'bin\Win64_Shipping_Client'
    foreach ($d in $DllNames) {
        $p = Join-Path $bin $d
        if (-not (Test-Path -LiteralPath $p)) { return $false }
    }
    return $true
}

function Test-FolderEmptyish {
    param([string] $Path)
    $any = Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue | Select-Object -First 1
    return -not $any
}

$official = Get-OfficialSets
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$modulesPath = Join-Path $GameRoot 'Modules'
$localQuarantine = Join-Path $GameRoot "Modules_DISABLED_$timestamp"
$workshopQuarantine = Join-Path $GameRoot "Workshop261550_QUARANTINE_$timestamp"

Write-Host "=== prune-bannerlord-mods.ps1 ===" -ForegroundColor Cyan
Write-Host "GameRoot:        $GameRoot"
Write-Host "WorkshopRoot:    $WorkshopRoot"
Write-Host "Mode:            $(if ($Execute) { 'EXECUTE (moves)' } else { 'DRY RUN (no moves)' })"
Write-Host "IncludeWorkshop: $IncludeWorkshop"
Write-Host "Local quarantine root (if used):   $localQuarantine"
Write-Host "Workshop quarantine root (if used): $workshopQuarantine"
Write-Host ""

if (-not (Test-Path -LiteralPath $modulesPath)) {
    Write-Error "Modules path not found: $modulesPath"
}

$workshopById = Get-WorkshopModuleIds -WorkshopRootPath $WorkshopRoot

# --- Harmony: reference validation (never quarantine if healthy single copy)
$harmonyWorkshopPath = $null
if ($workshopById['Bannerlord.Harmony']) {
    $harmonyWorkshopPath = $workshopById['Bannerlord.Harmony']
}
$harmonyHealthy = $false
if ($harmonyWorkshopPath) {
    $hXml = Join-Path $harmonyWorkshopPath 'SubModule.xml'
    $hMeta = Read-SubModuleMeta -SubModuleXmlPath $hXml
    if ($hMeta.DllNames.Count -gt 0 -and (Test-DllsPresent -ModuleRoot $harmonyWorkshopPath -DllNames @($hMeta.DllNames[0]))) {
        $harmonyHealthy = $true
    }
}
Write-Host "[Reference] Bannerlord.Harmony workshop path: $(if ($harmonyWorkshopPath) { $harmonyWorkshopPath } else { '<not found>' })"
Write-Host "[Reference] Harmony primary DLL looks present: $harmonyHealthy"
Write-Host ""

$actions = [System.Collections.Generic.List[object]]::new()
$healthyLocal = [System.Collections.Generic.List[string]]::new()

# --- Local Modules scan
Get-ChildItem -LiteralPath $modulesPath -Directory -ErrorAction SilentlyContinue | ForEach-Object {
    $dir = $_
    $folderName = $dir.Name
    $full = $dir.FullName
    $subXml = Join-Path $full 'SubModule.xml'

    $mid = $null
    $display = $null
    if (Test-Path -LiteralPath $subXml) {
        $meta = Read-SubModuleMeta -SubModuleXmlPath $subXml
        $mid = $meta.ModuleId
        $display = $meta.DisplayName
    }

    if (Test-IsOfficialModule -FolderName $folderName -ModuleId $mid -Official $official) {
        return
    }

    # Stale Bannerlord.RTSCameraLite heuristic: missing XML or declared DLLs missing with empty/problem bin
    $isRtscamLite = ($folderName -eq 'Bannerlord.RTSCameraLite') -or ($mid -eq 'Bannerlord.RTSCameraLite')

    if (Test-FolderEmptyish -Path $full) {
        $actions.Add([PSCustomObject]@{
                Scope      = 'local'
                Action     = 'quarantine'
                Reason     = 'Folder is empty (or only hidden entries not enumerated).'
                Path       = $full
                ModuleId   = $mid
                DisplayName = $display
                Risk       = 'Medium'
            })
        return
    }

    if (-not (Test-Path -LiteralPath $subXml)) {
        $actions.Add([PSCustomObject]@{
                Scope      = 'local'
                Action     = 'quarantine'
                Reason     = 'No SubModule.xml at module root (not an official protected folder).'
                Path       = $full
                ModuleId   = $mid
                DisplayName = $display
                Risk       = 'Medium'
            })
        return
    }

    $m = Read-SubModuleMeta -SubModuleXmlPath $subXml
    if ($m.ParseError) {
        $actions.Add([PSCustomObject]@{
                Scope      = 'local'
                Action     = 'investigate'
                Reason     = "SubModule.xml parse error: $($m.ParseError)"
                Path       = $full
                ModuleId   = $m.ModuleId
                DisplayName = $m.DisplayName
                Risk       = 'Low'
            })
        return
    }

    if ($m.DllNames.Count -gt 0 -and -not (Test-DllsPresent -ModuleRoot $full -DllNames $m.DllNames)) {
        $reason = 'SubModule.xml declares DLL(s) but one or more are missing under bin\Win64_Shipping_Client: ' + ($m.DllNames -join '; ')
        if ($isRtscamLite) { $reason = '[RTS Commander Doctrine / RTSCameraLite] ' + $reason }
        $actions.Add([PSCustomObject]@{
                Scope      = 'local'
                Action     = 'quarantine'
                Reason     = $reason
                Path       = $full
                ModuleId   = $m.ModuleId
                DisplayName = $m.DisplayName
                Risk       = 'High'
            })
        return
    }

    # Duplicate manual install when Workshop has same Module Id
    if ($m.ModuleId -and $workshopById.ContainsKey($m.ModuleId)) {
        $wsPath = $workshopById[$m.ModuleId]
        $actions.Add([PSCustomObject]@{
                Scope      = 'local'
                Action     = 'quarantine'
                Reason     = "Duplicate Module Id '$($m.ModuleId)': valid Workshop copy exists at $wsPath. Quarantine manual copy to avoid double registration."
                Path       = $full
                ModuleId   = $m.ModuleId
                DisplayName = $m.DisplayName
                Risk       = 'Medium'
            })
        return
    }

    # Healthy non-official local with XML and DLLs OK - track for summary only (no row in actions table)
    $label = "$folderName | ModuleId=$($m.ModuleId) | $($m.DisplayName)"
    if (-not $healthyLocal.Contains($label)) { [void]$healthyLocal.Add($label) }
}

# --- Workshop orphan / junk folders (no SubModule.xml at Workshop id root)
if ($IncludeWorkshop) {
    if (-not (Test-Path -LiteralPath $WorkshopRoot)) {
        Write-Warning "WorkshopRoot not found: $WorkshopRoot"
    }
    else {
        Get-ChildItem -LiteralPath $WorkshopRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object {
            $wdir = $_
            $wxml = Join-Path $wdir.FullName 'SubModule.xml'
            if (Test-Path -LiteralPath $wxml) { return }

            # Never touch numeric folders that might be mid-download; user rule: only clearly broken/orphan
            $actions.Add([PSCustomObject]@{
                    Scope      = 'workshop'
                    Action     = 'quarantine'
                    Reason     = 'Workshop item folder has no SubModule.xml at root (orphan/junk or failed download). Not Steam-managed module layout. Optional: unsubscribe stale Workshop id in Steam if listed.'
                    Path       = $wdir.FullName
                    ModuleId   = $null
                    DisplayName = "(orphan folder $($wdir.Name))"
                    Risk       = 'Medium'
                })
        }
    }
}
else {
    Write-Host "[Workshop] Skipped (use -IncludeWorkshop to scan orphan Workshop folders)." -ForegroundColor DarkYellow
}

# Filter Harmony: do not quarantine a healthy reference copy (manual duplicate of Workshop Harmony)
$filtered = [System.Collections.Generic.List[object]]::new()
foreach ($a in $actions) {
    if ($a.Action -eq 'quarantine' -and $a.ModuleId -eq 'Bannerlord.Harmony' -and $harmonyHealthy) {
        Write-Host "[Skip] Not quarantining Bannerlord.Harmony - Workshop reference copy appears healthy; resolve manual duplicate manually if needed." -ForegroundColor Green
        continue
    }
    $filtered.Add($a)
}
$actions = $filtered

# --- Report
$toMoveLocal = @($actions | Where-Object { $_.Scope -eq 'local' -and $_.Action -eq 'quarantine' })
$toMoveWs = @($actions | Where-Object { $_.Scope -eq 'workshop' -and $_.Action -eq 'quarantine' })

if ($healthyLocal.Count -gt 0) {
    Write-Host "--- Non-official local modules with no auto-quarantine rule matched ---" -ForegroundColor DarkGreen
    $healthyLocal | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
}

Write-Host "--- Planned actions (quarantine / investigate only) ---" -ForegroundColor Cyan
if ($actions.Count -eq 0) {
    Write-Host "(none)"
}
else {
    foreach ($a in ($actions | Sort-Object Scope, Action, Path)) {
        Write-Host ""
        Write-Host "[$($a.Scope)] $($a.Action) | Risk=$($a.Risk)" -ForegroundColor Yellow
        Write-Host "  ModuleId   : $($a.ModuleId)"
        Write-Host "  Display    : $($a.DisplayName)"
        Write-Host "  Path       : $($a.Path)"
        Write-Host "  Reason     : $($a.Reason)"
    }
}
Write-Host ""
Write-Host "Quarantine moves (local):  $($toMoveLocal.Count)"
Write-Host "Quarantine moves (workshop): $($toMoveWs.Count)"
Write-Host ""

if (-not $Execute) {
    Write-Host "Dry run complete. No files were moved." -ForegroundColor Green
    exit 0
}

# --- Execute moves (no deletes)
if ($toMoveLocal.Count -gt 0) {
    New-Item -ItemType Directory -Path $localQuarantine -Force | Out-Null
}
if ($IncludeWorkshop -and $toMoveWs.Count -gt 0) {
    New-Item -ItemType Directory -Path $workshopQuarantine -Force | Out-Null
}

foreach ($a in $toMoveLocal) {
    $destParent = $localQuarantine
    $leaf = Split-Path -Leaf $a.Path
    $dest = Join-Path $destParent $leaf
    Write-Host ""
    Write-Host "EXEC local quarantine: $($a.Reason)" -ForegroundColor Cyan
    Write-Host "MOVE local: $($a.Path) -> $dest"
    Move-Item -LiteralPath $a.Path -Destination $dest
}

if ($IncludeWorkshop) {
    foreach ($a in $toMoveWs) {
        $leaf = Split-Path -Leaf $a.Path
        $dest = Join-Path $workshopQuarantine $leaf
        Write-Host ""
        Write-Host "EXEC workshop quarantine: $($a.Reason)" -ForegroundColor Cyan
        Write-Host "MOVE workshop: $($a.Path) -> $dest"
        Move-Item -LiteralPath $a.Path -Destination $dest
    }
}

Write-Host 'Done. Review quarantine folders and use Steam verify if Workshop content was moved.' -ForegroundColor Green
