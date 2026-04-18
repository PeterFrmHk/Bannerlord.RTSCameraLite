# Slice 7 — Backspace conflict handling and Commander Mode input ownership

## Purpose

Centralize **Commander Mode activation key** handling in `CommanderInputReader`, record **advisory input ownership** while commander mode is active, and isolate **Backspace vs native mission-order UI** policy inside `BackspaceConflictGuard`. This slice prepares for future engine hooks without scattering `IInputContext` calls or guessing at TaleWorlds internals.

## Native Backspace conflict (summary)

- **Slice 0** (`docs/research/implementation-decision-slice0.md` §7) and **`base-game-order-scan.md` §8** document that **Backspace is high-risk** for collision with vanilla **mission order** UI (`MissionOrderUIHandler` and related view types).
- Default **player hotkey tables** are not fully resolved from static XML alone; bindings are **UNCERTAIN** per profile.
- **Harmony** is **not** approved for this mitigation in the baseline roadmap.

## Suppression: wired or not?

| Surface | Status |
| --- | --- |
| **`ShouldSuppressNativeBackspace()`** | **Not wired** — always returns **`false`** (safe). Managed code has **no verified public API** to prevent the engine from observing Backspace for native order UI. |
| **`BackspaceConflictResult.Kind`** | **`Unsupported`** when commander mode is on and `OverrideNativeBackspaceOrders` is **true** (policy requests coordination but enforcement is impossible without new research). |
| **`CommanderNativeInputGuard.ShouldSuppressNativeOrderMenu()`** | **Not wired** — always **`false`**. Movement/combat suppression flags are **reserved** and default **false** until a safe path exists. |

## Config fields (`commander_config.json`)

| Field | Default (Slice 7) | Role |
| --- | --- | --- |
| `OverrideNativeBackspaceOrders` | `true` | Policy: mod **intends** to coordinate Backspace with native orders when commander is on. Does **not** enable engine suppression. |
| `AllowNativeOrdersWhenCommanderModeDisabled` | `true` | Policy placeholder: when commander is **off**, native order flows are expected to behave normally. |
| `EnableInputOwnershipGuard` | `true` | When `true`, publishes advisory `CommanderInputOwnershipState` while commander is on. |
| `SuppressNativeMovementInCommanderMode` | `false` | Reserved — **no safe suppression** in this slice. |
| `SuppressNativeCombatInCommanderMode` | `false` | Reserved — **no safe suppression** in this slice. |
| `ModeActivationKey` | `Backspace` | Parsed centrally by `CommanderInputReader` (invalid names fall back to defaults). |
| `EnableDebugFallbackToggle` / `DebugFallbackToggleKey` | `true` / `F10` | If the fallback key parses to the same key as activation, the fallback is **disabled** (logged once) to avoid ownership conflict. |

## Failure behavior

- **No crash:** guards are pure C# state; no reflection into private engine input stacks.
- **No input lock:** `Cleanup()` on mission teardown and `ExitCommanderMode()` on toggle **off** reset guard state; `CommanderMissionView` calls **`CameraBridge.RestoreNativeCamera`** when commander disables (bridge may still return **NotWired**).
- **Double-fire risk remains:** releasing the activation key can still satisfy both mod toggle (`IsKeyReleased`) and native handlers — documented limitation until a verified interception API exists.

## Tests

- `dotnet build -c Release`
- Manual: `docs/tests/manual-test-checklist-slice-7.md`

## Design audit (minimal)

| | Statement |
| --- | --- |
| **A** | Backspace can simply toggle Commander Mode. |
| **¬A** | Backspace may already trigger native order UI, causing double-fire and input ownership conflicts. |
| **A\*** | Centralize activation key ownership in `CommanderInputReader` and isolate native suppression policy inside `BackspaceConflictGuard` (effective suppression only when research delivers a supported path). |

## Audit (risks)

| Risk | Mitigation |
| --- | --- |
| **False sense of security** from `OverrideNativeBackspaceOrders` | Result **`Kind = Unsupported`** and docs state clearly that suppression is **not** implemented. |
| **Key shadowing** | `CommanderInputReader` disables debug fallback when it matches the activation key (one-time warning). |
| **Stale JSON without new keys** | `CommanderConfigService` merges Slice 7 defaults when properties are **omitted** from the file (bools would otherwise default to `false`). |

## See also

- [`manual-test-checklist-slice-7.md`](../tests/manual-test-checklist-slice-7.md)
- [`slice-4-internal-camera-pose-movement.md`](slice-4-internal-camera-pose-movement.md) (camera bridge hand-off)
- Research: `docs/research/implementation-decision-slice0.md`, `base-game-order-scan.md`, `base-game-camera-scan.md`, `native-cavalry-command-sequence.md`
