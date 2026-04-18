# Manual test checklist — Slice 7 (Backspace conflict + input ownership)

## Build

- [ ] `dotnet build .\Bannerlord.RTSCameraLite\Bannerlord.RTSCameraLite.csproj -c Release` — **0 errors**.

## Launcher / battle

- [ ] Mod appears in launcher without errors.
- [ ] **Custom battle** (or other `CommanderMissionModeGate`-supported mission) loads.

## Commander Mode and keys

- [ ] Commander mode **starts on** or **off** per `StartBattlesInCommanderMode` in `config/commander_config.json`.
- [ ] **`ModeActivationKey`** (default Backspace) toggles commander mode via `IsKeyReleased` (centralized in `CommanderInputReader`).
- [ ] **F10** (or configured fallback) toggles **only** when `EnableDebugFallbackToggle` is **true**.
- [ ] If fallback key is configured equal to the activation key, fallback is **disabled** and a **one-time** debug warning is emitted (`commander_debug_toggle_shadows_activation`).

## Guards lifecycle

- [ ] When commander mode **enables**, `BackspaceConflictGuard.EnterCommanderMode()` and `CommanderNativeInputGuard.EnterCommanderMode()` run (no throw).
- [ ] When commander mode **disables**, both **Exit** run and `CameraBridge.RestoreNativeCamera` is invoked (may log NotWired — acceptable).
- [ ] On **mission end** / behavior remove, both guards **`Cleanup()`** run and commander mode is forced off — **no stuck “locked input” state** in managed guards.

## Suppression expectations

- [ ] **`ShouldSuppressNativeBackspace()`** remains **false** (suppression **not wired** per Slice 0).
- [ ] **`BackspaceConflictGuard.LastEvaluation.Kind`** is **`Unsupported`** when commander is on and `OverrideNativeBackspaceOrders` is true; see `docs/slices/slice-7-backspace-conflict-input-ownership.md`.
- [ ] **No native order menu “double-fire” fix** is claimed in this slice; if both mod and vanilla react to the same release, that is the **documented limitation**.

## Battle exit

- [ ] Leaving the battle does not leave abnormal guard state; second battle behaves normally.
