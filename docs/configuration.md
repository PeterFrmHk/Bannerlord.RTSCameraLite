# RTS Camera Lite — Commander configuration

This mod reads a **single JSON file** shipped with the module. There is **no MCM** and no external config UI dependency.

## File location

- **Path**: `Modules/Bannerlord.RTSCameraLite/config/commander_config.json` (relative to the game installation root, sometimes exposed as `Bannerlord` in mod managers).
- **Creation**: If the file is missing but the folder can be created, `CommanderConfigService` writes the default profile on first load.

## Version field

- **`ConfigFileVersion`**: Integer schema stamp (see `CommanderConfigSchema.CurrentConfigVersion`). Older files may omit it; the loader migrates by adding missing root keys from defaults and stamping the version. Unknown JSON keys at the root are **ignored** for binding and reported as warnings.

## Mission runtime gate

- **`EnableMissionRuntimeHooks`**: When **false** (default), `SubModule` does **not** add `CommanderMissionView`; the module stays load-safe with no mission-side attachment. When **true**, mission runtime may attach on supported battles only. Preflight uses a **fail-closed** read (missing file, bad JSON, or unreadable path → treated as **false**).

## Major config groups

The authoritative list of properties is `CommanderConfig` in source. High-level groups:

1. **Mode and policy** — start in commander mode, activation key, Backspace / native-order policy flags (`OverrideNativeBackspaceOrders`, `AllowNativeOrdersWhenCommanderModeDisabled`, input ownership guard, reserved suppression toggles).
2. **Camera keys** — WASD-style move, Q/E rotate, fast move, zoom keys, debug fallback toggle key.
3. **Camera tuning** — `MoveSpeed` (> 0), `FastMoveMultiplier` (≥ 1), rotation and zoom speeds, `DefaultHeight` clamped to `[MinHeight, MaxHeight]`, `MinHeight` (≥ 2), `MaxHeight` (> min), `DefaultPitch`.
4. **Commander presence** — who counts as commander for advanced formations, authority score threshold (0–1).
5. **Commander anchor** — offsets and allowed radius behind formations (for positioning logic).
6. **Doctrine scoring** — weights (≥ 0), scan interval (≥ 0.1 s), debug flag.
7. **Formation eligibility** — discipline and equipment ratio thresholds (0–1), debug flag.
8. **Rally and absorption** — radii, cooldowns, rally scan interval (≥ 0.1 s).
9. **Cavalry doctrine** — spacing (> 0), release and reform distances (reform ≥ release), impact thresholds (0–1), cooldowns.
10. **Native order execution** — opt-in switches for primitive native calls (defaults conservative).
11. **Command router** — validation and routing toggles, validation debug log throttle (≥ 0).
12. **Native cavalry sequence** — optional sequence toggles and forward-to-charge distance (> 0).
13. **Command markers** — lifetimes (≥ 0.1 s), refresh throttle (≥ 0).
14. **Diagnostics** — panel toggles, refresh interval (≥ 0), inclusion flags.

For a machine-readable outline, call `CommanderConfigSchema.BuildDocumentationOutline()` from tooling that references the assembly, or mirror the groups in `CommanderConfigSchema.DocumentationGroups`.

## Default keybinds (reference JSON)

| Action | Default key name |
| --- | --- |
| Commander mode | `Backspace` |
| Debug fallback toggle | `F10` |
| Move forward / back / left / right | `W` / `S` / `A` / `D` |
| Rotate left / right | `Q` / `E` |
| Fast move | `LeftShift` |
| Zoom in / out | `R` / `F` |
| Diagnostics toggle | `F9` |

Invalid or empty key strings are **replaced** with these defaults at load time (with a warning).

## Backspace and native orders

Slice 7 policy fields (`OverrideNativeBackspaceOrders`, `AllowNativeOrdersWhenCommanderModeDisabled`, related flags) remain **explicit** in JSON. Slice 23 does not infer or override them from keybind choices; operators must set them deliberately.

## Safe recovery behavior

- **Bad JSON** → in-memory defaults; concise warning; no crash.
- **Bad numbers** → clamped or reset per validator rules; warnings; file may be rewritten when fixes should persist.
- **Bad keys** → default key for that slot; warning.
- **Missing keys** → filled from `CommanderConfigDefaults` during migration when the property is absent from the file.

## Further reading

- Slice narrative: `docs/slices/slice-23-config-validation-safe-defaults-audit.md`
- Acceptance tests: `docs/tests/manual-test-checklist-slice-23.md`
- Default object: `CommanderConfigDefaults.CreateDefault` and `config/commander_config.json`
