# Slice 6 — Commander config and control profile

## Purpose

Ship a **local JSON** control and tuning profile for RTS Commander Mode without MCM, ButterLib, UIExtenderEx, BLSE, or other external config UI. Battles can start with commander mode on or off, activation and debug fallback keys are data-driven, camera tuning is centralized and clamped for safety, and malformed files fail open to defaults with a **single diagnostic warning** per issue class.

## Config fields

| Field | Type | Role |
| --- | --- | --- |
| `StartBattlesInCommanderMode` | bool | If true, commander mode is enabled when the mission shell initializes on supported battles. |
| `ModeActivationKey` | string | `InputKey` name for the primary commander toggle (released edge). |
| `OverrideNativeBackspaceOrders` | bool | Reserved policy flag for future native-order coordination (no suppression wiring in this slice). |
| `EnableDebugFallbackToggle` | bool | When true, the debug fallback key may toggle commander mode. |
| `DebugFallbackToggleKey` | string | `InputKey` name for the optional debug toggle. |
| `MoveForwardKey` … `ZoomOutKey` | string | Camera control bindings (parsed defensively; invalid names fall back to defaults). |
| `MoveSpeed` | float | Base camera move speed (must be positive after clamp). |
| `FastMoveMultiplier` | float | Fast-move scale when the fast key is held (≥ 1). |
| `RotationSpeedDegrees` | float | Rotation rate (must be positive after clamp). |
| `ZoomSpeed` | float | Zoom rate (passed through; used by future camera tick). |
| `DefaultHeight` | float | Initial logical height, clamped to `[MinHeight, MaxHeight]`. |
| `MinHeight` | float | Lower height bound (floored at 2). |
| `MaxHeight` | float | Upper bound; if ≤ `MinHeight`, bumped to `MinHeight + 1`. |
| `DefaultPitch` | float | Initial pitch in degrees. |

## Default values

Defaults are defined in code (`CommanderConfigDefaults.CreateDefault`) and match the checked-in `config/commander_config.json`: Backspace activation with native-order override flag on, F10 debug fallback enabled, WASD / QE / LeftShift / R F controls, and the numeric tuning block (`MoveSpeed` 12, `FastMoveMultiplier` 2.5, `RotationSpeedDegrees` 90, `ZoomSpeed` 3, heights 18 / 6 / 60, `DefaultPitch` 60).

## Failure behavior

- **Missing file**: the service creates `Modules/<ModuleId>/config/commander_config.json` with the default object (indented JSON) when the directory can be created.
- **Unreadable path / IO errors**: in-memory defaults; one `LogWarningOnce` keyed by the failure class.
- **Malformed JSON**: in-memory defaults; `LogWarningOnce` for the fallback path; game continues.
- **Invalid key strings**: `CommanderInputReader` re-parses each binding and substitutes the default profile’s key name for that slot when `Enum.TryParse` fails.
- **Unsafe tuning numbers**: `CommanderCameraMovementSettings.FromConfig` clamps per the acceptance rules (minimum height floor, ordered min/max, positive move and rotation speeds, fast multiplier ≥ 1, default height inside the band).

## Tests

Manual coverage lives in `docs/tests/manual-test-checklist-slice-6.md` (missing file, malformed JSON, remapped keys, tuning edits, invalid keys, height clamping, start-in-mode flag, debug toggle gating).

## Audit

- **A**: “Hardcoded controls are enough for prototype.”
- **¬A**: “Camera feel and key conflicts must be tuneable, especially with Backspace overriding native behavior.”
- **A\***: “Add local JSON config before UI dependencies.”
