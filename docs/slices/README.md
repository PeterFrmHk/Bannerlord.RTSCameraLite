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
- **Slice 12** — native order execution  

## Slice docs under `docs/slices/`

| Slice | Foundation / notes |
| --- | --- |
| 1 | [`slice-1-foundation.md`](slice-1-foundation.md) — loadable module only |
| 2 | [`slice-2-mission-runtime-shell.md`](slice-2-mission-runtime-shell.md) — battle-only `MissionView` shell, commander mode state, Backspace toggle |
| 3 | [`slice-3-adapter-boundaries.md`](slice-3-adapter-boundaries.md) — adapter skeletons (`CameraBridge`, orders, formation reads, Backspace guard) |
| 4 | [`slice-4-internal-camera-pose-movement.md`](slice-4-internal-camera-pose-movement.md) — internal pose + input snapshot; manual checklist [`manual-test-checklist-slice-4.md`](../tests/manual-test-checklist-slice-4.md) |
| 5 | [`slice-5-real-camera-bridge.md`](slice-5-real-camera-bridge.md) — typed `CameraBridge` + restore; manual checklist [`manual-test-checklist-slice-5.md`](../tests/manual-test-checklist-slice-5.md) |

## Slice audit index (repository root)

Audit files live under `docs/` (historical layout):

| Slice | Audit |
| --- | --- |
| 1 | `docs/slice-1-audit.md` |
| 2 | `docs/slice-2-audit.md` |
| 3 | `docs/slice-3-audit.md` |
| 4 | § Audit in `docs/slices/slice-4-internal-camera-pose-movement.md` |
| 5 | `docs/slice-5-audit.md` |
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
