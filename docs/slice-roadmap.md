# Slice roadmap - RTS Commander Doctrine

This roadmap aligns **design intent** with **incremental delivery**. Status labels:

- **Shipped (repo today):** present in codebase with slice audit / checklist coverage (may still need owner in-game verification).
- **In progress:** partially implemented or awaiting hard-gate research sign-off.
- **Planned:** not implemented; documented only.

> **Doc numbering note:** Some historical audit files use a different slice ID for "minimal markers" work. Doctrine roadmap **Slice 14** is the canonical home for **tactical feedback markers** going forward; see `docs/slice-13-audit.md` for the earlier marker write-up until docs are renamed in a later housekeeping pass.

| Slice | Name | Status | Notes |
| --- | --- | --- | --- |
| **0** | Base game + native command research | **Shipped (docs)** | `docs/research/base-game-camera-scan.md`, `base-game-order-scan.md`, `base-game-formation-layout-scan.md`, `native-order-hooks.md`, `native-cavalry-command-sequence.md`, `installed-mod-reference-scan.md`, `implementation-decision-slice0.md`, `camera-hooks.md`, `public-reference-scan.md`. |
| **1** | Foundation + loadable module | **Shipped** | `docs/slices/slice-1-foundation.md`, `docs/tests/manual-test-checklist-slice-1.md`; load-safe defaults; mission runtime exists in code but is **opt-in** via `EnableMissionRuntimeHooks` (default **false**). |
| **2** | Mission behavior shell | **Shipped** | `CommanderMissionView` when **`EnableMissionRuntimeHooks`** is **true** (default **false** - not attached by default). |
| **3** | Camera controller skeleton | **Shipped** | Pose / controller skeleton. |
| **4** | Movement input + pose updates | **Shipped** | Input snapshot + pose integration. |
| **5** | Real camera bridge + restore | **Shipped / hard-gated** | `docs/slices/slice-5-real-camera-bridge.md`, `docs/tests/manual-test-checklist-slice-5.md`; `src/Adapters/CameraBridge.cs` + `CommanderMissionView`; verify per `docs/slice-hard-gates.md`. |
| **6A** | Default RTS battle entry | **Planned** | Battle begins in RTS Commander Mode by default (design target; **default config keeps `StartBattlesInCommanderMode` false** until verified). |
| **6B** | Backspace native conflict guard | **Planned** | Backspace becomes doctrine toggle; guard native conflicts. |
| **7A** | Commander presence model | **Planned** | Presence gates discipline. |
| **7B** | Commander anchor behind formation | **Planned** | Nucleus placement rule. |
| **8A** | Formation doctrine profile | **Planned** | Infantry vs cavalry spacing presets. |
| **8B** | Formation eligibility rules | **Planned** | Morale / training / equipment hooks. |
| **9A** | Rally + row/rank/spacing planner | **Planned** | Absorption only when close enough. |
| **9B** | No-commander formation restriction | **Planned** | Without commander, no disciplined formation behavior. |
| **10** | Command router skeleton | **Shipped** | Validate intents (`CommandRouter`). |
| **11** | Ground target resolver | **Shipped** | `GroundTargetResolver` + terrain projection service. |
| **12** | Native order executor | **Shipped / hard-gated** | `NativeOrderExecutor`; verify per `docs/slice-hard-gates.md`. |
| **13** | Native cavalry command sequence | **Planned** | Advance, charge, lock release, >= 30 m reform cadence. |
| **14** | Tactical feedback marker | **Shipped (early)** | Minimal markers / feedback; see historical `docs/slice-13-audit.md`. |
| **P1** | Portfolio scaffold | **Shipped (docs)** | README + design + architecture + hiring + media plan + slice indexes. |
| **D1** | Deployable package + Steam verification | **Shipped (docs + scripts)** | `docs/install.md`, `docs/deploy.md`, `scripts/package-module.ps1`, `scripts/audit-steam-deployment.ps1`, `scripts/deploy-to-steam.ps1`; research `docs/research/public-deployment-scan.md`, `local-steam-mod-scan.md`. |
| **CQ1** | Crash quarantine - load-safe defaults | **Shipped** | `EnableMissionRuntimeHooks` fail-closed preflight; dormant debug/router/marker/native-order defaults; guarded `SubModule` / `CommanderMissionView`; `CommanderConfigService.TryReadMissionRuntimeHooksEnabledFailClosed()`. |
| **H1** | Harmony integration scaffold v1 | **Shipped** | `DependedModule Bannerlord.Harmony`; compile-only **Lib.Harmony**; `HarmonyPatchService` + config gates (`EnableHarmonyPatches`, `EnableHarmonyDiagnostics`, both **false** default; requires `EnableMissionRuntimeHooks` for scaffold init); **zero patches**; research `docs/research/harmony-reference-scan.md`. |
| **TW-D1** | TW command layer design | **Planned (design)** | `docs/design/total-war-style-command-layer-v1.md`; Total War-inspired control grammar mapped onto Bannerlord formation/native-order substrate. No gameplay implemented. |
| **TW-1** | Formation selection state | **Planned** | Selection state/service, number-key selection, feedback only; no command intents, no native orders, no default-live runtime. |
| **TW-2** | Ground command intent preview | **Planned** | Ground target resolution + command marker/text preview; validation optional behind gates; no native execution. |

---

## Dependency highlights

- Slices **5** and **12** remain **research-hard-gated** before treating integration as done on a new Bannerlord build (`docs/slice-hard-gates.md`).
- Doctrine slices **6A-9B** depend on commander + layout layers and must not ship inside camera-only commits.
- TW command slices are planned only. Total War inspiration is a control grammar; Bannerlord formations and native order primitives remain the execution substrate.
