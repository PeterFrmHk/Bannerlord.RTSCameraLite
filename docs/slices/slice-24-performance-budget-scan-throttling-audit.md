# Slice 24 — Performance budget and scan throttling

## Purpose

Centralize expensive RTS Commander work behind **monotonic gates** so formation scans, doctrine passes, rally/absorption planning, native cavalry orchestration ticks, ground targeting, marker decay batches, and diagnostics snapshot builds do not run every frame. Camera movement and lightweight input guards remain on the normal mission tick.

## Components (`src/Performance/`)

| Type | Role |
| --- | --- |
| `UpdateBudgetCategory` | Enum labels for each throttled bucket (targeting, commander scan, doctrine, eligibility, rally, cavalry sequence, feedback, markers, diagnostics, config check). |
| `CommanderPerformanceBudget` | Immutable interval set built from `CommanderConfig` with safe defaults (`FromCommanderConfig`). |
| `ThrottledUpdateGate` | `ShouldRun` / `MarkRun` / `Reset` / `ResetAll` against a mission clock (seconds); tracks run/skip counts and optional last duration for diagnostics. |
| `PerformanceBudgetSnapshot` | Per-category stats for reporting. |
| `PerformanceDiagnosticsService` | Optional debug: throttled summary via `ConfigReloadCheck` gate, `WarnOnOverBudget` with `PerformanceWarningThrottleSeconds`, `BuildScanRateSummary()` for the diagnostics block when enabled. |

## Mission wiring (`CommanderMissionView`)

- `_missionPerfClock` accumulates `dt` each tick.
- **Doctrine + cavalry doctrine** share one `DoctrineScan` gate per interval.
- **Commander presence + anchor scan** share `CommanderScan`; eligibility debug uses `EligibilityScan` when enabled.
- **Rally / absorption** uses `RallyAbsorptionScan`.
- **Native cavalry sequence** uses `CavalrySequenceTick` with `TickSequence(..., tickSeconds)` matching the budget interval.
- **Markers** decay on `MarkerTick`; **ground targeting** uses `Targeting`.
- **Diagnostics** heavy snapshot loop uses `DiagnosticsTick` in addition to existing `ShouldPublishRefresh`.
- **FeedbackTick** drives `PerformanceDiagnosticsService.Tick` when `EnablePerformanceDiagnostics` is true.
- **Mission end / behavior removal** calls `ResetAll()` on the gate and `Reset()` on performance diagnostics.

## Non-goals

- No threading, async, or background workers.
- No gameplay rule changes; only cadence of existing computations and logging.

## References

- Operator guide: `docs/performance-budget.md`
- Manual tests: `docs/tests/manual-test-checklist-slice-24.md`
