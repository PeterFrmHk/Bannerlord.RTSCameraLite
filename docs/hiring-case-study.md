# Hiring case study — RTS Commander Doctrine (Bannerlord.RTSCameraLite)

## Problem

Bannerlord battles give players powerful **tactical verbs**, but the default fantasy often drifts toward **disembodied UI** and **formation-as-icon** thinking. The project asks: how do you move toward a **commander-centric doctrine**—rally, spacing, morale, and native orders—**without** rebuilding the engine or resorting to fragile per-agent hacks?

## Design goal

Ship a **credible commander doctrine**: troops **earn** their geometry; the commander is the **nucleus**; cavalry follows **native-friendly** advance/charge/reform cadence; and all **version-sensitive** engine access sits behind **adapters** with research artifacts.

## Constraints

- **Single-player** module scope; respect engine lifecycle and performance.
- **Public APIs first** for camera and orders; **Harmony** only as a documented last resort.
- **No false advertising**: documentation separates **shipped** slices from **planned** doctrine.
- **Build drift is normal**: local `TaleWorlds.*` assemblies are the authority, not forum snippets.

## System design

- **Layered architecture** (see `docs/architecture.md`): MissionView camera spine, input ownership, doctrine and layout (planned), native order execution, feedback.
- **Slice-based delivery**: each slice ships auditable behavior with manual checks (`docs/manual-test-checklist.md`).
- **Research-first gates** for the riskiest integrations (`docs/slice-hard-gates.md`).

## Native engine integration strategy

- **Camera:** `MissionView.UpdateOverridenCamera` + `MissionScreen` / `Engine.Camera` surfaces, centralized in **`CameraBridge`**.
- **Orders:** `Team` → `OrderController` → `SetOrder` / `SetOrderWithPosition` with **`WorldPosition`**, with selection snapshot restore.
- **Terrain / targeting:** conservative projection and optional reflection for scene height (documented under `docs/research/`).

## AI / design logic

- **Doctrine** encodes eligibility and spacing outcomes (planned), separate from **camera math**.
- **Command routing** validates intents before execution; execution never assumes a valid UI selection state without restoring it.

## Testing plan

- **Build:** `dotnet build -c Release` in CI and locally.
- **Manual:** `docs/manual-test-checklist.md` — RTS toggle, camera bridge warnings, formation cycling, ground target, native orders, markers, battle end cleanup.
- **Version lock:** `docs/version-lock.md` records owner game path and tested build.

## What this demonstrates for hiring

- **Systems thinking:** doctrine layers, explicit boundaries, non-goals.
- **Engine literacy:** research scans from local DLLs, not guesswork.
- **Production hygiene:** throttled feedback, defensive execution paths, slice audits.
- **Communication:** recruiter-readable README and design docs.

## Target roles

- **Technical game designer** — thesis, loops, constraints, non-goals.
- **Gameplay systems designer** — doctrine rules, command routing, native vs custom split.
- **Combat designer** — infantry vs cavalry doctrine, spacing and reform cadence.
- **AI gameplay designer** — commander presence, rally/absorption, pressure (planned).
- **Junior gameplay programmer** — MissionView integration, adapters, incremental slices.
