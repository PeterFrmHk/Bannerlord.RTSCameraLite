# Slice 1 — Module foundation

## Purpose

Deliver a **clean, loadable** Mount & Blade II: Bannerlord module that validates the **build → deploy → launcher → main menu → battle** path. **By default** the module must **not** attach experimental mission runtime: `SubModule` may register `CommanderMissionView` only when `EnableMissionRuntimeHooks` is explicitly **true** in `config/commander_config.json` (fail-closed preflight). No Harmony or community UI stacks (MCM, ButterLib, UIExtenderEx, BLSE) in baseline.

## Files created or verified

| Path | Role |
| --- | --- |
| `Bannerlord.RTSCameraLite.csproj` | `net472`, output `bin\Win64_Shipping_Client\`, explicit `Compile` list for mod sources, `SubModule.xml` + `config/commander_config.json` copied to output. |
| `SubModule.xml` | Module **Id** `Bannerlord.RTSCameraLite`, display **Name** RTS Commander Doctrine, version **v0.1.0-slice1**, singleplayer on / multiplayer off, depends on Native + SandBoxCore + Sandbox + StoryMode + CustomBattle, loads `Bannerlord.RTSCameraLite.dll` / `Bannerlord.RTSCameraLite.SubModule`. |
| `src/SubModule.cs` | `MBSubModuleBase`: guarded lifecycle, optional `CommanderMissionView` when mission is supported **and** `EnableMissionRuntimeHooks` is true (config read fail-closed). |
| `config/commander_config.json` | Shipped defaults: **`EnableMissionRuntimeHooks` false**, dormant doctrine/diagnostic/router flags off. |
| `src/Core/ModConstants.cs` | `ModuleId`, `DisplayName`, `LegacyShortName`, `Version`, `SupportedGameVersion` placeholder. |
| `src/Core/ModLogger.cs` | Try/catch logging; **InformationManager** only when UI is marked ready. |
| `docs/slices/slice-1-foundation.md` | This document. |
| `docs/tests/manual-test-checklist-slice-1.md` | Manual acceptance tests for Slice 1. |

## Non-goals (explicit)

- No mandatory mission behavior for users who keep **default** config (hooks off).
- No Harmony, MCM, ButterLib, UIExtenderEx, or BLSE dependency in baseline.
- No automated in-repo E2E of the full game (manual checklist only).

## Tests

Follow **`docs/tests/manual-test-checklist-slice-1.md`**.

Minimum bar:

- `dotnet build` succeeds.
- Module appears and enables in the Bannerlord launcher.
- Game reaches main menu with the mod enabled.
- Battle starts with **default** `EnableMissionRuntimeHooks` **false** without attaching experimental mission shell.

## Audit

| Topic | Assessment |
| --- | --- |
| **Identity** | Display name **RTS Commander Doctrine**; internal legacy label preserved in `ModConstants.LegacyShortName` for logs. |
| **Dependencies** | Official modules only, per `SubModule.xml`. |
| **Risk** | **Low** with defaults — mission code dormant until opt-in; lifecycle wrapped in try/catch. |

## Next slice readiness

- Later slices extend `CommanderMissionView` and config; defaults remain conservative until operators opt in.
- Update `ModConstants.SupportedGameVersion` after pinning the target Native build.

**Related:** `docs/slice-1-audit.md` (historical), `docs/slice-roadmap.md`.
