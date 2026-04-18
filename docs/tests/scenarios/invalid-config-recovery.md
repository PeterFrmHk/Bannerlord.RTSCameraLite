# Scenario: invalid config — recovery & harmonize

## Goal

Prove the mod **survives bad JSON** (falls back to defaults / merge helpers) and remains playable after fixing the file.

## Setup

1. **Backup** `Modules/Bannerlord.RTSCameraLite/config/commander_config.json`.
2. Prepare a broken variant (e.g. truncate mid-object, remove a closing brace, or insert invalid trailing bytes).

## Steps

1. Replace `commander_config.json` with the **broken** file.
2. Launch game → start custom battle → enter mission shell.
3. Observe: module should still load with **defaults** or partial parse per `CommanderConfigService` behavior (see code / logs).
4. Restore **valid** JSON from backup.
5. **Restart mission** or full game (document which you did); confirm intended keys (e.g. diagnostics toggle key) apply.

## Pass criteria

- No hard crash to desktop solely from JSON parse (if observed, file **KF-*** in `known-failures.md`).
- After restore, `EnableDiagnostics` and other edited flags read as expected.

## Evidence

- Note log lines (module id) from failed load if present.
- Screenshot of restored `[Diag]` behavior.

## Safety

Never commit broken JSON to the repo; keep corruption tests **local** only.
