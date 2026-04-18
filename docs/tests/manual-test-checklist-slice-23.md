# Manual test checklist — Slice 23 (config validation and migration)

Use a shipping or test build with logging visible (Debug output for `ModLogger.Warn` / `LogDebug`). Paths are under `Modules/Bannerlord.RTSCameraLite/config/commander_config.json` unless noted.

## Preconditions

- [ ] One backup copy of a known-good `commander_config.json`.
- [ ] Ability to edit JSON and launch a battle that loads the mod.

## Missing file

- [ ] Delete `commander_config.json` (keep the `config` folder).
- [ ] Launch battle: mod creates default file with `ConfigFileVersion` present.
- [ ] No crash; mission receives config (commander mode behaves as before).

## Malformed JSON

- [ ] Replace file contents with `{ not json`.
- [ ] Launch: mod falls back to in-memory defaults; one-shot warning for invalid JSON path; no crash.

## Unknown and duplicate keys

- [ ] Add a bogus root key `"UnknownExperimentalField": 1` alongside valid keys.
- [ ] Launch: warning mentions unknown property ignored; game runs.
- [ ] Add duplicate root key for a non-critical field (same property twice, different values).
- [ ] Launch: warning about duplicate root property; last value wins; no crash.

## Invalid keybinds

- [ ] Set `"MoveForwardKey": ""` or `"MoveForwardKey": "NotARealKey"`.
- [ ] Launch: warning about invalid keybind; movement uses default `W` after sanitization; optional file rewrite restores valid JSON.

## Numeric clamps

- [ ] Set `"MoveSpeed": -5`, `"FastMoveMultiplier": 0.5`, `"MinHeight": 10`, `"MaxHeight": 5` (invalid ordering).
- [ ] Set `"DoctrineScanIntervalSeconds": 0.05`, `"DefaultMarkerLifetimeSeconds": 0`.
- [ ] Set `"CavalryReleaseLockDistance": 20`, `"CavalryReformDistanceFromAttackedFormation": 5`.
- [ ] Launch: warnings describe clamps; sanitized values respect: move speed > 0, fast multiplier ≥ 1, min height ≥ 2, max > min, scan intervals ≥ 0.1 s, marker lifetimes ≥ 0.1 s, reform distance ≥ release distance.

## Missing `ConfigFileVersion`

- [ ] Remove `ConfigFileVersion` from an otherwise valid file.
- [ ] Launch: migration adds version (and may rewrite file); subsequent load includes the field.

## Regression smoke

- [ ] Restore known-good JSON; launch battle; commander camera and doctrine-related diagnostics behave as in Slice 22 (no intentional gameplay change from Slice 23).
