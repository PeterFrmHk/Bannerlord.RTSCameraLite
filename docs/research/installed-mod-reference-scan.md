# Installed mod reference scan — research only

**Scope:** Read-only inspection of paths on the machine used for this scan. **No production mod files were modified.** **No third-party source or decompiled bodies were copied** into `Bannerlord.RTSCameraLite`. Patterns below are **summaries** for orientation; treat workshop DLL string hits as **heuristic** until verified in ILSpy/dnSpy.

**Scan date:** 2026-02-02

---

## 1. Scan environment

### Paths scanned

| Location | Path |
|----------|------|
| Game Modules | `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\` |
| Steam Workshop (App 261550) | `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\` |
| Local dev repo (not under Steam Modules) | `C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\` — **not** a duplicate of the Steam `Modules` tree; used only to compare **SubModule** / stated dependencies vs installed ecosystem |

### Mods found

**`Modules\` (11 folder roots):** `BannerlordRTSCamera`, `BirthAndDeath`, `CustomBattle`, `FastMode`, `Multiplayer`, `Native`, `NavalDLC`, `SandBox`, `SandBoxCore`, `StoryMode` — plus vanilla-style modules as expected.

**Workshop `261550\`:** **25** numeric item folders.

**SubModule.xml resolution:** **22** workshop items contained a discoverable `SubModule.xml` (recursive search). **3** folders had **no** `SubModule.xml` found in this pass: `2875093027`, `2876427251`, `3302249842` — treat as **uninspected / incomplete layout** until manually checked.

### Which mods were inspected

| Mod / item | Inspection depth |
|------------|------------------|
| **RTS Camera** (Workshop `3596692403`) | `SubModule.xml`, `RTSCamera.dll` **string token scan** (ASCII), folder layout |
| **RTS Camera Command System** (`3596693285`) | `SubModule.xml` only |
| **Harmony, ButterLib, UIExtenderEx, MCM v5** (Workshop `2859188632`, `2859232415`, `2859222409`, `2859238197`) | `SubModule.xml` (dependencies + versions) |
| **Bannerlord RTS Camera** under **`Modules\BannerlordRTSCamera`** | `SubModule.xml`, **readable `.cs` sources** (small partial reads + ripgrep for API tokens), **not** the same tree as `Bannerlord.RTSCameraLite` repo path |
| **Party AI Controls** (`3620272139`) | `SubModule.xml`, `PartyAIUpdated.dll` **string token scan** |
| **RBM / RBM_WS** (`2859251492`, `3635788184`) | `SubModule.xml` headers only (IDs/names) |
| Other workshop items (weapon/armor/QoL mods) | **SubModule.xml** grep list only — **not** deep-dived (low relevance to RTS camera slice) |

### Source vs DLL

| Artifact | Method |
|----------|--------|
| `Modules\BannerlordRTSCamera\src\**\*.cs` | **Direct source read** (patterns only) |
| `RTSCamera.dll`, `PartyAIUpdated.dll` | **No decompiler** run in this session; **embedded ASCII string** presence checks for coarse signals (`Harmony`, `MissionView`, etc.) — **reference-only**, not proof of control flow |
| **Decompiled code** | **Not produced** here; any future ILSpy work must stay **local notes** — do not paste licensed third-party decompilation into this repo |

---

## 2. Candidate reference mods

| Mod name | Path | Version (from XML) | Source / DLL | Relevant systems (evidence) | Risk / quality notes |
|----------|------|---------------------|--------------|-----------------------------|------------------------|
| **RTS Camera** | `...\261550\3596692403\` | v5.3.25 | **DLL** (`RTSCamera.dll`, `RTSCameraAgentComponent.dll`) | Workshop XML declares **Harmony** metadata `LoadBeforeThis`; string scan shows **MissionView**, **UpdateOverridenCamera**, **OrderController**, **SetOrder**, **Gauntlet**, **MissionLibrary**, **Harmony**, **Patch** | **High complexity** vs Lite goals; **license** not inspected in this scan; **do not treat as dependency** |
| **RTS Camera Command System** | `...\261550\3596693285\` | v5.3.25 | **DLL** | Same Harmony metadata pattern; separate module id `RTSCamera.CommandSystem` | Loads alongside base RTS Camera; **order/UI** surface likely large |
| **Harmony** | `...\261550\2859188632\` | per XML | DLL module | Ecosystem baseline for many mods | **Patch conflict** hub if Lite ever adds Harmony |
| **ButterLib** | `...\261550\2859232415\` | per XML | DLL module | Hotkeys / helpers | Common **MCM** companion |
| **UIExtenderEx** | `...\261550\2859222409\` | v2.13.2 (sample) | DLL module | Depends on **Bannerlord.Harmony** in XML | **Gauntlet** extension stack |
| **MCM v5** | `...\261550\2859238197\` | v5.11.3 | DLL module | Depends **Harmony**, **ButterLib**, **UIExtenderEx** (explicit in `SubModule.xml`) | **Heavy dependency fan-out** for any mod that adopts MCM as shown |
| **Bannerlord RTS Camera** (Modules folder) | `...\Modules\BannerlordRTSCamera\` | v0.1.0 (XML) | **Full source + solution** present under game Modules (unusual layout) | `RtsCameraMissionView : MissionView`, `UpdateOverridenCamera` override; `MissionScreenCameraAdapter` uses **typed** `MissionScreen` / `CombatCamera` public surface | **Not** automatically the same as `Bannerlord.RTSCameraLite` git tree — avoid accidental **cross-copy**; good **local** contrast for “public `MissionScreen` vs reflection bridge” approaches |
| **Party AI Controls** | `...\261550\3620272139\` | v1.3.13 | `PartyAIUpdated.dll` | XML: depends **Harmony** + **UIExtenderEx**; strings: **Formation**, **Harmony**, **Patch**; no `OrderController` token hit | **Campaign / AI** tilt; **UIExtenderEx** coupling — not a Lite v0.1 model |
| **RBM / RBM WS** | `2859251492` / `3635788184` | per XML | Presumed DLL-heavy | Large combat overhaul ecosystem | **High** Harmony surface; **conflict** risk with anything touching combat AI, damage, formations |

---

## 3. Camera reference patterns

| Mod | Location (concept) | What it does (summary) | Public API / Harmony / reflection / UI | Adopt / adapt / reject | Reason |
|-----|---------------------|-------------------------|------------------------------------------|-------------------------|--------|
| **RTS Camera** (Workshop DLL) | *Decompilation not run* — inferred from strings + public product knowledge | Free camera + battle commanding stack | **Harmony** + **MissionView** string evidence; **MissionLibrary** / **Gauntlet** suggest UI extensions | **Reject** as dependency for **RTSCameraLite**; **adapt** only as “what the ecosystem ships” | Matches **public-reference-scan.md**: Lite avoids Harmony stack |
| **Modules `BannerlordRTSCamera`** | `RtsCameraMissionView` (`MissionView` subclass) | Adds mission view, wires tick path, overrides **camera update** hook | **Public** `MissionView` / `MissionScreen` usage in `MissionScreenCameraAdapter` (typed `CombatCamera`, bearing/elevation/distance from screen) | **Adapt idea** (“MissionView owns camera frame”) **verify** against your locked game version | Strong typing may **differ** by Bannerlord branch vs Lite’s reflection `CameraBridge` — **ILSpy on game DLLs** still required |
| **Modules `BannerlordRTSCamera`** | `MissionScreenCameraAdapter` | Captures state from `MissionScreen`, applies pose via engine camera clone pattern | **Public engine/view** types | **Adapt** pattern naming (adapter boundary) | Aligns with Lite’s **adapter** concept; implementation details must stay version-specific |

**String-scan disclaimer (RTSCamera.dll):** Presence of `UpdateOverridenCamera` in the binary does **not** prove it is the *only* camera path; only that the symbol text appears (obfuscation absent for that string).

---

## 4. Input reference patterns

| Mod | Location | What it does (summary) | Mechanism | Adopt / adapt / reject | Reason |
|-----|----------|-------------------------|-----------|-------------------------|--------|
| **Modules `BannerlordRTSCamera`** | `BannerlordInputSource`, `RtsInputRouter` (file names from tree listing) | Centralizes reading native input into mod contracts | Appears **public input** APIs (not inspected line-by-line here) | **Adapt** “single input adapter” shape | Matches Lite goal of isolating input; verify suppression limits per `slice-6-audit.md` |
| **RTS Camera** (Workshop) | *Not decompiled* | Historically integrates with **MissionLibrary** / hotkey stacks (per XML `GameText` paths) | Likely **Harmony** + framework hooks | **Reject** copying | Couples to ecosystem Lite does not want for v0.1 |

---

## 5. Formation / order reference patterns

| Mod | Location | What it does (summary) | Mechanism | Adopt / adapt / reject | Reason |
|-----|----------|-------------------------|-----------|-------------------------|--------|
| **RTS Camera** (Workshop DLL) | *Strings only* | References **OrderController** / **SetOrder** text present | Unknown call sites without decompile | **Reject** as template | Lite already targets **public** `OrderController` path in `native-order-hooks.md` — **verify locally**, do not mirror RTS Camera IL |
| **RTS Camera Command System** | Separate module id | Companion for command UX / issuance | DLL + Harmony metadata | **Reject** dependency | Same as above |
| **Party AI Controls** | DLL strings | **Formation** token present | **Harmony**/**Patch** suggested | **Reject** for Lite v0.1 | Campaign AI domain; not a clean order-execution reference |

---

## 6. UI / marker / config reference patterns

| Mod | Pattern | Mechanism | Adopt / adapt / reject |
|-----|---------|-----------|-------------------------|
| **RTS Camera** | `Gauntlet` string in DLL | UI layer / Gauntlet stack | **Reject** for Lite slice 8 non-goals |
| **RTS Camera** | `MissionLibrary` XML nodes in `SubModule.xml` | Text / config integration | **Reject** — different config pipeline than Lite JSON |
| **MCM v5** | `SubModule.xml` chains **Harmony → ButterLib → UIExtenderEx** | Standard MCM stack | **Later** optional — see `public-reference-scan.md` |
| **Lite repo** (`Bannerlord.RTSCameraLite`) | `config\rts_camera_lite.json` | JSON config | **Adopt** (already project direction) |

**Tactical markers:** No dedicated marker mod was deep-scanned in workshop set; RTS Camera’s Gauntlet string suggests **UI-heavy** approach — **not** Lite’s first slice.

---

## 7. Compatibility lessons

### Dependencies observed

- **RTS Camera stack:** `Bannerlord.Harmony` metadata `LoadBeforeThis` on both RTS modules sampled; depends on Native/Sandbox/StoryMode **1.3.13** line in XML.
- **MCM v5:** explicit triangular dependency on **Harmony + ButterLib + UIExtenderEx** (versions pinned in XML excerpt).
- **UIExtenderEx:** depends on **Bannerlord.Harmony** per its `SubModule.xml`.

### Common patch targets (inferred ecosystem risk)

- Anything **Harmony-heavy** (RBM family, RTS Camera, Party AI) increases chance of **shared method prefixes** on mission tick, AI tick, or UI view models — **not enumerated** here without patch table export.

### Likely conflict zones for `Bannerlord.RTSCameraLite`

| Zone | Cause | Mitigation |
|------|-------|------------|
| **Camera ownership** | Another `MissionView` returning `true` from **UpdateOverridenCamera** in the same mission | Document **priority** / `ViewOrderPriority` policy; test with RTS Camera disabled |
| **Harmony ordering** | Lite uses **no Harmony** today — third-party patches may still alter same public methods Lite calls | Prefer **narrow public surface** + version lock; avoid private patches in Lite |
| **Input keys** | Overlapping default binds with RTS Camera / MCM / ButterLib hotkeys | Keep Lite keys configurable; document clashes in manual checklist |

### Load order

- Workshop modules commonly use **`DependedModuleMetadatas`** with `LoadBeforeThis` / `LoadAfterThis` for Harmony and Native — Lite’s `SubModule.xml` is **minimal** (no Harmony) — **advantage**: fewer ordering constraints, but also **no** automatic ordering vs Harmony mods unless you add metadata later.

---

## 8. Recommended code patterns for `Bannerlord.RTSCameraLite`

### Safe patterns to adopt (aligned with installed + repo research)

1. **`MissionView` subclass** as the mission hook host (matches Modules `BannerlordRTSCamera` and official extension point).
2. **Explicit adapter type** at the `MissionScreen` / camera boundary (name mirrors `MissionScreenCameraAdapter` — concept only, do not copy source).
3. **`OrderController` public issuance** with selection snapshot/restore (already in Lite research `native-order-hooks.md`) — **preferred** over copying workshop IL.
4. **JSON config** in module folder — avoids MCM dependency fan-out seen in installed MCM.

### Patterns to avoid

1. **Taking a Harmony dependency** “because RTS Camera does” — contradicts Lite hard gates unless scope changes.
2. **Importing MissionLibrary / Gauntlet paths** from RTS Camera XML as a template — different product architecture.
3. **Trusting DLL string hits** as specification — always **ILSpy local game DLLs** + Lite `docs/version-lock.md`.

### APIs to verify locally

- `MissionView.UpdateOverridenCamera` spelling and return contract on **your** game branch.
- `MissionScreen` members used by any typed adapter (`CombatCamera`, `CameraBearing`, etc.) vs Lite’s reflection allowlist — **one** approach should be chosen per version matrix, not mixed blindly.
- `OrderController.SetOrder` / `SetOrderWithPosition` and `WorldPosition` ctor — already in `native-order-hooks.md`; re-verify on upgrade.

### Minimum dependency recommendation

- **Ship path:** remain **Native + SandBoxCore + …** only (as Lite `SubModule.xml` today) for v0.1.
- **Player machine:** expect coexistence with **Harmony / MCM / RTS Camera** — document **incompatibility testing** (manual checklist) rather than adding deps to Lite.

---

## Scan gaps / follow-ups

1. **Decompilation:** Not performed; to respect licensing and this task, any deep inspection of `RTSCamera.dll` should be done **locally** by the owner with ILSpy, notes only.
2. **Three workshop folders** without `SubModule.xml` in search — manual cleanup or re-download check.
3. **Relationship** between `Modules\BannerlordRTSCamera` source tree and `Bannerlord.RTSCameraLite` git repo — **clarify internally** to prevent editing the wrong tree when iterating.

---

## Document history

| Date | Note |
|------|------|
| 2026-02-02 | Initial installed-mod scan from Steam `Modules` + Workshop `261550` + string heuristics on selected DLLs. |
