# Manual test checklist — Slice 24 (performance budget)

## Preconditions

- [ ] Debug output visible for `ModLogger.LogDebug` / `ModLogger.Warn`.
- [ ] `commander_config.json` editable.

## Baseline (budget off)

- [ ] Default `EnablePerformanceDiagnostics` false: battle loads; commander mode, doctrine, rally, markers, and diagnostics behave as before Slice 24 (cadence may align to new defaults).

## Commander and doctrine cadence

- [ ] Set `CommanderScanIntervalSeconds` to a large value (e.g. `10`) and confirm commander presence log lines are not emitted every frame.
- [ ] Set `DoctrineScanIntervalSeconds` high; doctrine debug and cavalry doctrine logs do not advance every frame (shared `DoctrineScan` gate).

## Rally and cavalry sequence

- [ ] Set `RallyAbsorptionIntervalSeconds` high; rally absorption debug does not fire every frame.
- [ ] Enable native cavalry sequence; set `CavalrySequenceIntervalSeconds` to `0.5` and confirm orchestrator tick is not per-frame (gate + `TickSequence` dt).

## Targeting and markers

- [ ] Set `TargetingIntervalSeconds` high; ground target marker refresh is not every frame while markers remain enabled.
- [ ] Set `MarkerTickIntervalSeconds` high; marker lifetime decay still advances only on gated `Tick` batches (larger dt steps when the gate opens).

## Diagnostics and performance reporting

- [ ] Set `EnablePerformanceDiagnostics` true; confirm periodic debug line `perf budget` (via `ConfigReloadCheck` interval) without spamming every frame.
- [ ] With diagnostics visible (F9), confirm an extra `— perf —` line appears on the summary when performance diagnostics are enabled.
- [ ] Set `WarnOnOverBudget` true and use very small intervals to stress scans; at most one over-budget warning per `PerformanceWarningThrottleSeconds` (if any category exceeds its interval in measured duration).

## Mission end

- [ ] End battle or leave mission: no throw; next battle gates reset (scans resume from a fresh cadence).
