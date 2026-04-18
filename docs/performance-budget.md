# Performance budget — RTS Commander Lite

Slice 24 introduces **throttled update gates** so expensive formation and doctrine work does not run every mission frame. This is **cadence only**: no new gameplay rules and no threading.

## Why scans are throttled

Formation iteration, doctrine scoring, rally/absorption sync, diagnostics snapshots, and ground-ray targeting scale with formation count and battle size. Running them every frame wastes CPU and can hitch the simulation. Gates enforce minimum spacing between passes using a monotonic mission clock (`dt` sum in `CommanderMissionView`).

## Update categories

| Category | Typical work |
| --- | --- |
| `Targeting` | `GroundTargetResolver.TryResolveFromCamera` for ground markers. |
| `CommanderScan` | Commander presence enumeration; when commander mode is on, anchor zone scan runs in the same gated pass. |
| `DoctrineScan` | Doctrine debug aggregation and cavalry doctrine evaluation (shared gate). |
| `EligibilityScan` | Eligibility debug lines inside the commander scan (separate cadence when enabled). |
| `RallyAbsorptionScan` | Rally planner + absorption + slot sync over player formations. |
| `CavalrySequenceTick` | `CavalryNativeChargeOrchestrator.TickSequence` for active sequences. |
| `FeedbackTick` | Drives `PerformanceDiagnosticsService.Tick` when `EnablePerformanceDiagnostics` is true. |
| `MarkerTick` | `CommandMarkerService.Tick` (marker lifetime decay) with accumulated `dt` between opens. |
| `DiagnosticsTick` | Heavy per-formation diagnostics snapshot build (in addition to Slice 20 refresh throttle). |
| `ConfigReloadCheck` | Rate limit for optional perf summary logging (not a file reload in this slice). |

## Default intervals (code + `CommanderPerformanceBudget`)

See `CommanderPerformanceBudget` default constants and `CommanderConfigDefaults` / `config/commander_config.json` for overrides:

- Targeting `0.25`s  
- Commander scan `3.0`s  
- Doctrine scan `3.0`s (same property as `DoctrineScanIntervalSeconds` in config)  
- Eligibility scan `3.0`s  
- Rally absorption `3.0`s (falls back to `RallyScanIntervalSeconds` when the dedicated field is `0`)  
- Cavalry sequence `0.25`s  
- Feedback tick `0.1`s  
- Marker tick `0.1`s  
- Diagnostics tick uses `DiagnosticsTickIntervalSeconds` when `> 0`, otherwise `DiagnosticsRefreshIntervalSeconds`  
- Config reload check `5.0`s  

## What may still run every frame

- Commander mode toggle handling, backspace guard, native input guard ticks.
- Commander camera pose update and camera bridge apply when commander mode is on.
- Command-router **key edge** detection (must not miss `IsKeyReleased`).

## What must not run every frame

- Full player-formation doctrine / eligibility / rally / diagnostics sweeps.
- Native cavalry orchestrator tick.
- Ground targeting ray for markers.
- Batched marker decay ticks.

## Configuration keys

All intervals and flags live on `CommanderConfig` under the **Performance budget (Slice 24)** group in `CommanderConfigSchema.DocumentationGroups`:

- `EnablePerformanceDiagnostics`, `WarnOnOverBudget`, `PerformanceWarningThrottleSeconds`
- Interval fields listed above plus `ConfigReloadCheckIntervalSeconds`

## Testing method

1. Enable debug logging.
2. Temporarily raise specific interval fields and confirm the related logs or diagnostics updates slow down.
3. Enable `EnablePerformanceDiagnostics` and observe the occasional `perf budget` debug line and optional `— perf —` appendix on the F9 diagnostics summary.
4. See `docs/tests/manual-test-checklist-slice-24.md`.
