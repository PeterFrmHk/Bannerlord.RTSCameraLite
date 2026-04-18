# Slice 6 audit — input ownership and native guard

## Goal

RTS camera mode should not fight native player controls. This slice introduces explicit ownership state and a single guard type so suppression attempts stay centralized.

## What was implemented

- **`InputOwnershipState`**: flags for RTS owning camera reads and requested native movement/combat suppression, plus a short reason string for diagnostics.
- **`NativeInputGuard`**: `EnterRtsMode` / `ExitRtsMode` / `Tick`; updates ownership; `Tick` is reserved for future public APIs.
- **`RTSCameraInput.Read`**: takes `InputOwnershipState`; returns an empty snapshot when RTS does not own camera input (defensive; mission path only reads while RTS is on).
- **`RTSCameraMissionBehavior`**: calls guard on RTS enable/disable, every mission tick (`Tick`), on behavior removal, and when `Mission.MissionEnded` is true (camera restore + guard exit + forced RTS state off).

## Native suppression limitation (exact)

Bannerlord’s mission input stack ties **agent movement and combat** to the same **`IInputContext`** keys the RTS camera reads for free-cam style motion. In the shipping public surface used by this module (`MissionView.Input`, `IInputContext`), there is **no documented, stable API** that:

1. Lets the mod keep reading WASD/Q/E/etc. for RTS camera, and  
2. Reliably disables only the **agent’s** response to those keys for the local player,

without resorting to **Harmony** or other undocumented internals.

Therefore **`NativeInputGuard.Tick` is currently a no-op** aside from ownership bookkeeping in `Enter`/`Exit`. **`NativeMovementSuppressionRequested` / `NativeCombatSuppressionRequested`** are **advisory flags** for future wiring or documented APIs; they do not change engine behavior in Slice 6.

**Command menu**: no changes were made to command/formation UI; the guard does not patch or intercept those code paths.

## Harmony

Not used. If a future slice adds patches, document the target type/method and risk in this file and in `docs/research/`.

## Manual acceptance checks

1. Build: `dotnet build` on `Bannerlord.RTSCameraLite.csproj`.
2. In battle: F10 enables RTS; camera moves with WASD/Q/E/shift/scroll (or R/F fallback).
3. F10 disables RTS; agent controls feel as before toggle.
4. End battle with RTS still on: no stuck “RTS owns input” state; camera restore runs; next battle starts clean after behavior reattach.

## Follow-up (optional)

- If TaleWorlds exposes a supported “spectator / disable agent input” mission flag, wire it in `NativeInputGuard.Tick` or `EnterRtsMode` and update this audit.
