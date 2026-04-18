# RTS Commander Doctrine (Bannerlord.RTSCameraLite)

**Unofficial mod** for *Mount & Blade II: Bannerlord*. Not affiliated with or endorsed by TaleWorlds Entertainment.

## Pitch

**RTS Commander Doctrine** is a design-led mod project that reframes battle control around **commander presence** and **formation discipline**—not hotkey shapes alone. The repository **`Bannerlord.RTSCameraLite`** is the implementation home: a C# module that grows from a minimal RTS camera and native-order spine toward a full **commander doctrine** loop.

## What the mod does (today)

- Loads as a single-player Bannerlord module.
- Provides an **RTS-style mission camera** path (mission view, pose, input, bridge research) and **tactical command validation and issuance** primitives grounded in **public** `OrderController` / formation APIs where possible.
- Uses **JSON configuration** for keys and camera feel.
- Ships **research and slice audits** under `docs/` so engine integration stays explicit and version-aware.

## Why it exists

Bannerlord’s battles excel at spectacle and melee chaos, but **commander fantasy**—reading the field, standing where the line lives, and having troops **earn** their geometry through proximity, morale, and training—is under-served by “formation as a button” thinking. This project documents and builds toward a **doctrine-first** commander experience while **preferring native orders** over agent puppeteering.

## Current status

- **Playable / engineering slices:** Foundation through **native order execution**, **ground targeting**, and **minimal command feedback** are in active development; see `docs/slice-roadmap.md` and `docs/slices/README.md` for the honest split between **shipped behavior** and **planned doctrine**.
- **Doctrine slices** (default RTS entry, Backspace policy, commander nucleus, rally/absorption, cavalry sequence): **planned**—documented in design and architecture files, not claimed as finished gameplay.
- **Display name in `SubModule.xml` may read “RTS Camera Lite”** until a future release pass aligns packaging with the doctrine title.

## Feature pillars (target)

1. **Commander doctrine** — Presence, morale, training, and pressure gate disciplined formation behavior.
2. **Native command orchestration** — Charge, hold, move, and (later) cavalry flows built on **engine order primitives**, isolated behind adapters.
3. **Formation discipline** — Row, rank, spacing as **earned layout**, not a free UI shape.
4. **Readable battlefield camera** — RTS camera that supports command decisions without a full tactical overlay requirement.
5. **Evidence-first engineering** — Local assemblies and audits are the source of truth; Harmony only when public APIs are insufficient.

## Technical focus

- **C#**, .NET Framework compatible with Bannerlord’s client profile.
- **MissionView** camera override hook (`UpdateOverridenCamera`) and **mission tick** logic.
- **Adapter boundaries** for camera and orders (`CameraBridge`, order executor, future formation data adapter).
- **No MCM / no mandatory Harmony** in the baseline philosophy (optional later, explicitly gated).

## Portfolio goal

Demonstrate **systems design + native engine integration + disciplined iteration**: clear thesis, layered architecture, research-backed API use, slice-based delivery, and recruiter-readable documentation (`docs/hiring-case-study.md`, `docs/design-overview.md`).

## Repository structure (high level)

| Path | Role |
| --- | --- |
| `src/` | C# gameplay and integration code (not modified in Slice P1). |
| `config/` | Default JSON profile (keys, camera). |
| `docs/` | Design, architecture, roadmap, audits, hiring case study, media plan. |
| `docs/research/` | Base-game and installed-mod research; local DLLs are authority. |
| `docs/slices/` | Slice methodology index (links to slice audits and checklists). |
| `SubModule.xml` | Module metadata for the Bannerlord launcher. |
| `bin/Win64_Shipping_Client/` | Build output (local). |

## Disclaimer

This is an **independent, unofficial** mod. Use at your own risk. Game updates can break APIs; verify against **your** installed `TaleWorlds.*` assemblies and `docs/version-lock.md`.
