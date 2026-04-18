# Installed Bannerlord mods — reference pattern scan (research only)

**Scope:** Summarize **patterns** from locally installed **Official Modules** and **Steam Workshop** items. **No code was copied** from other mods into this repository. **No `src/` edits** are part of Slice 0. DLL-only mods are listed from **`SubModule.xml` + file layout** only; internal logic is **UNCERTAIN** unless decompiled (not done here).

**Paths scanned**

| Path | Purpose |
| --- | --- |
| `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\` | Official + sideloaded modules |
| `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\` | Workshop Bannerlord (`AppId` **261550**) |

**Counts (2026-04-17):** **10** module directories under `Modules\`; **25** numeric workshop folders. **2** workshop folders lacked a root `SubModule.xml` (`2875093027`, `3302249842`) — **skipped** (partial download or nested layout).

**Tooling note:** Reflection on third-party DLLs was **not** relied on for this refresh; parity with [`base-game-camera-scan.md`](base-game-camera-scan.md) uses **TaleWorlds** assemblies only.

---

## 1. Official `Modules\` folder (pattern relevance)

| Module (folder) | Typical role | RTS / orders / camera relevance |
| --- | --- | --- |
| `Native` | Core game + **`TaleWorlds.MountAndBlade.View.dll`** host | **Source of truth** for engine hooks (`MissionView`, `MissionScreen`). |
| `SandBox`, `SandBoxCore`, `StoryMode`, `CustomBattle`, `Multiplayer` | Campaign / modes | Mission variants — test RTS hooks per mode (**UNCERTAIN** coverage until played). |
| `NavalDLC` | Expansion | Naval missions may alter camera/order UI — **UNCERTAIN** risk surface. |
| `BannerlordRTSCamera` | Community RTS camera (if sideloaded) | **High** conceptual overlap; treat as **pattern reference**, not a dependency. |
| `BirthAndDeath`, `FastMode` | Gameplay toggles | Low direct overlap; still touch missions — regression matrix only. |

---

## 2. Steam Workshop inventory (parseable `SubModule.xml`)

| Mod name (`<Name value="…"/>`) | Workshop folder | Version (`<Version value="…"/>`) | Source / DLL | Relevance |
| --- | --- | --- | --- | --- |
| Harmony | `2859188632` | v2.4.2.0 | Library + `0Harmony.dll` | Patch ecosystem baseline; **conflict risk** if Lite ever adopts Harmony. |
| Mod Configuration Menu v5 | `2859238197` | v5.11.3 | DLL | Options stack; **dependency graph** reference (often Harmony + ButterLib + UIExtenderEx). |
| ButterLib | `2859232415` | v2.10.3 | DLL | Shared utilities; **not** required for Lite doctrine. |
| UIExtenderEx | `2859222409` | v2.13.2 | DLL | VM/UI extension patterns — opposite of minimal RTS core unless product wants overlays. |
| (RBM) Realistic Battle Mod Bannerlord | `2859251492` | v4.2.23 | Large gameplay mod | **High conflict risk** with battle formulas, AI, possibly orders. |
| (RBM WS) Realistic Battle Mod War Sails Submod | `3635788184` | v4.2.23 | Submod | Same family — **compatibility hazard**. |
| RTS Camera | `3596692403` | v5.3.25 | DLL + `GUI` + ships **Harmony** | **Camera + UI stack** reference; **reject** as template for “no Harmony” Lite baseline. |
| RTS Camera Command System | `3596693285` | v5.3.25 | Companion DLL | Suggests **splitting** camera vs. commands commercially — informational only. |
| Party AI Controls | `3620272139` | v1.3.13 | **DLL-only** | AI focus — patterns **unknown** without ILSpy; assume **mission tick** touchpoints. |
| Open Source Armory / Saddlery / Weaponry | `3011479883`, `3010990914`, `3010984416` | various | Asset / DLL mixes | Low direct RTS relevance; keep in **compatibility matrix** only. |
| True Levies | `3483463349` | v2.2.0 | Unknown depth | Troop systems — **UNCERTAIN** mission impact. |
| Retinues | `3599557394` | v1.3.14.27 | Unknown | Party/retinue logic — **UNCERTAIN** battle hooks. |
| Clan Armies | `3618752412` | v1.2.3.0 | Unknown | Strategic layer — likely low camera overlap (**UNCERTAIN**). |
| Army Fleets | `3619429143` | v1.0.0.0 | Unknown | Naval/strategic — **UNCERTAIN** overlap with NavalDLC missions. |
| Bannerlord: Take All Perks | `3617286010` | v1.3.0 | Unknown | Character progression — low battle camera relevance. |
| BannerPigeon | `3617335871` | v1.5.5 | Unknown | Messenger / meta — low relevance. |
| Faster Child Growth | `3618489007` | v1.0.0 | Unknown | Campaign — low relevance. |
| Better Time | `2875960358` | v1.1.0 | Unknown | Time scale — **UNCERTAIN** tick sensitivity. |
| Unblockable Thrust | `3614435151` | v1.1.3.1 | Unknown | Combat — **medium** conflict risk with melee patches. |
| Suteki Women Mod | `2876427251` | v1.7.0 | Unknown | Content pack — **UNCERTAIN** battle scripts. |
| swadian armoury | `2875090166` | v1.0.0 | Unknown | Items — low relevance. |

---

## 3. Pattern matrix (no code)

### 3.1 Camera patterns

| Pattern | Where seen | Mechanism | Lite stance |
| --- | --- | --- | --- |
| `MissionView` + camera override | Workshop **RTS Camera**, many camera mods | Public engine hook family | **Adopt** (matches [`base-game-camera-scan.md`](base-game-camera-scan.md)). |
| `Harmony` + multi-DLL stack | Workshop **RTS Camera** (`SubModule.xml` + `0Harmony.dll`) | Transpilers / prefixes | **Defer / avoid** for Lite core. |
| JSON / light config | `BannerlordRTSCamera` (if present under `Modules`) | File-based settings | **Adopt** philosophy. |

### 3.2 Order / formation patterns

| Pattern | Where seen | Lite stance |
| --- | --- | --- |
| Separate “command” DLL | **RTS Camera Command System** | Informational split only — not a dependency recommendation. |
| Deep AI / battle mutation | **RBM**, **Party AI Controls** (likely) | **Treat as compatibility hazards**; do not mirror without ILSpy + policy. |

### 3.3 Config / dependency patterns

| Pattern | Example | Risk |
| --- | --- | --- |
| `DependedModuleMetadatas` chains | MCM + ButterLib + UIExtenderEx | Load-order fragility |
| `LoadBeforeThis` on Harmony | Harmony module | Many mods assume this — ordering puzzles |

---

## 4. Risk notes (doctrine-relevant)

1. **Parallel RTS products:** Running **Workshop RTS Camera** alongside **`Bannerlord.RTSCameraLite`** risks **double `MissionView` camera overrides** — **BLOCKER**-class player experience if both subscribe.  
2. **RBM-class mods:** Expect **non-public** battle changes; even public RTS orders may **feel** different — test matrix item.  
3. **DLL-only mods:** **UNCERTAIN** patch targets — assume **unknown** `Mission` / `Agent` / `MissionScreen` interactions until ILSpy’d locally.

---

## 5. Recommended use of this scan

- Treat entries as **compatibility / inspiration** only.  
- For any third-party DLL behavior claim, add a **follow-up ILSpy note** in a future slice — **do not** copy decompiled source into the repo.  
- Re-run this inventory when Workshop subscriptions change.
