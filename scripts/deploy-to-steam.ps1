#Requires -Version 5.1
<#
.SYNOPSIS
  Copies packaged module into the game Modules folder with backup; optionally unblocks DLLs and runs audit.

.PARAMETER GameRoot
  Required unless resolvable via default Steam path or BANNERLORD_INSTALL.

.PARAMETER ModuleId
  Default Bannerlord.RTSCameraLite.

.PARAMETER SkipPackage
  Do not run package-module.ps1 first (artifacts must exist).

.PARAMETER UnblockDlls
  Run Unblock-File on all DLLs under the deployed module.

.PARAMETER WhatIf
  Show what would happen without copying.

.PARAMETER DryRun
  Alias for WhatIf.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/deploy-to-steam.ps1 -GameRoot "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord" -UnblockDlls
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$GameRoot = "",

    [string]$ModuleId = "Bannerlord.RTSCameraLite",

    [switch]$SkipPackage,

    [switch]$UnblockDlls,

    [switch]$WhatIf,

    [Alias("DryRun")]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$artifactModule = Join-Path $repoRoot "artifacts\$ModuleId"
$backupRoot = Join-Path $repoRoot "artifacts\backups"
$auditScript = Join-Path $repoRoot "scripts\audit-steam-deployment.ps1"

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

$whatIfMode = $WhatIf -or $DryRun
if ($whatIfMode) {
    $PSCmdlet.ShouldProcess("deploy", "WhatIf") | Out-Null
}

$targetRoot = Find-GameRoot -Explicit $GameRoot
if (-not $targetRoot) {
    throw "Game root not found. Pass -GameRoot '<path to Mount & Blade II Bannerlord>' or set BANNERLORD_INSTALL."
}

$dest = Join-Path $targetRoot "Modules\$ModuleId"

if (-not $SkipPackage) {
    $pkg = Join-Path $repoRoot "scripts\package-module.ps1"
    Write-Host "Running package-module.ps1..." -ForegroundColor Cyan
    if (-not $whatIfMode) {
        & $pkg -Configuration Release
        if ($LASTEXITCODE -ne 0) { throw "package-module.ps1 failed with exit code $LASTEXITCODE" }
    }
    else {
        Write-Host "[WhatIf] Would run: $pkg -Configuration Release" -ForegroundColor Yellow
    }
}

if (-not (Test-Path -LiteralPath $artifactModule)) {
    throw "Packaged module not found: $artifactModule. Run package-module.ps1 first or remove -SkipPackage."
}

if ($whatIfMode) {
    Write-Host "[WhatIf] Would copy:" -ForegroundColor Yellow
    Write-Host "  From: $artifactModule"
    Write-Host "  To:   $dest"
    Write-Host "[WhatIf] Would backup existing dest to artifacts/backups if present."
    exit 0
}

if (Test-Path -LiteralPath $dest) {
    $ts = Get-Date -Format "yyyyMMdd_HHmmss"
    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
    $backup = Join-Path $backupRoot "${ModuleId}_$ts"
    Write-Host "Backing up existing module to: $backup" -ForegroundColor Cyan
    Copy-Item -LiteralPath $dest -Destination $backup -Recurse -Force
}

$parent = Split-Path $dest -Parent
if (-not (Test-Path -LiteralPath $parent)) {
    throw "Modules parent missing: $parent"
}

if (Test-Path -LiteralPath $dest) {
    Remove-Item -LiteralPath $dest -Recurse -Force
}

Copy-Item -LiteralPath $artifactModule -Destination $dest -Recurse -Force
Write-Host "Deployed to: $dest" -ForegroundColor Green

if ($UnblockDlls) {
    Get-ChildItem -LiteralPath $dest -Recurse -Include *.dll -File -ErrorAction SilentlyContinue | ForEach-Object {
        Unblock-File -LiteralPath $_.FullName -ErrorAction SilentlyContinue
    }
    Write-Host "Unblock-File applied to DLLs under module." -ForegroundColor Green
}

Write-Host "Running audit..." -ForegroundColor Cyan
& $auditScript -GameRoot $targetRoot -ModuleId $ModuleId -RepoRoot $repoRoot
