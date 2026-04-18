# Slice 1 — Module foundation

## Purpose

Deliver a **clean, loadable** Mount & Blade II: Bannerlord module that validates the **build → deploy → launcher → main menu → custom battle** path without registering any **mission behaviors**, camera code, commander doctrine, Harmony, or community UI stacks (MCM, ButterLib, UIExtenderEx, BLSE).

## Files created or verified

| Path | Role |
| --- | --- |
| `Bannerlord.RTSCameraLite.csproj` | `net472`, output `bin\Win64_Shipping_Client\`, **Slice 1 compile scope** (foundation `.cs` only), `SubModule.xml` copied to output. |
| `SubModule.xml` | Module **Id** `Bannerlord.RTSCameraLite`, display **Name** RTS Commander Doctrine, version **v0.1.0-slice1**, singleplayer on / multiplayer off, depends on Native + SandBoxCore + Sandbox + StoryMode + CustomBattle, loads `Bannerlord.RTSCameraLite.dll` / `Bannerlord.RTSCameraLite.SubModule`. |
| `src/SubModule.cs` | `MBSubModuleBase`: startup debug log, UI-ready mark + optional player message, **`OnMissionBehaviorInitialize` calls base only** (no `AddMissionBehavior`). |
| `src/Core/ModConstants.cs` | `ModuleId`, `DisplayName`, `LegacyShortName`, `Version`, `SupportedGameVersion` placeholder. |
| `src/Core/ModLogger.cs` | Try/catch logging; **InformationManager** only when UI is marked ready. |
| `docs/slices/slice-1-foundation.md` | This document. |
| `docs/tests/manual-test-checklist-slice-1.md` | Manual acceptance tests for Slice 1. |

**Note:** Historical camera / tactical / command `.cs` files remain in `src/` for later slices but are **excluded from compilation** until the csproj `Compile` item group is expanded.

## Non-goals (explicit)

- No RTS camera apply/restore, no `MissionView` registration.
- No commander doctrine, rally, spacing planner, or cavalry cadence.
- No command routing or native order executor wiring from `SubModule`.
- No Harmony, MCM, ButterLib, UIExtenderEx, or BLSE dependency.
- No automated in-repo E2E of the full game (manual checklist only).

## Tests

Follow **`docs/tests/manual-test-checklist-slice-1.md`**.

Minimum bar:

- `dotnet build` succeeds.
- Module appears and enables in the Bannerlord launcher.
- Game reaches main menu with the mod enabled.
- Custom battle starts with the mod enabled.
- Confirm **no** new mission behaviors are registered in this slice (empty `OnMissionBehaviorInitialize` body aside from `base`).

## Audit

| Topic | Assessment |
| --- | --- |
| **Identity** | Display name **RTS Commander Doctrine**; internal legacy label preserved in `ModConstants.LegacyShortName` for logs. |
| **Dependencies** | Official modules only, per `SubModule.xml`. |
| **Risk** | **Low** — minimal runtime surface; logging wrapped in try/catch. |
| **Tech debt** | Other `src/` trees are **not compiled**; future slices must re-add `Compile` includes or remove the explicit `Compile Remove` pattern. |

## Next slice readiness

- **Slice 2** can register a **mission shell** (e.g. empty `MissionBehavior` or `MissionView`) once Slice 1 acceptance is signed off.
- Update `ModConstants.SupportedGameVersion` after pinning the target Native build.
- When re-enabling the full codebase, restore `TaleWorlds.MountAndBlade.View` (and any other) references in the csproj as required by restored files.

**Related:** `docs/slice-1-audit.md` (historical), `docs/slice-roadmap.md`.
