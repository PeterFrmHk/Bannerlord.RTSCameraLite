# Slice 2 — Battle mission runtime shell & RTS Commander Mode state

## Scope

- Attach a **battle-only** `MissionView` (`CommanderMissionView`) when `CommanderMissionModeGate.IsSupportedMission` is true.
- Introduce **RTS Commander Mode** on/off state (`CommanderModeState`) and centralized input (`CommanderInputReader`).
- **Not in this slice:** camera movement, native order issuance, commander doctrine, formation restrictions.

## Behavior

| Topic | Decision |
| --- | --- |
| **Default at battle start** | Supported missions (**`MissionMode.Deployment`** or **`MissionMode.Battle`**) open with **RTS Commander Mode enabled** (`CommanderModeState.Enable` with `ModConstants.CommanderShellDefaultEnableReason`). |
| **Mode activation key** | **Backspace** (key **released**), read only through `CommanderInputReader.TryConsumeCommanderModeToggle`. |
| **Emergency debug** | **F10** may toggle the same mode **only** via `TryConsumeEmergencyDebugCommanderToggle` — documented as **DEBUG / development**, not a player contract. |
| **Camera** | **Not moved** in Slice 2 (`CommanderMissionView` does not override `UpdateOverridenCamera`). |
| **Commands** | **Not issued** — no `OrderController` / `NativeOrderExecutor` integration. |
| **Logging** | On first attach: debug + UI line **"RTS Commander Mode active"**. On each **enabled/disabled** transition: one **debug** line (no per-frame spam). |
| **Cleanup** | `Mission.MissionEnded` or `OnRemoveBehavior` forces **disabled** and clears one-shot log flags. |

## Gate (`CommanderMissionModeGate`)

Conservative **allow-list**: `Mission` not null, not ended, mode is **`Battle`** or **`Deployment`** only. All other `MissionMode` values (tournament, duel, conversation, cutscene, replay, barter, benchmark, stealth, startup, etc.) **do not** get `CommanderMissionView` from `SubModule.OnMissionBehaviorInitialize`.

## Files

| Path | Role |
| --- | --- |
| `src/Mission/CommanderMissionModeGate.cs` | Static mission filter. |
| `src/Mission/CommanderMissionView.cs` | `MissionView` shell + tick + cleanup. |
| `src/Mission/CommanderModeState.cs` | `IsEnabled`, `StartsEnabled`, `ToggleCount`, `LastToggleReason`, `Enable`/`Disable`/`Toggle`/`ForceDisabled`. |
| `src/Input/CommanderInputReader.cs` | Backspace + optional F10 debug toggle. |
| `src/SubModule.cs` | Registers `CommanderMissionView` when the gate passes. |

## Build notes

- `TaleWorlds.MountAndBlade.View.dll` is referenced from **`Modules\Native\bin\Win64_Shipping_Client`** when using `BannerlordGameFolder`. If that path is missing on your install, point `BannerlordGameFolder` at the game root or adjust `Bannerlord.RTSCameraLite.csproj` / `local.props`.

## See also

- [`docs/tests/manual-test-checklist-slice-2.md`](../tests/manual-test-checklist-slice-2.md)
