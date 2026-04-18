# RTS Commander Doctrine (Bannerlord.RTSCameraLite)

**Unofficial mod** for *Mount & Blade II: Bannerlord*. Not affiliated with or endorsed by TaleWorlds Entertainment.

## Pitch

**RTS Commander Doctrine** is a design-led mod project that reframes battle control around **commander presence** and **formation discipline**—not hotkey shapes alone. The repository **`Bannerlord.RTSCameraLite`** is the implementation home: a C# module that grows from a minimal RTS camera and native-order spine toward a full **commander doctrine** loop.

## What the mod does (today)

- **Default install:** **Load-safe foundation** — the module loads, logs startup, and **does not attach `CommanderMissionView`** unless you opt in. Mission code (doctrine, camera shell, diagnostics, command router, etc.) ships in the DLL but stays **dormant** when **`EnableMissionRuntimeHooks`** is **`false`** in `config/commander_config.json` (the shipped default). Debug-heavy toggles default **off**. **`Bannerlord.Harmony`** is listed in `SubModule.xml` (launcher dependency); **Harmony patches remain off** unless **`EnableHarmonyPatches`** and **`EnableMissionRuntimeHooks`** are both **true** (scaffold only — no patches registered yet).
- **Experimental / unverified:** With hooks on, commander doctrine, RTS camera bridge, native-order routing, markers, and diagnostics are **candidate** behaviors — treat as **not in-game verified** until manual checklists are signed off. **Correct deployment (D1) does not guarantee battle stability** — runtime stability is a separate concern (Crash Quarantine defaults).
- Ships **research and slice audits** under `docs/` so engine integration stays explicit and version-aware.

**Deploy / install:** Use **`docs/deploy.md`** (package + audit + optional deploy scripts) or **`docs/install.md`** (manual copy checklist). Do **not** copy only the main DLL — copy the **entire** `bin/Win64_Shipping_Client` output from the build. Scripts: `scripts/package-module.ps1`, `scripts/audit-steam-deployment.ps1` (read-only), `scripts/deploy-to-steam.ps1` (optional; backs up existing module). Research: `docs/research/public-deployment-scan.md`, `docs/research/local-steam-mod-scan.md`.

## Why it exists

Bannerlord’s battles excel at spectacle and melee chaos, but **commander fantasy**—reading the field, standing where the line lives, and having troops **earn** their geometry through proximity, morale, and training—is under-served by “formation as a button” thinking. This project documents and builds toward a **doctrine-first** commander experience while **preferring native orders** over agent puppeteering.

## Current status

- **Shipped in repo** means **code present**, not “proven in your build” unless a manual checklist is completed. **Default config** keeps **`EnableMissionRuntimeHooks` false**, **`StartBattlesInCommanderMode` false**, and doctrine / diagnostics / router / markers / native-order / performance-warning paths **off** until you enable them deliberately.
- See `docs/slice-roadmap.md` and `docs/slices/README.md` for **shipped vs planned** doctrine work (default RTS battle entry remains a **design target**, not the default runtime).
- **Display name** in `SubModule.xml` is **RTS Commander Doctrine** (v0.1.0-slice1); legacy short name remains in `ModConstants.LegacyShortName` for logs.

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
- **Harmony:** declared dependency (**Bannerlord.Harmony**); **Lib.Harmony** is compile-only in the `.csproj` — **do not ship `0Harmony.dll`** with this mod (the Harmony module provides it). **No MCM** in baseline.

## Portfolio goal

Demonstrate **systems design + native engine integration + disciplined iteration**: clear thesis, layered architecture, research-backed API use, slice-based delivery, and recruiter-readable documentation (`docs/hiring-case-study.md`, `docs/design-overview.md`).

## Repository structure (high level)

| Path | Role |
| --- | --- |
| `src/` | C# gameplay and integration code (explicit compile list in `.csproj`). |
| `config/` | Default JSON profile (keys, camera). |
| `docs/` | Design, architecture, roadmap, audits, hiring case study, media plan. |
| `docs/research/` | Base-game and installed-mod research; local DLLs are authority. |
| `docs/slices/` | Slice methodology index (links to slice audits and checklists). |
| `docs/install.md`, `docs/deploy.md` | Manual install vs scripted package/audit/deploy. |
| `scripts/` | `package-module.ps1`, `audit-steam-deployment.ps1`, `deploy-to-steam.ps1`. |
| `SubModule.xml` | Module metadata for the Bannerlord launcher. |
| `bin/Win64_Shipping_Client/` | Build output (local). |

## Disclaimer

This is an **independent, unofficial** mod. Use at your own risk. Game updates can break APIs; verify against **your** installed `TaleWorlds.*` assemblies and `docs/version-lock.md`.
