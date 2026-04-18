# Slice 20 — Formation diagnostics (portfolio / capture)

## Purpose

Provide **read-only**, **throttled** visibility into commander-mode internals for **testing, screenshots, and video capture** without building a final HUD or pulling in external UI frameworks. Output is **compact text** via **`TacticalFeedbackService`** (`InformationManager` path when UI is ready).

## Components

| Type | Role |
| --- | --- |
| `DiagnosticsSettings` | Snapshot of config flags + refresh interval; built from `CommanderConfig` via `FromCommanderConfig`. |
| `DiagnosticsThrottle` | Interval gate for refresh emissions (independent of UX throttle keys). |
| `CommanderDiagnosticsService` | Toggle visibility, `BuildSnapshot`, `Tick`, `Cleanup`; owns refresh throttle. |
| `FormationDiagnosticsSnapshot` | Per-formation capture: label, commander, anchor, doctrine, eligibility, rally counts, cavalry, target, native status, UTC ticks. |
| `FormationDiagnosticsFormatter` | One-line compact strings + multi-formation block (length-capped). |

## Config (`CommanderConfig`)

| Property | Default | Meaning |
| --- | --- | --- |
| `EnableDiagnostics` | true | Master switch; when false, no toggle handling or emissions. |
| `ShowDiagnosticsInCommanderModeOnly` | true | When true, refresh block only while commander mode is **enabled** (toggle still works). |
| `DiagnosticsToggleKey` | F9 | Parsed in `CommanderInputReader` (must not alias mode activation key). |
| `DiagnosticsRefreshIntervalSeconds` | 1.0 | Minimum seconds between full multi-formation refresh lines. |
| `IncludeDoctrineScores` | true | Include discipline + dominant role segments. |
| `IncludeEligibility` | true | Include abbreviated allowed formation types. |
| `IncludeRallyAbsorption` | true | Include rally / absorbable / assigned counts. |
| `IncludeCavalrySequence` | true | Include native vs planning cavalry state prefix. |
| `IncludeNativeOrderStatus` | true | Include `EnableNativeOrderExecution` / `EnableNativePrimitiveOrderExecution` summary (same for all lines in a tick). |

Missing JSON keys are merged via `CommanderConfigService.ApplyOmittedSlice20DiagnosticsDefaults`.

## Integration (`CommanderMissionView`)

- **`MaybeDiagnostics(dt)`** runs each tick after marker tick; respects mission gate and `EnableDiagnostics`.
- **F9** (configurable) toggles visibility; immediate **`[Diag] ON` / `[Diag] OFF`** feedback (short cooldown, `forceImmediate`).
- **Refresh** uses `CommanderDiagnosticsService.ShouldPublishRefresh` + `DiagnosticsSettings.DiagnosticsRefreshIntervalSeconds`.
- **Feedback** uses existing **`TacticalFeedbackService.ShowDiagnosticsSummary`** (throttle key `diagnostics-summary`).
- **Cleanup** on mission end: `CommanderDiagnosticsService.Cleanup()` + `FeedbackThrottle.ClearKey("diagnostics-summary")`.

## Example line shape

`Infantry: Cmd YES | Disc 0.62 | Role Infantry | Allow BasicLin/Loose/... | Rally 12/24 abs 3 asg 0 | Anchor OK | Cav Pl:MountedFormationAssembling | Tgt En:Cavalry | Native ex=False pr=False`

(Exact segments depend on `Include*` flags.)

## Non-goals

- No Gauntlet / ViewScreen / persistent overlay.
- No charts or log files.
- No gameplay or native order behavior changes.

## Audit

| Claim | Negation | Resolution |
| --- | --- | --- |
| **A:** Portfolio testing needs a full debug HUD. | **¬A:** HUD scope explodes and couples to engine UI. | **A\*:** Throttled text summaries from one service boundary + config gates. |

## Tests

Manual: [`docs/tests/manual-test-checklist-slice-20.md`](../tests/manual-test-checklist-slice-20.md).
