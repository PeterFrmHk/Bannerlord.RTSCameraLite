#requires -Version 5.1
<#
.SYNOPSIS
  Read-only full mod manifest scan (local Modules + Workshop root SubModule.xml).
.PARAMETER GameRoot
.PARAMETER WorkshopRoot
.PARAMETER OutputCsv
#>
[CmdletBinding()]
param(
    [string] $GameRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord',
    [string] $WorkshopRoot = 'C:\Program Files (x86)\Steam\steamapps\workshop\content\261550',
    [string] $OutputCsv = ""
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($OutputCsv)) {
    $repoDocs = Resolve-Path (Join-Path $PSScriptRoot '..\..\docs\research')
    $OutputCsv = Join-Path $repoDocs.Path 'local-bannerlord-mod-manifest.csv'
}

function Read-SubModuleMeta {
    param([string] $SubModuleXmlPath)
    $result = [PSCustomObject]@{
        ModuleId = $null; DisplayName = $null; Version = $null
        DllNames = [string[]]@(); ParseError = $null
        Depended = ''; Incompatible = ''
    }
    try {
        [xml] $xml = Get-Content -LiteralPath $SubModuleXmlPath -Encoding UTF8
        $m = $xml.Module
        if (-not $m) { throw 'No Module root' }
        if ($m.Id) { $result.ModuleId = [string]$m.Id.value }
        if ($m.Name) { $result.DisplayName = [string]$m.Name.value }
        if ($m.Version) { $result.Version = [string]$m.Version.value }
        if ($m.DependedModules.DependedModule) {
            $result.Depended = @( $m.DependedModules.DependedModule | ForEach-Object { [string]$_.Id } ) -join ';'
        }
        if ($m.IncompatibleModules.IncompatibleModule) {
            $result.Incompatible = @( $m.IncompatibleModules.IncompatibleModule | ForEach-Object { [string]$_.Id } ) -join ';'
        }
        if ($m.SubModules.SubModule) {
            foreach ($sm in @($m.SubModules.SubModule)) {
                if ($sm.DLLName) { $dn = [string]$sm.DLLName.value; if ($dn) { $result.DllNames += $dn } }
            }
        }
    }
    catch { $result.ParseError = $_.Exception.Message }
    return $result
}

function Test-Dlls {
    param([string] $ModuleRoot, [string[]] $DllNames)
    if ($DllNames.Count -eq 0) { return $true }
    $bin = Join-Path $ModuleRoot 'bin\Win64_Shipping_Client'
    foreach ($d in $DllNames) {
        if (-not (Test-Path -LiteralPath (Join-Path $bin $d))) { return $false }
    }
    return $true
}

$officialIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
@('Native','SandBoxCore','Sandbox','StoryMode','CustomBattle','Multiplayer','BirthAndDeath','NavalDLC','FastMode') | ForEach-Object { [void]$officialIds.Add($_) }

$rows = [System.Collections.Generic.List[object]]::new()
$idsSeen = @{}

function Add-Row {
    param($Source, $DisplayName, $ModuleId, $Version, $Path, $HasXml, $HasBin, $BinNonEmpty, $HasDllDecl, $DeclaredDll, $DllOk, $ClassType, $Deps, $Incompat, $IsOfficial, $WorkshopId, $FolderName)
    $dupId = $false
    if ($ModuleId) {
        if ($idsSeen.ContainsKey($ModuleId)) { $dupId = $true } else { $idsSeen[$ModuleId] = $Path }
    }
    $rows.Add([PSCustomObject]@{
        Source = $Source; DisplayName = $DisplayName; ModuleId = $ModuleId; Version = $Version; Path = $Path
        HasRootSubModuleXml = $HasXml; HasBinWin64 = $HasBin; BinNonEmpty = $BinNonEmpty
        HasDeclaredDllName = $HasDllDecl; DeclaredDllName = $DeclaredDll; DllExistsExpected = $DllOk
        SubModuleClassType = $ClassType; DependedModules = $Deps; IncompatibleModules = $Incompat
        IsOfficial = $IsOfficial; DuplicateId = $dupId; WorkshopId = $WorkshopId; FolderName = $FolderName
    })
}

