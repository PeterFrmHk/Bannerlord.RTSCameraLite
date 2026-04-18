# Prune / quarantine plan (recommended only — not executed)

This document lists **optional** housekeeping steps inferred from the read-only audit. **Do not perform** these automatically; confirm in Steam, backups, and mod documentation before changing anything.

## 1. Orphan Workshop directories

**Observation:** Folders `2875093027` and `3302249842` under `steamapps\workshop\content\261550` contain stray files, no `SubModule.xml`, and do not appear in `appworkshop_261550.acf`.

**Recommendation:** After verifying nothing else references those paths, **delete** or **quarantine** these two folders to reduce clutter. If unsure, rename to `.bak` and launch the game once.

**Risk:** Low if they are truly orphaned; Steam may recreate empty placeholders on next verify.

## 2. Version skew warnings (RTSCamera, RBM, etc.)

**Observation:** Several mods declare `DependentVersion` for Native/SandBoxCore below the installed `v1.3.15`.

**Recommendation:**

- Prefer **updated** mod releases from Workshop/Nexus that list `1.3.15` (or compatible wildcards).
- If authors have not updated yet, many mods still run; **ignore launcher warnings** only after in-game smoke tests.
- Use **BLSE** log output if crashes occur — do not delete mods solely for cosmetic launcher icons.

## 3. Harmony load order

**Observation:** RTSCamera family uses `DependedModuleMetadata` for Harmony `LoadBeforeThis`.

**Recommendation:** In the launcher or mod manager, ensure **Bannerlord.Harmony** is **enabled** and **above** RTSCamera / Command System / any mod that patches the same systems.

## 4. RBM optional camera dependencies

**Observation:** RBM lists `RTSCamera` and `RTSCamera.CommandSystem` as **optional**.

**Recommendation:** If you do not use those camera mods, you can leave them disabled without breaking RBM’s core behavior; launcher messages may still reference them if enabled elsewhere.

## 5. Duplicate manual vs Workshop (future risk)

**Observation:** This scan found **no** duplicate `ModuleId` between `GameRoot\Modules` and Workshop.

**Recommendation:** When installing manual copies of Workshop mods, **remove** the duplicate path (manual or Workshop) to avoid future id collisions.

## 6. MCM / ButterLib / UIExtenderEx chain

**Observation:** Several mods depend on Harmony + UIExtenderEx and/or MCM.

**Recommendation:** Keep **Harmony → ButterLib → UIExtenderEx → MCM** (when required) in a sensible order per BUTR guidance; update all four together when bumping game versions.

## 7. Backup before any cleanup

**Recommendation:** Copy `Mount & Blade II Bannerlord\Modules` and the relevant `261550` Workshop subtree (or use Steam’s verify) before bulk deletes.

---

**Status:** Suggestions only. No filesystem changes were made during the audit that produced this file.
