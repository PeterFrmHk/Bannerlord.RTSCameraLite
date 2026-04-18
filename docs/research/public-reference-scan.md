# Public reference scan — Bannerlord.RTSCameraLite

**Purpose:** Catalog public examples and documentation relevant to RTS/free camera, battle camera, mission input, formation orders, order execution, tactical UI/markers, config systems, and Harmony patterns. **No external code was copied into this mod.** Use this file for orientation only; **local game assemblies + official apidoc remain authoritative** (see `docs/version-lock.md`, `docs/research/camera-hooks.md`, `docs/research/native-order-hooks.md`).

**Date accessed (session):** 2026-02-02 (all “accessed” dates below refer to this research pass unless noted).

---

## 1. Sources searched

| Source | URL | Date accessed | Relevance | Reliability |
|--------|-----|---------------|-----------|-------------|
| GitHub — lzh-mb-mod/RTSCamera | https://github.com/lzh-mb-mod/RTSCamera | 2026-02-02 | **High** — flagship RTS/free camera + command features; architectural reference (not copy source). | **Medium–high** for patterns; verify per-branch and game version. License/README in repo. |
| GitHub — Norbivar/RTSCamera | https://github.com/Norbivar/RTSCamera | 2026-02-02 | **High** — maintained fork/releases; same domain as above. | Same as above; compare with upstream. |
| Nexus — RTS Camera (classic id) | https://www.nexusmods.com/mountandblade2bannerlord/mods/355 | 2026-02-02 | **Medium** — user-facing description, dependencies, version notes. | **Medium** — Nexus metadata can lag; cross-check GitHub. |
| TaleWorlds — Official modding docs | https://moddocs.bannerlord.com/ | 2026-02-02 | **High** — official concepts, workflows. | **High** for process; may trail latest patch APIs. |
| TaleWorlds — Official API browser | https://apidoc.bannerlord.com/ | 2026-02-02 | **High** — versioned API surface (e.g. 1.2.x trees). | **High**; still verify against **your** `Win64_Shipping_Client` DLLs. |
| TaleWorlds — Documentations repo | https://github.com/TaleWorlds/Documentations | 2026-02-02 | **Medium** — source behind moddocs; contribution hub. | **High** as official channel. |
| BUTR — API mirror | https://bannerlordapi.butr.link/ | 2026-02-02 | **High** — navigable `MissionView`, namespaces. | **Medium–high** — community mirror; cross-check apidoc + ILSpy. |
| Bannerlord-Modding — Documentation | https://github.com/Bannerlord-Modding/Documentation | 2026-02-02 | **Medium** — tutorials, Harmony notes, wiki links. | **Medium** — community; spot-check against engine. |
| docs.bannerlordmodding.com | e.g. MissionView / MissionBehaviour pages | 2026-02-02 | **Medium** — narrative + API excerpts. | **Medium**; **stale risk** on edge APIs — confirm with ILSpy. |
| docs.bannerlordmodding.lt | Harmony, UIExtenderEx, etc. | 2026-02-02 | **Medium** — practical guides. | **Medium**; Lithuanian community mirror — verify dates on articles. |
| BUTR org | https://github.com/BUTR | 2026-02-02 | **High** — ButterLib, UIExtenderEx, MCM ecosystem, BLSE, templates. | **High** for ecosystem facts; versions change often. |
| Nexus — ButterLib | https://www.nexusmods.com/mountandblade2bannerlord/mods/2018 | 2026-02-02 | **Medium** — dependency hub for many mods. | **Medium** — use GitHub + docs for implementation truth. |
| Nexus — UIExtenderEx | https://www.nexusmods.com/mountandblade2bannerlord/mods/2102 | 2026-02-02 | **Medium** — distribution + compatibility chatter. | **Medium** |
| Nexus — BLSE | https://www.nexusmods.com/mountandblade2bannerlord/mods/1 (files tab often used) | 2026-02-02 | **Medium** — launcher/extender role, user expectations. | **Medium** — confirm version on BUTR GitHub releases. |
| Aragas — MCM (MBOptionScreen) | https://github.com/Aragas/Bannerlord.MBOptionScreen | 2026-02-02 | **High** — MCM patterns, docs, NuGet. | **High**; check license (repo). |
| BUTR — Bannerlord.Harmony | https://github.com/BUTR/Bannerlord.Harmony | 2026-02-02 | **High** — how Harmony is shipped for Bannerlord mods. | **High** |
| BUTR — Bannerlord.Lib.Harmony | https://github.com/BUTR/Bannerlord.Lib.Harmony | 2026-02-02 | **Medium** — fork notes for mod compile refs. | **High** for packaging intent. |
| Forums — Bannersample Harmony | https://forums.taleworlds.com/index.php?threads/bannersample-an-example-mod-using-harmony.402188/ | 2026-02-02 | **Low–medium** — historical sample pointer. | **Low** — **stale**; prefer current GitHub samples. |
| Nexus — Harmony Patch Scanner | https://www.nexusmods.com/mountandblade2bannerlord/mods/9179 | 2026-02-02 | **Low** for RTSCameraLite v0.1 — debugging/ops tool if you add Harmony later. | **Medium** |
| Reddit / YouTube | various | 2026-02-02 | **Low** — anecdotes, version confusion. | **Low** — **not** engineering sources. |

