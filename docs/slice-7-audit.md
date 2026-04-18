# Slice 7 audit — file-based RTS camera config

## Audit logic (design rationale)

- **A:** Hardcoded controls are fine for a prototype.
- **¬A:** Camera feel depends heavily on local preference and conflicts with player bindings.
- **A\*:** Add file-based config before UI, so tuning becomes testable without dependency bloat (no MCM / ButterLib / UIExtenderEx in this slice).

## Goal

Provide a simple JSON config for key bindings and camera tuning without MCM, ButterLib, or UIExtenderEx. Invalid or missing files must never crash the game.

## Layout

- **Path:** `{Module root}/config/rts_camera_lite.json`
- **Module root** is resolved as two directories above the loaded assembly (`bin/Win64_Shipping_Client` → module folder), matching the standard Bannerlord module layout.
- **Example payload:** matches the repo file `config/rts_camera_lite.json` and the copy-paste block under Slice 7 in `docs/manual-test-checklist.md`.

## Components

| File | Role |
|------|------|
| `RTSCameraConfig` | Serializable settings model (strings + floats). |
| `ConfigDefaults` | Canonical default values matching previous hardcoded behavior. |
| `ConfigService` | Create directory, write default JSON if missing, deserialize with `System.Text.Json`, merge/sanitize, build `CameraKeyBindings` (`InputKey` materialization). |
| `RTSCameraInput` | Uses resolved `InputKey` values from bindings (strings parsed defensively in `ConfigService` at load). |
| `RTSCameraController` | Applies clamped tuning from config for move, rotate, zoom, height, pitch. |
| `RTSCameraMissionBehavior` | Loads config in `OnBehaviorInitialize`, logs one summary line, wires toggle and input. |

## Safety rules

- All disk and JSON operations are wrapped; failures fall back to `ConfigDefaults.CreateDefault()` after merge/sanitize.
- Unknown or empty key names are replaced with default key **names**, then re-parsed to `InputKey`.
- Floats: NaN/Infinity rejected; sensible min/max clamps; `MinHeight` / `MaxHeight` validated so `min < max`.
- `System.Text.Json` is referenced as a normal NuGet dependency (no in-game UI stack).

## Manual acceptance

1. `dotnet build` succeeds.
2. Delete `config/rts_camera_lite.json`; start a mission — file is recreated with defaults.
3. Corrupt JSON (invalid syntax) — game loads; defaults used; diagnostics mention parse failure.
4. Set `MoveSpeed` to `80` — RTS pan speed increases.
5. Set `ToggleKey` to `Home` — RTS toggles on Home instead of F10.
6. Change `ZoomInKey` / `ZoomOutKey` — keyboard zoom fallback follows config (mouse wheel unchanged).

## Non-goals (this slice)

- Hot reload while in battle, MCM, localization, or command-menu integration.
