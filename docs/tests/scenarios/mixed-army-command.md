# Scenario: mixed army — multi-formation diagnostics

## Goal

Prove **multi-formation** `[Diag]` block (`;;` separated) and stable behavior when switching selection across **infantry + cavalry** (and optional ranged).

## Setup

1. Custom battle: player army with **at least two** non-empty formation types (e.g. infantry + cavalry).
2. `EnableDiagnostics: true`, `DiagnosticsRefreshIntervalSeconds: 1.0` (default ok).

## Steps

1. Commander **ON**; diagnostics **F9 ON**.
2. Wait one full refresh; capture the **`[Diag]`** line showing **two** formation summaries separated by **` ;; `**.
3. Select **second** formation; on next refresh, confirm ordering or labels reflect current battle (formation class token).
4. Cycle selection 3×; ensure **no crash** and throttle still prevents per-frame messages.

## Pass criteria

- Combined line length reasonable (truncation with `…` if huge army list — acceptable).
- Tokens remain readable for portfolio capture.

## Evidence

- One screenshot of full multi-formation `[Diag]` line.
- 20s clip switching formations twice.