---

## 2. Useful repositories / docs

| Name | URL | What it contains | Why relevant | Active / recent | License (if visible) |
|------|-----|------------------|--------------|-------------------|----------------------|
| RTSCamera (upstream) | https://github.com/lzh-mb-mod/RTSCamera | Full RTS camera + command system source tree, releases. | Direct prior art for **free camera**, **orders**, dependency stack often cited on Nexus. | Commits/releases vary by maintainer workload — **check default branch activity** before treating as “current.” | **Verify in repo** (often MIT/GPL-style; do not assume). |
| RTSCamera (Norbivar fork) | https://github.com/Norbivar/RTSCamera | Fork with releases for newer game lines. | Same as above; often what players run. | **Usually active** for supported game versions. | **Verify in repo** |
| BUTR — Module.Template | https://github.com/BUTR/Bannerlord.Module.Template | `dotnet new` template, module layout, packaging conventions. | Standard **module skeleton** if you ever align with BUTR layout (optional for this repo). | **Active** (NuGet `Bannerlord.Templates`). | Per repo |
| BUTR — ButterLib | https://github.com/BUTR/Bannerlord.ButterLib | HotKey wrappers, helpers, docs at https://butterlib.butr.link/ | **Hotkeys**, utilities, JSON config patterns many mods depend on. | **Active** | Per repo |
| BUTR — UIExtenderEx | https://github.com/BUTR/Bannerlord.UIExtenderEx | Gauntlet prefab/VM extension APIs; docs site linked from README. | **Tactical UI** / order UI injection patterns. | **Active** | Per repo |
| Aragas — MCM (MBOptionScreen) | https://github.com/Aragas/Bannerlord.MBOptionScreen | MCM v2–v5 docs, attribute/fluent settings, NuGet. | **In-game settings** standard for Bannerlord mods. | **Active** | Per repo |
| BUTR — BLSE | https://github.com/BUTR/Bannerlord.BLSE | Launcher / extender, crash interception, mod list UX. | Player **ecosystem** expectation; optional for pure module load. | **Active** | Per repo |
| BUTR — Bannerlord.Harmony | https://github.com/BUTR/Bannerlord.Harmony | Community Harmony module packaging. | If you ever add Harmony: **correct dependency model** for Bannerlord. | **Active** | Per repo |
| Bannerlord-Modding/Documentation | https://github.com/Bannerlord-Modding/Documentation | Community guides + links. | Onboarding, cross-links to apidoc/wiki. | Moderate activity | Per repo |
| TaleWorlds/Documentations | https://github.com/TaleWorlds/Documentations | Official doc sources. | Official baseline. | Active | Per TaleWorlds terms |
| schplorg/bannersample-mod (example) | https://github.com/schplorg/bannersample-mod | Small Bannerlord + Harmony example (per search hits). | **Harmony patch structure** illustration only. | Verify last commit — may be **stale**. | **Verify in repo** |

**Project-local research (not “public web” but authoritative for this repo):**

| Name | Path | What it contains | Why relevant |
|------|------|------------------|--------------|
| Camera hooks | `docs/research/camera-hooks.md` | `MissionView.UpdateOverridenCamera`, bridge/restore strategy, no Harmony. | **Primary** decision input for Slice 5. |
| Native order hooks | `docs/research/native-order-hooks.md` | `OrderController`, `OrderType`, `Team`, `WorldPosition`, selection restore. | **Primary** decision input for Slice 12. |

---

## 3. Camera implementation references

