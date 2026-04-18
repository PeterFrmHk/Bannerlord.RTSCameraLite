# Local Bannerlord prune / quarantine actions (review)

**Generated:** 2026-04-18  
**Inputs:** `local-bannerlord-mod-audit.md`, `local-bannerlord-mod-manifest.csv`, `local-bannerlord-mod-prune-plan.md`  
**Script:** `Bannerlord.RTSCameraLite/scripts/prune-bannerlord-mods.ps1` (also callable via repo root `scripts/prune-bannerlord-mods.ps1`)

This document lists **proposed** actions aligned with the automated scanner. **Nothing here is executed** by saving this file. Use the script in **dry run** first, then `-Execute` only after review.

---

## Script behavior (summary)

| Behavior | Detail |
|----------|--------|
| Default | **Dry run** — prints planned moves only |
| Execute | Requires **`-Execute`** to move folders |
| Workshop | Requires **`-IncludeWorkshop`** to evaluate/move Workshop paths |
| Deletes | **None** — only `Move-Item` to quarantine roots |
| Official modules | **Never** quarantined (folder name + Module Id allowlist) |
| Harmony | **Never** auto-quarantined when the Workshop reference copy is healthy (`Bannerlord.Harmony.dll` present) |
| Local quarantine root | `\<GameRoot>\Modules_DISABLED_<timestamp>\` |
| Workshop quarantine root | `\<GameRoot>\Workshop261550_QUARANTINE_<timestamp>\` |

**Protected official folder names / ids:** Native, SandBoxCore, Sandbox, StoryMode, CustomBattle, Multiplayer, BirthAndDeath, NavalDLC, FastMode.

---

## Automated actions (current machine state)

From the latest dry run against your documented paths:

### Local `Modules` (non-official)

| Proposed action | Module Id | Display name | Current path | Reason | Risk | How to restore |
|-----------------|-----------|--------------|----------------|--------|------|----------------|
| **keep** | `Bannerlord.RTSCameraLite` | RTS Commander Doctrine | `...\Mount & Blade II Bannerlord\Modules\Bannerlord.RTSCameraLite` | Root `SubModule.xml` present; declared DLL(s) present under `bin\Win64_Shipping_Client`; not a duplicate of a Workshop id (distinct from `RTSCamera` / `RTSCamera.CommandSystem`). | Low | N/A — leave in place for development/play. |

No other non-official local folders were present in the audit beyond this entry; the script would **quarantine** empty folders, folders without `SubModule.xml`, broken DLL layouts, or a **manual** folder whose `Module Id` matches an existing Workshop module (duplicate registration risk).

### Workshop `content\261550` (orphan folders only)

These are **not** subscribed entries in `appworkshop_261550.acf` per the audit; they contain stray files and no module XML.

| Proposed action | Module Id | Display name | Current path | Reason | Risk | How to restore |
|-----------------|-----------|--------------|----------------|--------|------|----------------|
| **quarantine** (optional; script + `-IncludeWorkshop` `-Execute`) | *(n/a)* | *(orphan)* | `...\workshop\content\261550\2875093027` | No `SubModule.xml`; junk/debris (`DismembermentPlus.json` observed). | Medium | Move folder back from `Workshop261550_QUARANTINE_<ts>\2875093027` to the original Workshop path, or delete and run Steam **Verify integrity** for Bannerlord. Prefer **unsubscribe** if the id reappears in Steam without intent. |
| **quarantine** (optional; same flags) | *(n/a)* | *(orphan)* | `...\workshop\content\261550\3302249842` | No `SubModule.xml`; junk/debris (`rca_log.txt` observed). | Medium | Same as above for `3302249842`. |

**Steam-first alternative (recommended before moving files):** use the launcher / Steam Workshop UI to **unsubscribe** or **disable** problematic items; avoid manual deletes inside `steamapps` unless you accept Steam re-download behavior.

---

## Special focus — no auto-quarantine from structure alone

These mods are **healthy on disk** per the manifest (XML + DLLs where expected). Launcher warnings are more likely **version skew** (`DependentVersion` vs game `v1.3.15`) or **load order** (Harmony before RTSCamera). **Do not** quarantine solely for launcher icons.

| Item | Workshop id | Module Id | Notes |
|------|---------------|-------------|--------|
| **Harmony (reference)** | 2859188632 | `Bannerlord.Harmony` | `SubModule.xml` at module root; `Bannerlord.Harmony.dll` under `bin\Win64_Shipping_Client`. Treat as the dependency anchor for RTSCamera / ButterLib / MCM chain. |
| **RTS Camera** | 3596692403 | `RTSCamera` | DLL present; uses `DependedModuleMetadata` for Harmony **LoadBeforeThis**. |
| **RTS Camera Command System** | 3596693285 | `RTSCamera.CommandSystem` | Same pattern as RTSCamera. |
| **RBM** | 2859251492 | `RBM` | `RBM.dll` present; optional deps on RTSCamera family. |
| **RTS Commander Doctrine** | *(local)* | `Bannerlord.RTSCameraLite` | Separate product id from Workshop RTSCamera; not a duplicate install. |

**Recommended non-destructive steps:** adjust mod order (Harmony above dependents), update mods for `1.3.15`, use BLSE logs if crashes occur.

---

## Commands (exact)

**Dry run (local Modules only):**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\scripts\prune-bannerlord-mods.ps1"
```

**Dry run including Workshop orphan scan:**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\scripts\prune-bannerlord-mods.ps1" -IncludeWorkshop
```

**Execute (moves) — Workshop orphans only (typical use on this machine):**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\scripts\prune-bannerlord-mods.ps1" -IncludeWorkshop -Execute
```

Run PowerShell **as Administrator** if `GameRoot` is under `Program Files` and moves fail with access denied.

---

## Restore instructions

1. **Local quarantine:** Move the folder from `...\Mount & Blade II Bannerlord\Modules_DISABLED_<timestamp>\<ModuleFolder>` back to `...\Modules\<ModuleFolder>`.
2. **Workshop quarantine:** Move from `...\Mount & Blade II Bannerlord\Workshop261550_QUARANTINE_<timestamp>\<id>` back to `...\steamapps\workshop\content\261550\<id>`, or unsubscribe/re-subscribe the Workshop item in Steam and let Steam repopulate the id folder.
3. **Steam verify:** After manual Workshop folder edits, use **Steam → Properties → Installed files → Verify** for Bannerlord if the game or launcher misbehaves.

---

## Actions scripted vs not executed

| | |
|--|--|
| **Scripted** | Dry-run detection; optional `-Execute` moves to quarantine roots; official + healthy Harmony protections. |
| **Not executed** | No automatic run was performed as part of authoring this document; you must run the script explicitly. |
| **Not scripted** | Unsubscribe/disable in Steam UI; Nexus reinstall; BLSE-only troubleshooting — document manually. |
