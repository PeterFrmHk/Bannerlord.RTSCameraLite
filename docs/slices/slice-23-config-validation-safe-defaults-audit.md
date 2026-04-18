# Slice 23 — Config validation, safe defaults, migration, and audit

## Purpose

Harden the existing `CommanderConfig` pipeline without MCM, external config libraries, new gameplay behavior, or native execution changes. The mod continues to load a single JSON file under the module folder; Slice 23 adds **migration**, **schema metadata**, **validation and sanitization**, **optional rewrite** of the file when fixes should be persisted, and **documentation** for operators.

## Components

| Type | Role |
| --- | --- |
| `CommanderConfigSchema` | Exposes `CurrentConfigVersion`, `SchemaLabel`, known root property names, and documentation-oriented field groupings (`DocumentationGroups`, `BuildDocumentationOutline`). |
| `CommanderConfigMigration` | Scans raw JSON with `Utf8JsonReader` (tolerates duplicate root keys that `JsonSerializer` accepts with last-wins semantics). Warns on unknown root properties, fills **absent** root properties from `CommanderConfigDefaults`, and stamps `ConfigFileVersion` when missing or invalid. Returns `MigrationOutcome` with `NeedsPersist` when the on-disk file should be rewritten. |
| `CommanderConfigValidator` | Clones the merged model, validates keybind strings via `CommanderInputKeyParser`, clamps numeric and interval fields per Slice 23 rules, and returns `CommanderConfigValidationResult` with warnings, errors, `SanitizedConfig`, and `RequiresRewrite` when any sanitizer changed a value. |
| `CommanderConfigService` | After existing `ApplyOmitted*` / `Harmonize*` steps: runs migration, then validation, logs capped warning lines via `ModLogger.Warn`, and attempts `WriteConfig` when migration or validation requests a rewrite. |

## Versioning

- `ConfigFileVersion` on `CommanderConfig` mirrors `CommanderConfigSchema.CurrentConfigVersion` (currently `1`).
- Older files without this property receive it on load through migration (absent-key merge) and a rewrite when persistence succeeds.

## Safe recovery behavior

- **Malformed JSON**: unchanged from prior slices — deserialize failure yields in-memory defaults and `LogWarningOnce` for the fallback class; no crash.
- **Invalid keybinds**: replaced with the canonical default for that slot; warning emitted.
- **Non-finite or out-of-range floats**: clamped or reset with warnings; doctrine weights are floored at `0`; ratios and authority scores clamped to `[0,1]`.
- **Unknown JSON properties**: ignored for binding; warned so operators can clean typos.
- **Duplicate root keys**: warned; last value from deserialization remains authoritative.

## Non-goals

- No MCM, no UI, no new gameplay semantics, no changes to native order execution paths beyond reading the same config shape.

## References

- Runtime defaults: `CommanderConfigDefaults.CreateDefault`
- Checked-in reference JSON: `config/commander_config.json`
- Operator guide: `docs/configuration.md`
- Manual tests: `docs/tests/manual-test-checklist-slice-23.md`