| Link | Public surface (as documented / described) | Method pattern | Public API vs Harmony / reflection | Adoption recommendation |
|------|---------------------------------------------|----------------|-------------------------------------|-------------------------|
| https://apidoc.bannerlord.com/ | `MissionView` in `TaleWorlds.MountAndBlade.View` — search **UpdateOverridenCamera** (Bannerlord spelling). | Virtual per-frame camera override returning `bool`. | **Public virtual** on `MissionView`. | **Adopt** — this is the supported extension point for Slice 5 (matches `camera-hooks.md`). |
| https://bannerlordapi.butr.link/ (MissionView) | Class members listing for navigation. | Same as above. | Public API docs. | **Use for navigation**; confirm on local DLLs. |
| https://github.com/lzh-mb-mod/RTSCamera / Norbivar fork | Battle camera + RTS mode; implementation in `source/` (per public descriptions). | Typically `MissionView` subclass + camera state; **may** use Harmony/reflection beyond public hooks — **inspect before mirroring**. | **Mixed** in mature feature mods. | **Study patterns only**; RTSCameraLite policy is **public API + bounded reflection** per `slice-5-audit.md` / `camera-hooks.md`. **Do not copy.** |
| Nexus RTS Camera pages | Feature lists (F10, command modules, dependency lists). | N/A | User-facing only. | **Use for dependency landscape** only. |

**Consolidated recommendation (camera):**

- **Primary path:** `MissionView.UpdateOverridenCamera(float)` returning `true` when owning the frame (official + `camera-hooks.md`).
- **Bridge:** `MissionScreen` + camera instance; apply frame via **public reflection** allowlist (`SetCameraFrame` / `SetFrame` / `Frame` setter) — names **must** be verified on target build (ILSpy).
- **Restore:** parameterless `MissionScreen` methods by stable name list (e.g. `ActivateMainAgentCamera`) — already reflected in project `CameraBridge`; verify on build.
- **Harmony:** **Not recommended** for Slice 5 in this repo (explicit hard gate).

**Known failure modes (from public + local docs):**

- Member rename between game versions → reflection miss → no camera motion (safe failure if logged once).
- Wrong camera object resolved → no-op or wrong view.
- Restore not called on mission end → stuck camera (mitigate in `OnRemoveBehavior` — already a project concern).

**Test cases (manual / checklist alignment):**

- Toggle RTS on/off; battle end with RTS on; spam toggle — see `docs/slice-5-audit.md` and `docs/manual-test-checklist.md`.

---

## 4. Command / order references

| Link | Public surface | Method pattern | Public API vs Harmony | Adoption recommendation |
|------|----------------|----------------|------------------------|---------------------------|
| `native-order-hooks.md` (local) | `Team.GetOrderControllerOf(Agent)`, `PlayerOrderController`, `OrderController.SelectedFormations`, `ClearSelectedFormations`, `SelectFormation`, `SetOrder`, `SetOrderWithPosition`, `OrderType`, `WorldPosition`. | Snapshot selection → clear → select target → issue → restore in `finally`. | **All public** on inspected 1.2.12-class refs. | **Adopt** for Slice 12 — matches current `NativeOrderExecutor` design. |
| https://apidoc.bannerlord.com/ | `OrderController`, `Formation`, `OrderType`, `MovementOrder` factories. | Alternative: `Formation.SetMovementOrder` — different UX semantics. | Public. | **Defer** `MovementOrder` path for minimal set; public docs + `native-order-hooks.md` explain why `OrderType.Charge` / `StandYourGround` / `Move` were chosen. |
| RTSCamera (GitHub) | Command system submodule in some distributions (`RTSCamera.CommandSystem` mentioned in community text). | Likely broader than three commands. | Unknown without file-level review. | **Reference for product ideas only** — not an implementation template for this lite slice. |

**Consolidated recommendation (orders):**

- **First commands:** `Charge` → `SetOrder(Charge)`; `HoldPosition` → `SetOrder(StandYourGround)`; `MoveToPosition` → `SetOrderWithPosition(Move, WorldPosition(scene, vec3))`.
- **Formation handling:** `OrderController` selection snapshot + restore (as documented in `native-order-hooks.md`).
- **Ground target:** keep **Slice 11** camera-forward / terrain reflection path; orders consume `WorldPosition` built in executor — aligns with public `WorldPosition` ctor.
- **Harmony:** **Not required** for issuance path per local research.

**Test cases:** See `docs/slice-12-audit.md` (H/C/G keys, invalid formation, mission ended).

---

## 5. Config / UI references

| Link | Topic | Adoption recommendation |
|------|-------|---------------------------|
| https://github.com/Aragas/Bannerlord.MBOptionScreen + https://mcm.bannerlord.aragas.org/ | MCM menus, global/per-campaign settings, attribute APIs. | **Later** for RTSCameraLite if you want in-game UI over JSON; **v0.1** uses `config/rts_camera_lite.json` per slice audits. |
| https://github.com/BUTR/Bannerlord.ButterLib + https://butterlib.butr.link/articles/HotKeys/Overview.html | HotKeyManager wrapper, categories, JSON serialization. | **Later** — reduces keybind conflict complexity; adds **Hard dependency chain** (often ButterLib + friends). |
| https://github.com/BUTR/Bannerlord.UIExtenderEx + https://docs.bannerlordmodding.lt/gauntletui/uiextenderex/ | Gauntlet prefab + VM mixins. | **Later** for tactical HUD / markers; **not** Slice 8 non-goal. |
| Nexus mod pages (ButterLib, UIExtenderEx, MCM) | Dependency graphs users expect. | **Product** insight only — engineering truth = GitHub README + NuGet. |

