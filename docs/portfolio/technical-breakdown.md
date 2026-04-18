# Technical breakdown

Language below matches the **Bannerlord.RTSCameraLite** repo layout. This mod does not introduce a separate `MissionLogic` C# type; **mission-time orchestration** lives in **`MissionView`** subclasses and helpers (Bannerlord pattern).

## MissionView layer

- **`CommanderMissionView`** (and related mission views): per-tick orchestration for commander mode, camera controller, doctrine scans, cavalry sequences, markers, diagnostics, and lifecycle cleanup.
- **Responsibilities:** scheduling, throttling, “when to call doctrine,” and bridging to services — not low-level agent steering in slices documented to date.

## MissionLogic layer (conceptual)

- In Bannerlord, much “logic” runs from **mission behaviors** and **mission views**. For portfolio purposes, treat **mission behaviors** (if present in repo) and **MissionView** ticks together as the **mission logic tier** — the place where state advances and side effects are decided.

## Adapter boundaries

- **`CameraBridge`**, **`FormationDataAdapter`**, **`NativeOrderPrimitiveExecutor`** (and related types): isolate TaleWorlds calls and keep failures localized.
- **Portfolio angle:** adapters are the answer to “how do you keep engine upgrades survivable?”

## Doctrine pipeline

- **Inputs:** composition, commander presence, morale/training-style scores (per slice docs).
- **Outputs:** eligibility sets, spacing plans, cavalry charge sequence **state**, reform/lock policy decisions.
- **Invariant:** doctrine layers aim to be **advisory or planning-first** until research gates explicitly allow execution.

## Native command orchestration

- **`CommandRouter`** validates `CommandIntent` against `CommandContext` and formation restrictions.
- **Executor:** native primitives are meant to flow through a **single** boundary with config allow-list and “not wired” semantics until verified on a pinned game build.

## Testing approach

- **Manual:** per-slice checklists under `docs/tests/manual-test-checklist-slice-*.md` and the main `docs/manual-test-checklist.md`.
- **Automated:** limited by game host; rely on `dotnet build` for compile safety and in-game smoke for behavior.
- **Evidence:** `feature-proof-checklist.md` maps behaviors to clips and screenshots.

## Doc map

- Roadmap: `docs/slice-roadmap.md`
- Hard gates: `docs/slice-hard-gates.md`
- Slice specs: `docs/slices/slice-*.md`
