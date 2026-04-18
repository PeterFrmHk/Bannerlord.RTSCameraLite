# Local Bannerlord mod installation audit (read-only)

**Scan date:** 2026-04-18  
**Scope:** Filesystem inspection only. No game, Steam, or Workshop content was modified.

## Paths verified

| Role | Path |
|------|------|
| Game root | `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord` |
| Workshop content (app 261550) | `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550` |
| Steam Workshop metadata | `C:\Program Files (x86)\Steam\steamapps\workshop\appworkshop_261550.acf` |
| Library folders | `C:\Program Files (x86)\Steam\steamapps\libraryfolders.vdf` |

Steam reports Bannerlord (`261550`) installed on the default library at `C:\Program Files (x86)\Steam`. `appworkshop_261550.acf` lists subscribed Workshop items with sizes and `timeupdated` stamps; `NeedsUpdate` / `NeedsDownload` were `0` at read time.

## Methodology

1. **Local modules:** Enumerate each child folder of `GameRoot\Modules` that contains `SubModule.xml` at the **module root** (not nested paths such as `bin\...\SubModule.xml`).
2. **Workshop modules:** Enumerate each numeric Workshop item folder under `content\261550` that contains `SubModule.xml` at the **root of that item id folder**.
3. **Parse:** For each module, read `SubModule.xml` for `<Name>`, `<Id>`, `<Version>`, first `<SubModule>` block’s `<DLLName>` / `<SubModuleClassType>`, `<DependedModules>`, and `<IncompatibleModules>` (when present).
4. **Binary check:** If a `DLLName` is declared, test existence at `bin\Win64_Shipping_Client\<DLLName>`.
5. **Duplicates:** Compare `ModuleId` and `DisplayName` across all discovered rows (see manifest CSV).

Nested Workshop layouts (mod packaged one folder deep) would **not** appear as separate launcher entries unless `SubModule.xml` sits at the Workshop id root; this scan matches what the stock launcher typically indexes.

## Summary counts

| Metric | Value |
|--------|-------|
| Modules in `GameRoot\Modules` with root `SubModule.xml` | 10 |
| Workshop item folders with root `SubModule.xml` | 23 |
| **Total manifest rows** | **33** |
| Workshop top-level folders (any content) | 25 |
| Workshop folders **without** root `SubModule.xml` | 2 (orphan/junk; see below) |
| Duplicate `ModuleId` across all sources | **0** |
| Duplicate `DisplayName` | **0** |

## Orphan Workshop folders (not modules)

These directories sit under `content\261550` but contain **no** `SubModule.xml`. They are **not** listed in `appworkshop_261550.acf` and look like leftover download/cache debris. They should not appear as mods in the launcher.

| Workshop folder | Contents observed |
|------------------|-------------------|
| `2875093027` | Single file `DismembermentPlus.json` (~708 bytes) |
| `3302249842` | Single file `rca_log.txt` |

## Reference: Bannerlord.Harmony (Workshop `2859188632`)

Harmony is the standard patch framework used by many BLSE / BUTR ecosystem mods.

