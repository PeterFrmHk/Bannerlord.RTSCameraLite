# Slice-based development

This repository uses **vertical slices**: each slice delivers a **small, testable** increment with **documentation evidence**. Slices are how the project stays honest about **what is shipped** versus **what is planned** (commander doctrine, cavalry cadence, default RTS entry).

## What every slice should include

1. **Purpose** — one paragraph “why this slice exists.”
2. **Function spec** — observable behaviors and boundaries (often in `docs/slice-N-audit.md`).
3. **Implementation summary** — code areas touched (for engineers).
4. **Tests** — manual checklist rows in `docs/manual-test-checklist.md` (automated tests when added later).
5. **Audit** — tradeoffs, risks, non-goals (`docs/slice-N-audit.md`).
6. **Portfolio evidence** — what a reviewer can verify without reading C# (checklist + audit + media plan clips when applicable).

## Hard gates (research before implementation)

Some slices touch fragile engine surfaces. See **`docs/slice-hard-gates.md`** before implementing:

- **Slice 5** — real camera apply + restore  
- **Slice 12 (roadmap alternate)** — native order execution (see `docs/slice-12-audit.md`; hard gate)

## Slice docs under `docs/slices/`

| Slice | Foundation / notes |
| --- | --- |
| 1 | [`slice-1-foundation.md`](slice-1-foundation.md) — loadable module only |
| 2 | [`slice-2-mission-runtime-shell.md`](slice-2-mission-runtime-shell.md) — battle-only `MissionView` shell, commander mode state, Backspace toggle |
| 3 | [`slice-3-adapter-boundaries.md`](slice-3-adapter-boundaries.md) — adapter skeletons (`CameraBridge`, orders, formation reads, Backspace guard) |
| 4 | [`slice-4-internal-camera-pose-movement.md`](slice-4-internal-camera-pose-movement.md) — internal pose + input snapshot; manual checklist [`manual-test-checklist-slice-4.md`](../tests/manual-test-checklist-slice-4.md) |
| 5 | [`slice-5-real-camera-bridge.md`](slice-5-real-camera-bridge.md) — typed `CameraBridge` + restore; manual checklist [`manual-test-checklist-slice-5.md`](../tests/manual-test-checklist-slice-5.md) |
| 7 | [`slice-7-backspace-conflict-input-ownership.md`](slice-7-backspace-conflict-input-ownership.md) — Backspace policy + ownership state; manual checklist [`manual-test-checklist-slice-7.md`](../tests/manual-test-checklist-slice-7.md) |
| 8 | [`slice-8-commander-presence-model.md`](slice-8-commander-presence-model.md) — commander presence detection; `CommanderAssignmentService` |
| 9 | [`slice-9-commander-anchor-behind-formation.md`](slice-9-commander-anchor-behind-formation.md) — anchor behind formation (compute only); manual checklist [`manual-test-checklist-slice-9.md`](../tests/manual-test-checklist-slice-9.md) |
| 10 | [`slice-10-formation-doctrine-profile.md`](slice-10-formation-doctrine-profile.md); manual checklist [`manual-test-checklist-slice-10.md`](../tests/manual-test-checklist-slice-10.md) |
| 11 | [`slice-11-formation-eligibility-rules.md`](slice-11-formation-eligibility-rules.md); manual checklist [`manual-test-checklist-slice-11.md`](../tests/manual-test-checklist-slice-11.md) |
| 12 | [`slice-12-commander-rally-absorption-model.md`](slice-12-commander-rally-absorption-model.md) — rally bands + layout planning only; manual checklist [`manual-test-checklist-slice-12.md`](../tests/manual-test-checklist-slice-12.md) |
| 13 | [`slice-13-cavalry-spacing-charge-release.md`](slice-13-cavalry-spacing-charge-release.md); manual checklist [`manual-test-checklist-slice-13.md`](../tests/manual-test-checklist-slice-13.md) |

## Slice audit index (repository root)

Audit files live under `docs/` (historical layout):

| Slice | Audit |
| --- | --- |
| 1 | `docs/slice-1-audit.md` |
| 2 | `docs/slice-2-audit.md` |
| 3 | `docs/slice-3-audit.md` |
| 4 | § Audit in `docs/slices/slice-4-internal-camera-pose-movement.md` |
| 5 | `docs/slice-5-audit.md` |
| 7 | § Audit in `docs/slices/slice-7-backspace-conflict-input-ownership.md` |
| 9 | § Audit in `docs/slices/slice-9-commander-anchor-behind-formation.md` |
| 6 | `docs/slice-6-audit.md` |
| 7 | `docs/slice-7-audit.md` |
| 8 | `docs/slice-8-audit.md` |
| 9 | `docs/slice-9-audit.md` |
| 10 | `docs/slice-10-audit.md` |
| 11 | `docs/slice-11-audit.md` |
| 12 | `docs/slice-12-audit.md` |
| 13 | `docs/slice-13-audit.md` (minimal markers; see roadmap renumbering note) |

**Roadmap (doctrine-aligned, includes future slices):** `docs/slice-roadmap.md`

**Manual tests:** `docs/manual-test-checklist.md`