# Local Modules
$modulesPath = Join-Path $GameRoot 'Modules'
Get-ChildItem -LiteralPath $modulesPath -Directory -ErrorAction SilentlyContinue | ForEach-Object {
    $dir = $_.FullName; $fn = $_.Name
    $sx = Join-Path $dir 'SubModule.xml'
    $hasXml = Test-Path -LiteralPath $sx
    $bin = Join-Path $dir 'bin\Win64_Shipping_Client'
    $hasBin = Test-Path -LiteralPath $bin
    $binNon = $false
    if ($hasBin) { $binNon = @(Get-ChildItem -LiteralPath $bin -File -ErrorAction SilentlyContinue).Count -gt 0 }
    if (-not $hasXml) {
        Add-Row 'local_manual' '' '' '' $dir 'False' $hasBin $binNon 'False' '' 'False' '' '' '' 'False' '' $fn
        return
    }
    $m = Read-SubModuleMeta -SubModuleXmlPath $sx
    $firstDll = if ($m.DllNames.Count) { $m.DllNames[0] } else { '' }
    $hasDecl = $m.DllNames.Count -gt 0
    $dllOk = if (-not $hasDecl) { $false } else { Test-Dlls -ModuleRoot $dir -DllNames $m.DllNames }
    $isOff = $m.ModuleId -and $officialIds.Contains($m.ModuleId)
    $cls = ''
    try {
        [xml]$x = Get-Content -LiteralPath $sx -Encoding UTF8
        $sm = $x.Module.SubModules.SubModule
        if ($sm) { $one = @($sm)[0]; if ($one.SubModuleClassType) { $cls = [string]$one.SubModuleClassType.value } }
    } catch {}
    Add-Row 'local_manual' $m.DisplayName $m.ModuleId $m.Version $dir 'True' $hasBin $binNon "$hasDecl" $firstDll "$dllOk" $cls $m.Depended $m.Incompat "$isOff" '' $fn
}

# Workshop
if (Test-Path -LiteralPath $WorkshopRoot) {
    Get-ChildItem -LiteralPath $WorkshopRoot -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $dir = $_.FullName; $wid = $_.Name
        $sx = Join-Path $dir 'SubModule.xml'
        if (-not (Test-Path -LiteralPath $sx)) { return }
        $m = Read-SubModuleMeta -SubModuleXmlPath $sx
        $bin = Join-Path $dir 'bin\Win64_Shipping_Client'
        $hasBin = Test-Path -LiteralPath $bin
        $binNon = $false
        if ($hasBin) { $binNon = @(Get-ChildItem -LiteralPath $bin -File -ErrorAction SilentlyContinue).Count -gt 0 }
        $firstDll = if ($m.DllNames.Count) { $m.DllNames[0] } else { '' }
        $hasDecl = $m.DllNames.Count -gt 0
        $dllOk = if (-not $hasDecl) { $false } else { Test-Dlls -ModuleRoot $dir -DllNames $m.DllNames }
        $isOff = $false
        $cls = ''
        try {
            [xml]$x = Get-Content -LiteralPath $sx -Encoding UTF8
            $sm = $x.Module.SubModules.SubModule
            if ($sm) { $one = @($sm)[0]; if ($one.SubModuleClassType) { $cls = [string]$one.SubModuleClassType.value } }
        } catch {}
        Add-Row 'workshop' $m.DisplayName $m.ModuleId $m.Version $dir 'True' $hasBin $binNon "$hasDecl" $firstDll "$dllOk" $cls $m.Depended $m.Incompat "$isOff" $wid $wid
    }
}

$rows | Export-Csv -LiteralPath $OutputCsv -NoTypeInformation -Encoding UTF8

# Summary
$dupes = $rows | Where-Object { $_.DuplicateId -eq 'True' }
$framework = @('Bannerlord.Harmony','Bannerlord.ButterLib','Bannerlord.UIExtenderEx','Bannerlord.MBOptionScreen')
$installed = @{}
$rows | Where-Object { $_.ModuleId } | ForEach-Object { $installed[$_.ModuleId] = $true }
$missingDeps = [System.Collections.Generic.List[string]]::new()
foreach ($r in $rows) {
    if (-not $r.DependedModules) { continue }
    foreach ($d in ($r.DependedModules -split ';')) {
        $d = $d.Trim()
        if (-not $d) { continue }
        if ($officialIds.Contains($d)) { continue }
        if ($framework -contains $d) {
            if (-not $installed.ContainsKey($d)) { [void]$missingDeps.Add("$($r.ModuleId) -> missing $d") }
        }
    }
}

Write-Host "=== audit-bannerlord-mods (read-only) ===" -ForegroundColor Cyan
Write-Host "CSV written: $OutputCsv"
Write-Host "Total rows: $($rows.Count)"
Write-Host "Duplicate ModuleId rows: $($dupes.Count)"
if ($dupes.Count) { $dupes | Format-Table ModuleId, Path -AutoSize }
Write-Host "Framework dep gaps (Harmony/MCM chain only, if absent from scan): $($missingDeps.Count)"
if ($missingDeps.Count) { $missingDeps | ForEach-Object { Write-Host "  $_" } }
Write-Host "Done."