**Tactical markers / overlays:** No single dominant open-source “tactical marker” repo surfaced in this scan comparable to RTSCamera. Nexus mods such as **Formation Tweaks** (e.g. https://www.nexusmods.com/mountandblade2bannerlord/mods/9378) are **behavior**-tilted, not necessarily **open HUD** references — treat as **low relevance** for RTSCameraLite v0.1.

---

## 6. Dependency assessment

| Dependency | Needed for v0.1? | Needed later? | Risk | Benefit |
|------------|------------------|----------------|------|---------|
| **Harmony** | **No** (explicit Slice 5 / 12 stance; public APIs suffice for core camera + orders). | **Optional** only if a future slice requires patching private internals or input suppression impossible via public API (see `slice-6-audit.md`). | **High** — patch conflicts, update fragility, load order. | Unlocks engine internals not exposed publicly. |
| **ButterLib** | **No** | **Maybe** — hotkeys, helpers, logging integrations. | **Medium** — transitive expectations (load order, other mods). | Cleaner hotkey/config integration. |
| **UIExtenderEx** | **No** | **Maybe** — if extending order UI / Gauntlet HUD. | **Medium** — UI coupling to TW prefab IDs. | Safe multi-mod UI extension patterns. |
| **MCM** | **No** | **Maybe** — user-facing settings without hand-editing JSON. | **Low–medium** — extra deps (MCM docs historically list Harmony/UIExtenderEx/ButterLib as ecosystem peers — confirm current MCM major version docs). | Standard in-game settings UX. |
| **BLSE** | **No** for compiling the module; **player-side** many mod lists assume it. | **Optional** — recommend in user README if targeting heavy mod stacks. | **Low** as optional launcher; **medium** if you assume specific BLSE-only features. | Better crash reporting, mod list management. |

**Ecosystem note (from public descriptions):** Full **RTSCamera** on Nexus often lists **Harmony + UIExtenderEx (+ ButterLib / MCM)** style stacks. **RTSCameraLite intentionally diverges** for a smaller dependency surface until a slice explicitly accepts broader deps.

---

## 7. Recommended architecture updates

Based on this scan **plus** existing repo research:

| Change | Rationale |
|--------|-----------|
| **Keep** `MissionView` + `CameraBridge` reflection allowlist approach | Matches official extension point; avoids Harmony (hard gate). |
| **Keep** `OrderController` public path for three commands | Matches `native-order-hooks.md` and public apidoc intent. |
| **Do not add** ButterLib / MCM / UIExtenderEx / Harmony **for v0.1** unless scope change is explicit. | Nexus “standard stack” is informative, not a mandate for a “Lite” module. |
| **Files to add (optional, later)** | Thin `docs/research/nexus-dependency-landscape.md` if you want to track third-party mod interaction notes without bloating this scan. |
| **Files to remove** | None identified from this scan. |
| **Slices to adjust** | **Slice 5 / 12:** unchanged direction — public APIs + bounded reflection; Harmony remains out-of-scope until renegotiated. **Slice 6:** remains honest about suppression limits (no new deps from this scan). **Slice 8 / 11:** no UIExtenderEx / markers introduced. **Slice 13:** N/A in repo — if introduced later, gate any UI work behind explicit deps. |
| **Risks introduced by following public mods blindly** | Copying **RTSCamera** internals could import **Harmony assumptions**, **private patches**, or **wrong game-version symbols**. **Mitigation:** ILSpy on **your** Bannerlord `bin`, apidoc version tree matching `docs/version-lock.md`, and repo research artifacts. |

---

## Scan limitations

- **No file-level audit** of third-party repos was performed in this session (web index only). Class/method names in section 3–4 prioritize **official apidoc**, **BUTR API mirror**, and **this repo’s** `camera-hooks.md` / `native-order-hooks.md`.
- **Nexus** pages change; URLs are stable but descriptions may drift.
- **YouTube / Reddit** omitted from actionable conclusions except as “low reliability.”

---

## Document history

| Date | Note |
|------|------|
| 2026-02-02 | Initial public web scan consolidated into this research artifact. |
