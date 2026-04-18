# Installed Bannerlord mods — reference pattern scan (research only)

**Scope:** Summarize **patterns** observed in locally installed modules. **No code was copied** into `Bannerlord.RTSCameraLite`, and **no `src/` production files** were modified for this note. Where another mod’s logic is not visible as `.cs`, behavior is labeled **reference-only / DLL** (decompilation would be required for certainty — not performed here beyond dependency and file layout inspection).

**Related base-game research:** [`base-game-camera-scan.md`](base-game-camera-scan.md), [`base-game-order-scan.md`](base-game-order-scan.md).

---

## 1. Scan environment

| Item | Detail |
| --- | --- |
| **Paths scanned** | `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\` ; `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\` |
| **Local workspace (additional)** | `C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\` — **separate** greenfield mod repo (not under Steam `Modules` in this scan); used only as **scope context**, not as an installed Steam Workshop item. |
| **Mods found (Modules folder)** | `BannerlordRTSCamera`, `BirthAndDeath`, `CustomBattle`, `FastMode`, `Multiplayer`, `Native`, `NavalDLC`, `SandBox`, `SandBoxCore`, `StoryMode` |
| **Workshop items** | **25** numeric folders under `261550`; **23** had parseable `SubModule.xml` at folder root; **2** folders had **no** `SubModule.xml` at root (`2875093027`, `3302249842`) — **not inspected** (may be incomplete downloads or nested layouts). |
| **Primary inspection depth** | **Source read:** `Modules\BannerlordRTSCamera\` (C# tree present). **Metadata + binaries:** Workshop **RTS Camera** (`3596692403`, `3596693285`), **MCM** (`2859238197`), **Harmony** (`2859188632`), **Party AI Controls** (`3620272139` — DLL-only). **SubModule-only sample:** **RBM** (`2859251492`), **ButterLib** (`2859232415`), **UIExtenderEx** (`2859222409`) for dependency patterns. |
| **Decompilation** | **Not run** (no ILSpy output in this document). Workshop `RTSCamera.dll` **did not** complete `GetExportedTypes` in a quick reflection probe unless a full game bin dependency chain is preloaded — treat deep type lists as **DLL / decompiler follow-up**. |

---

## 2. Candidate reference mods

| Mod (as labeled) | Path | Version (SubModule) | Source / DLL | Relevant systems | Risk / quality notes |
| --- | --- | --- | --- | --- | --- |
| **Bannerlord RTS Camera** (local Modules clone) | `...\Modules\BannerlordRTSCamera\` | `v0.1.0` | **Full C# source** under `src\` | `MissionView`, `MissionScreen` adapter, input frame, lifecycle `AddMissionBehavior`, **no Harmony** in `START_HERE.md` | **High alignment** with `Bannerlord.RTSCameraLite` goals; **not** a dependency — pattern reference only. |
| **RTS Camera** (Workshop) | `...\261550\3596692403\` | `v5.3.25` | **DLLs:** `RTSCamera.dll`, `MissionLibrary.dll`, `RTSCameraAgentComponent.dll`, ships **`0Harmony.dll`** | RTS camera + Gauntlet `ModuleData` / `GUI`; **Harmony** metadata in `SubModule.xml` | **Heavy stack**; strong candidate for **what not to mirror** if keeping deps minimal. |
| **RTS Camera Command System** | `...\261550\3596693285\` | `v5.3.25` | **DLL** (companion to RTS Camera) | Command system add-on (name only from `SubModule.xml`) | Depends on **Harmony** metadata; **load order** coupling with main RTS Camera mod. |
| **Mod Configuration Menu v5** | `...\261550\2859238197\` | `v5.11.3` | DLL + XML | MCM / options stack | **Harmony + ButterLib + UIExtenderEx** chain in `DependedModules` — useful as **ecosystem** reference, not as a minimal-dependency template. |
| **Harmony** | `...\261550\2859188632\` | `v2.4.2.0` | Library | Transitive for many mods | **Patch conflict** surface scales with number of postfix/prefix patches on same methods. |
| **ButterLib** | `...\261550\2859232415\` | `v2.10.3` | Library | Shared modding utilities | Often paired with MCM / Harmony. |
| **UIExtenderEx** | `...\261550\2859222409\` | `v2.13.2` | Library | UI extension | **Overlay / VM** style extensions — opposite of “no overlay” direction unless justified. |
| **Party AI Controls** | `...\261550\3620272139\` | `v1.3.13` | **DLL only** (`PartyAIUpdated.dll`) | AI / party (name suggests non-camera) | **DLL-only** — patterns require decompile or author docs. |
| **(RBM) Realistic Battle Mod** | `...\261550\2859251492\` | `v4.2.23` | Not deep-read | Large gameplay mutator | **High conflict risk** with battle/camera/order patches — treat as **compatibility hazard** reference. |

---

## 3. Camera reference patterns

| Pattern | Mod | Location (conceptual) | What it does | Mechanism | Adopt / adapt / reject | Reason |
| --- | --- | --- | --- | --- | --- | --- |
| **`MissionView` + `UpdateOverridenCamera`** | **BannerlordRTSCamera** | `RtsCameraMissionView` | When RTS active, applies camera; otherwise returns `false` and restores | **Public** engine APIs (`MissionView`, `MissionScreen`, `CombatCamera`) | **Adopt** | Matches engine-supported override point from [`base-game-camera-scan.md`](base-game-camera-scan.md). |
| **`MissionScreen` as typed adapter** | **BannerlordRTSCamera** | `MissionScreenCameraAdapter` | Reads `CombatCamera`, distance/bearing/elevation; applies pose via **`CustomCamera`** swap; restore sets `CustomCamera` back to `CombatCamera` | **Public** properties on `MissionScreen` (`CombatCamera`, `CustomCamera`, `CameraBearing`, …) | **Adapt** | `Bannerlord.RTSCameraLite` today uses **`object` + reflection** in `CameraBridge`; **typed `MissionScreen`** adapter reduces fragility **if** project references `MountAndBlade.View.dll` consistently. |
| **Operational gating** | **BannerlordRTSCamera** | `RtsCameraMissionView.IsOperational` | Skips RTS when photo mode / conversation | **Public** `MissionScreen` flags | **Adopt** | Prevents fighting cinematic/UI camera modes. |
| **Harmony + MissionLibrary** | **Workshop RTS Camera** | `SubModule.xml` + `bin` | Ships Harmony and extra DLLs | **Harmony** + unknown internals | **Reject for Lite baseline** | Keeps dependency and patch surface small; **reference-only** for feature ideas. |

---

## 4. Input reference patterns

| Pattern | Mod | Location | What it does | Mechanism | Adopt / adapt / reject | Reason |
| --- | --- | --- | --- | --- | --- | --- |
| **`IInputContext` polling** | **BannerlordRTSCamera** | `BannerlordInputSource.Read` | Maps WASD/Q/E/F10/shift/ctrl/scroll to a small immutable frame | **Public** `Input.IsKeyPressed`, `IsKeyDown`, `GetDeltaMouseScroll` | **Adopt** | Same family as base-game `IInputContext` scan. |
| **Router split** | **BannerlordRTSCamera** | `RtsInputRouter` | Separates “RTS enabled” vs disabled routing | Pure C# | **Adapt** | Useful boundary; Lite already has `RTSCameraInput` / guard patterns — keep **one** routing choke point. |

---

## 5. Formation / order reference patterns

| Pattern | Mod | Evidence | Mechanism | Adopt / adapt / reject | Reason |
| --- | --- | --- | --- | --- | --- |
| **Companion command DLL** | **RTS Camera Command System** | `SubModule.xml` names `RTSCamera.CommandSystem.dll` | **DLL** (contents not decompiled here) | **Reference-only** | Suggests **splitting** camera vs. commands across modules is a **product** pattern, not a **Lite** requirement. |
| **Battle / AI DLL mods** | **Party AI Controls**, **RBM** | DLL-only or large gameplay | **Likely Harmony / deep patches** | **Reject as templates** | High **conflict** risk with mission tick, AI, and damage formulas. |

**Lite repo alignment:** `Bannerlord.RTSCameraLite` already documents **public `OrderController`** issuance in [`native-order-hooks.md`](native-order-hooks.md) — no installed mod was needed to validate that path.

---

## 6. UI / marker / config reference patterns

| Pattern | Mod | Evidence | Mechanism | Adopt / adapt / reject | Reason |
| --- | --- | --- | --- | --- | --- |
| **No MCM / no ButterLib** | **BannerlordRTSCamera** | `START_HERE.md` | JSON or embedded settings (**verify in repo**) | **Adopt philosophy** | Matches Lite’s “small JSON config” direction. |
| **MCM + UIExtenderEx stack** | **MCM v5** | `SubModule.xml` dependencies | Community UI stack | **Reject for Lite core** | Large dependency graph; use only if product explicitly approves MCM. |
| **Gauntlet / `GUI` folders** | **Workshop RTS Camera** | `GUI` directory present | UI assets | **Reference-only** | Tactical overlay class features live here in many mods — **not** Lite slice-13 direction. |

---

## 7. Compatibility lessons

| Topic | Observation |
| --- | --- |
| **Dependencies** | Workshop **RTS Camera** declares **`Bannerlord.Harmony` LoadBeforeThis** and ships **`MissionLibrary.dll`**. **MCM** pulls **Harmony + ButterLib + UIExtenderEx**. |
| **Common patch targets** | Mods that patch **`Mission`**, **`Agent`**, **`MissionAgent`**, **`DefaultCombatEngine`**, or **`MissionScreen`** tick paths are likely to **interact** with camera override and input guards. |
| **Conflict zones** | **RBM**-class mods (battle formulas), **RTS Camera** (camera + orders), **MCM-heavy UI** (Gauntlet layers) — all increase **order-of-execution** sensitivity. |
| **Load order** | `DependedModuleMetadata` with **`LoadBeforeThis` / `LoadAfterThis`** appears in multiple `SubModule.xml` files — **Lite** should keep dependencies minimal so players have fewer ordering puzzles. |
| **Parallel RTS camera products** | Having **Workshop RTS Camera** and **`Modules\BannerlordRTSCamera`** and **`Bannerlord.RTSCameraLite`** installed together is **probably undesirable** — expect **double camera hooks** if more than one `MissionView` overrides the camera. |

---

## 8. Recommended patterns for `Bannerlord.RTSCameraLite`

| Category | Safe to adopt (conceptually) | Avoid / defer | APIs to keep verifying locally |
| --- | --- | --- | --- |
| **Camera** | `MissionView.UpdateOverridenCamera`; gate on **photo mode / conversation**; **restore** on deactivate/finalize; prefer **typed `MissionScreen`** adapter over raw reflection when reference assemblies allow | Shipping **Harmony** for camera unless public hook disappears | `MissionScreen.CustomCamera` vs `CombatCamera` semantics on **your** game version |
| **Input** | Thin wrapper over **`IInputContext`**; single router; configurable keys | Hard-coded overlap with native battle keys without ownership policy | `Input` vs `DebugInput` on `MissionBehavior` |
| **Orders** | Keep issuance in **one** executor using **public** `OrderController` / `WorldPosition` | Copying DLL-only mods without ILSpy verification | `Formation` ownership fields (`Team` vs `PlayerOwner` discrepancy on 1.3.x — see [`base-game-order-scan.md`](base-game-order-scan.md)) |
| **UI / markers** | Throttled `InformationMessage` + optional **public** particle burst | Pulling **UIExtenderEx** / MCM unless approved | `Mission.AddParticleSystemBurstByName` string names per build |
| **Dependencies** | **Native + SandBoxCore** (as Lite already does) | **Harmony / MCM / ButterLib** as mandatory deps for v1 | Match **Native** version in `SubModule.xml` to player build |

**Minimum dependency recommendation (Lite):** stay on **public engine APIs** + **optional JSON**; treat **Workshop RTS Camera** and similar stacks as **behavioral references**, not libraries — **do not add** another mod as a NuGet/assembly dependency unless explicitly approved.

---

## 9. Follow-ups (optional)

1. If **Workshop RTS Camera** patterns are needed in detail, run **ILSpy** locally on `RTSCamera.dll` with the **game’s `bin`** as assembly resolution path (**reference-only** notes).  
2. Resolve the two Workshop folders **without** root `SubModule.xml` or document them as abandoned/partial.  
3. Keep this file updated when the machine’s Workshop subscription set changes.
