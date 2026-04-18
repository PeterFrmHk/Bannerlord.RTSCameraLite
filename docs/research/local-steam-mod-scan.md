# Local Steam mod scan — read-only (v1)

**Purpose:** Record **normalized** findings from a read-only inspection of a local **Steam** Mount & Blade II Bannerlord installation used to validate expected **`Modules/`** layout. No private account data; paths are typical Steam defaults.

**Scan date:** 2026-04-18

---

## Resolution method

1. Default Steam game root candidate:  
   `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord`
2. Existence checks: **`Modules`** subdirectory present.
3. **`libraryfolders.vdf`**: present under `C:\Program Files (x86)\Steam\steamapps\` (used by scripts for multi-library discovery; contents not pasted here).

---

## Game root detected

- **Status:** FOUND (default Steam path on scan machine)
- **Contains:** `Modules\`, `bin\`, game binaries (read-only listing only)

---

## `Modules/` subfolders observed (names only)

Official / first-party style modules detected (illustrative):

- `Native`
- `SandBoxCore`
- `Sandbox`
- `StoryMode`
- `CustomBattle`
- `Multiplayer`
- `BirthAndDeath`
- `NavalDLC`
- `FastMode`

Third-party / local deploy:

- **`Bannerlord.RTSCameraLite`** (this mod — present from prior manual or test deploy)

**Note:** Naming alone does not prove origin; treat as **folder inventory**, not endorsement.

---

## Official `SubModule.xml` spot checks (read-only)

For each of the following, **`SubModule.xml` exists at module root** (path pattern  
`<GameRoot>\Modules\<Id>\SubModule.xml`):

- `Native`
- `SandBoxCore`
- `Sandbox`
- `StoryMode`
- `CustomBattle`

---

## Comparison to this repo’s expected layout

| Expected | Role |
| --- | --- |
| `Modules/Bannerlord.RTSCameraLite/SubModule.xml` | Launcher metadata |
| `Modules/Bannerlord.RTSCameraLite/config/commander_config.json` | Shipped defaults |
| `Modules/Bannerlord.RTSCameraLite/bin/Win64_Shipping_Client/*.dll` | Mod + NuGet deps only |

Aligned with **`Native`** and other modules using **`bin/Win64_Shipping_Client`** for managed DLLs.

---

*End of local steam mod scan v1.*
