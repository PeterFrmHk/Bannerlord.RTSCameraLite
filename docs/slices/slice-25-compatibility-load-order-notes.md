# Slice 25 — Compatibility, dependencies, and load order

## Purpose

Give players and contributors a **single place** to understand:

- how RTS Commander Doctrine expects to coexist with Bannerlord and other mods,
- why **adapter boundaries** and **Harmony avoidance** matter,
- how to **test load order** and **report conflicts** without guessing.

## Files created

| Path | Role |
| --- | --- |
| `docs/compatibility/README.md` | Entry point: philosophy, adapters, Harmony stance, reporting |
| `docs/compatibility/load-order-notes.md` | Placement guidance, post-reorder smoke, high-risk neighbors |
| `docs/compatibility/known-conflict-zones.md` | Mod categories (camera, orders, AI, UI stacks) |
| `docs/compatibility/dependency-policy.md` | No hard third-party baseline; optional future libs |
| `docs/compatibility/mod-stack-testing.md` | Reproducible stacks (clean → camera → command → AI) |
| `docs/compatibility/version-support.md` | Ref pin vs game build, tested matrix, mismatch reporting |
| `docs/tests/manual-test-checklist-slice-25.md` | Manual acceptance for this slice |

## Compatibility risks

1. **Camera ownership** — Any mod that permanently owns the battle camera can break commander RTS bridge restore behavior.
2. **Order pipeline** — Mods that alter `OrderController` usage, selection, or keybinds can cause **silent** double-orders or blocked orders.
3. **Version skew** — Pin in `BannerlordRefsVersion` must be **revalidated** after game updates (`version-support.md`).
4. **Optional stacks** — If MCM / ButterLib / UIExtenderEx are added later, they become a **shared failure mode** across many mods.

## Tests

- Follow **`docs/tests/manual-test-checklist-slice-25.md`**.
- For broader regression, use **`docs/tests/regression-matrix.md`** and **`docs/compatibility/mod-stack-testing.md`**.

## Audit

| Claim | Negation | Resolution |
| --- | --- | --- |
| **A:** Compatibility is implied by shipping code alone. | **¬A:** Players run unknown mod stacks and game versions. | **A\*:** Document pins, risks, load-order smoke, and reporting paths without adding runtime deps. |

## Non-goals (slice 25)

- No gameplay logic changes.
- No new NuGet / Harmony / MCM dependencies.
- No release packaging.
