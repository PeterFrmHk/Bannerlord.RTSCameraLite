# Compatibility — RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)

This folder documents **how the mod coexists** with Bannerlord, other modules, and future optional tooling. It is **not** a substitute for reading `SubModule.xml` or the pinned **reference assemblies** version in `Bannerlord.RTSCameraLite.csproj`.

## Compatibility philosophy

1. **Prefer public engine surfaces** — Camera and orders go through documented paths (see `docs/research/` and slice docs). Version drift is contained in **small adapter boundaries** (e.g. native order primitive executor), not scattered calls.
2. **Fail safe, log once** — Invalid missions, missing teams, or blocked native paths should not hard-crash the campaign shell when avoidable.
3. **Document over patch** — If another mod conflicts, the first response is **accurate documentation** and a **bug report template** (`docs/tests/bug-report-template.md`), not an immediate Harmony dependency.

## Native API adapter boundaries

- **Order issuance** is centralized so TaleWorlds **`OrderController`** assumptions stay in one place (see slice 14 documentation).
- **Camera** uses `MissionView` / bridge patterns described in architecture and research notes; treat any third-party camera replacement as **high risk** (see `known-conflict-zones.md`).

## Why Harmony is avoided unless necessary

- **Patch order and game updates** break silently more often than public API drift.
- Slice 0 / implementation decisions **defer Harmony** until a researched public hook is unavailable **and** the team accepts ongoing maintenance (see `docs/research/implementation-decision-slice0.md`).
- If Harmony is ever introduced, it must stay **isolated** behind an explicit policy (see `dependency-policy.md`).

## How to report conflicts

1. Use **`docs/tests/bug-report-template.md`** — include **full mod list**, **load order**, game + mod version.
2. Add a row to **`docs/tests/known-failures.md`** if the issue is accepted as a known interaction until fixed.
3. For load-order experiments, follow **`mod-stack-testing.md`** so reports are reproducible.

## Contents

| File | Topic |
| --- | --- |
| [`load-order-notes.md`](load-order-notes.md) | Placement, smoke tests after reorder, high-risk neighbors |
| [`known-conflict-zones.md`](known-conflict-zones.md) | Mod categories that tend to clash |
| [`dependency-policy.md`](dependency-policy.md) | Hard deps, Harmony, optional libs |
| [`mod-stack-testing.md`](mod-stack-testing.md) | Reproducible stacks (clean → conflict) |
| [`version-support.md`](version-support.md) | Game vs ref assembly vs tested matrix |

Slice overview: [`../slices/slice-25-compatibility-load-order-notes.md`](../slices/slice-25-compatibility-load-order-notes.md).

Public zip / module folder checklist: [`../release/public-packaging-plan.md`](../release/public-packaging-plan.md).
