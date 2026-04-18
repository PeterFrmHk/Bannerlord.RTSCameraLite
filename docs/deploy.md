# Package, audit, and deploy

This repo ships PowerShell scripts under **`scripts/`** to build a **full** module tree, verify a Steam/game install, and optionally deploy with backup.

**Default safety:** Scripts that modify your game only run when you explicitly execute **`deploy-to-steam.ps1`**. **`audit-steam-deployment.ps1` is read-only.**

---

## 1. Package (build + stage + zip)

Creates **`artifacts/Bannerlord.RTSCameraLite/`** with:

- `SubModule.xml`
- `config/commander_config.json`
- `bin/Win64_Shipping_Client/` — **all** files from the project output (mod DLL + NuGet dependency DLLs). TaleWorlds assemblies are **not** copied (reference-only in the csproj).

Optional zip: **`artifacts/Bannerlord.RTSCameraLite.zip`**

```powershell
powershell -ExecutionPolicy Bypass -File scripts/package-module.ps1 -Configuration Release -Clean
```

Flags:

| Flag | Meaning |
| --- | --- |
| `-Configuration Release` | `dotnet build -c Release` |
| `-Clean` | Delete `artifacts/Bannerlord.RTSCameraLite` before staging |
| `-NoZip` | Skip creating the zip |
| `-SkipBuild` | Only stage; requires existing `bin/Win64_Shipping_Client` |

---

## 2. Audit (read-only)

Verifies a deployed module under the game **`Modules`** folder: paths, `SubModule.xml` Id/DLLName, official dependencies, optional comparison to repo **`bin/Win64_Shipping_Client`**, and whether **`EnableMissionRuntimeHooks`** is false (load-safe default).

```powershell
powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1
```

Optional:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1 -GameRoot "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord" -ModuleId Bannerlord.RTSCameraLite
```

Resolution order: **`-GameRoot`** → **`BANNERLORD_INSTALL`** → default Steam path → **`libraryfolders.vdf`** scan.

---

## 3. Deploy (copy + backup + optional unblock + audit)

**Requires** `-GameRoot` **or** a resolvable install (same rules as audit). Backs up an existing `Modules/Bannerlord.RTSCameraLite` to **`artifacts/backups/Bannerlord.RTSCameraLite_<timestamp>`** before overwrite. Does **not** touch other mods.

```powershell
powershell -ExecutionPolicy Bypass -File scripts/deploy-to-steam.ps1 -GameRoot "<BannerlordGameRoot>" -UnblockDlls
```

| Flag | Meaning |
| --- | --- |
| `-UnblockDlls` | `Unblock-File` on all `*.dll` under the deployed module (helps with browser-blocked downloads) |
| `-SkipPackage` | Do not run `package-module.ps1` first (expects `artifacts/...` already) |
| `-WhatIf` / `-DryRun` | Show intent only; no copy |

**Example (WhatIf):**

```powershell
powershell -ExecutionPolicy Bypass -File scripts/deploy-to-steam.ps1 -GameRoot "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord" -WhatIf
```

---

## Runtime safety note

- Keeping **`EnableMissionRuntimeHooks=false`** (default) is the **load-safe** baseline; see `config/commander_config.json`.
- **Correct deployment does not guarantee** no battle-time crashes. If crashes occur after main menu loads, treat mission features as suspect: confirm hooks stay off, then follow **Crash Quarantine** / slice docs before enabling experimental options.

---

## Research

- Public conventions: `docs/research/public-deployment-scan.md`
- Example local Steam layout: `docs/research/local-steam-mod-scan.md`