- **SubModule.xml path:** `...\261550\2859188632\SubModule.xml`
- **Declared Id:** `Bannerlord.Harmony`
- **Display name:** `Harmony`
- **Version (XML):** `v2.4.2.0`
- **Managed entry DLL:** `Bannerlord.Harmony.dll` under `bin\Win64_Shipping_Client\`
- **SubModuleClassType:** `Bannerlord.Harmony.SubModule`
- **Extra assemblies:** `0Harmony.dll`, Mono.Cecil*, MonoMod.* (listed under `<Assemblies>` in XML)
- **Hard dependencies (`DependedModules`):** empty
- **Load hints:** `ModulesToLoadAfterThis` lists Native, SandBoxCore, Sandbox, StoryMode, CustomBattle; `DependedModuleMetadatas` mark those as optional `LoadAfterThis` with version wildcards

**Implication:** Other mods usually reference Harmony via `DependedModuleMetadata` (`LoadBeforeThis`) or explicit `<DependedModule Id="Bannerlord.Harmony" .../>`. Harmony is **only** present under Workshop on this machine, not under `GameRoot\Modules`.

## Framework stack visible on disk

These Workshop ids were present with valid root XML and expected primary DLLs in `bin\Win64_Shipping_Client` (manifest CSV has details):

| Module Id | Workshop id | Notes |
|-----------|-------------|--------|
| `Bannerlord.Harmony` | 2859188632 | Reference baseline |
| `Bannerlord.UIExtenderEx` | 2859222409 | Depends on Harmony |
| `Bannerlord.ButterLib` | 2859232415 | Depends on Harmony |
| `Bannerlord.MBOptionScreen` (MCM v5) | 2859238197 | Multiple `<SubModule>` entries; primary loader DLLs include `MCMv5.dll`, `Bannerlord.ModuleLoader.Bannerlord.MBOptionScreen.dll`, and versioned `Bannerlord.MBOptionScreen.v*.dll` files |

Mods that **require** these for runtime include (non-exhaustive): Retinues, Better Time, Unblockable Thrust, Clan Armies, Army Fleets, Party AI Controls — see CSV `DependedModules` column.

## Suspect families (requested)

### RTS Commander Doctrine / `Bannerlord.RTSCameraLite` (local manual)

- **Path:** `...\Bannerlord\Modules\Bannerlord.RTSCameraLite`
- **Id:** `Bannerlord.RTSCameraLite` — **not** the same id as Workshop `RTSCamera` or `RTSCamera.CommandSystem`.
- **Dependencies:** Only Native, SandBoxCore, Sandbox, StoryMode, CustomBattle (no Harmony / MCM declared).
- **DLL:** `Bannerlord.RTSCameraLite.dll` — present under `bin\Win64_Shipping_Client` per scan.

### `RTSCamera` (Workshop `3596692403`)

- **DLL:** `RTSCamera.dll` present.
- **Official deps:** `DependedModules` target game modules at `v1.3.13` while local Native is `v1.3.15` — typical **version skew warning** in the launcher.
- **Harmony:** `DependedModuleMetadata` requires `Bannerlord.Harmony` **LoadBeforeThis** (soft ordering). Harmony must be enabled and ordered above for intended behavior.

### `RTSCamera.CommandSystem` (Workshop `3596693285`)

- Same pattern as RTSCamera: `v1.3.13` dependency block vs `v1.3.15` game, plus Harmony metadata ordering.

### RBM / `RBM` (Workshop `2859251492`)

- **DLL:** `RBM.dll` present.
- **Deps:** Native / SandBoxCore / Sandbox / StoryMode / CustomBattle at `v1.3.14`; optional `BirthAndDeath`; optional `RTSCamera` and `RTSCamera.CommandSystem`.
- **Version skew:** Declared `v1.3.14` vs installed `v1.3.15` — another **likely launcher warning** source.

### `RBM_WS` War Sails submod (Workshop `3635788184`)

- **SubModules:** **Empty** in XML — **no managed DLL**; this is an **XML/data** extension that depends on RBM, NavalDLC, etc.
- **Launcher behavior:** Some UIs still show an icon for “incomplete” or “data-only” packs; this is not necessarily a broken install.

### True Levies (Workshop `3483463349`)

- **Id:** `True Levies` (includes a space).
- **SubModules:** Empty — troop/party XML only. No `bin` requirement for managed code.

### FastMode / NavalDLC (official, local)

- Under `GameRoot\Modules`; standard TaleWorlds modules with DLLs and expected layout.

## SandBoxCore and “missing DLL” in the manifest

`SandBoxCore` has `<SubModules />` **empty** — it is an **official XML/data** module with no managed `DLLName`. The manifest correctly shows no primary DLL; this is **not** an error.

## Incomplete or unusual layouts (from manifest)

Rows where `HasDeclaredDllName` is false or `DllExistsExpected` is false fall into:

- **Official XML-only** (SandBoxCore).
- **Community XML-only** (Open Source*, True Levies, RBM_WS, Suteki Women Mod, swadian armoury in some layouts).
- **Parser limitation:** Only the **first** `<SubModule>` `DLLName` is summarized in the CSV; MCM and ButterLib declare **multiple** submodules — additional DLLs exist on disk.

## Duplicate installation analysis

- **No** second copy of `RTSCamera` or `RTSCamera.CommandSystem` under `GameRoot\Modules` was found; only Workshop copies.
- **No** duplicate `ModuleId` between manual and Workshop in this scan.
- Local `Bannerlord.RTSCameraLite` is a **different product id** from `RTSCamera`; they do not duplicate each other.

## Launcher warning / error icons — likely causes (hypotheses)

The native launcher often marks mods when:

1. **Game vs mod `DependentVersion` mismatch** (e.g. mods declaring `v1.3.13` or `v1.3.14` while Native is `v1.3.15`).
2. **Optional or metadata dependencies** not satisfied in the active load list (e.g. Harmony load order when using BLSE).
3. **Data-only** or **multi-DLL** modules where the UI heuristic flags nonstandard layouts.

Given this install, **RTSCamera**, **RTSCamera.CommandSystem**, and **RBM** are strong candidates for **version-surface warnings** independent of missing files. **RBM_WS** may trigger **“no DLL”** style hints. **Bannerlord.RTSCameraLite** has matching DLL on disk; if it still shows an icon, check **load order**, **BLSE** console output, or a **stricter** dependency checker than this filesystem pass.

## Artifacts

- Machine-readable table: `docs/research/local-bannerlord-mod-manifest.csv`
- Recommended cleanup (not executed): `docs/research/local-bannerlord-mod-prune-plan.md`
