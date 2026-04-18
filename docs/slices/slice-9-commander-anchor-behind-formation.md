# Slice 9 — Commander anchor behind formation (compute only)

## Purpose

Compute a **preferred commander anchor** slightly **behind** each commanded friendly formation: `PreferredPosition`, planar **facing**, **allowed radius**, and whether the resolved commander agent is **inside** that zone. This slice is **data only** — it does **not** move agents, issue orders, restrict formations, or change morale.

## Inputs

| Input | Source |
| --- | --- |
| **`Mission` + `Formation`** | Player team formations scanned on a **throttled** timer from `CommanderMissionView`. |
| **`CommanderPresenceResult`** | `CommanderAssignmentService.DetectCommander` (Slice 8). |
| **`FormationDataAdapter`** | Formation **center**, **facing** (`Formation.Direction`), **role** hint (`QuerySystem` flags + coarse fallbacks), **agent position**. |
| **`CommanderAnchorSettings`** | Loaded from `CommanderConfig` (offsets, radius, debug flag). |

## Outputs

| Output | Meaning |
| --- | --- |
| **`CommanderAnchorState`** | `HasAnchor`, `PreferredPosition`, `PreferredFacing` (unit XY), `AllowedRadius`, `CommanderInsideAnchorZone`, `CommanderDistanceFromAnchor`, `Reason`, `IsCertain`. |
| **Debug log** (optional) | Throttled summary, e.g. `Commander anchors: 2 inside, 1 out of position, 0 without anchor.` when `EnableCommanderAnchorDebug` is true. |

## Anchor geometry

- **Center** from `TryGetFormationCenter`.
- **Forward** from `TryGetFormationFacing` (unit **behind** is `center - forward * backOffset`).
- **Back offset** chosen from role (`ShieldWall`, `Archer`, `Cavalry`, `Skirmisher`, else **default**). Role detection is **best-effort** on pinned ref assemblies; unknown role uses **default** offset.
- If facing read fails, forward defaults to `(0, 1)` and **`IsCertain`** becomes false.

## Config (`commander_config.json`)

| Key | Default | Notes |
| --- | ---: | --- |
| `DefaultCommanderBackOffset` | 6.0 | Used when role is unknown / infantry-style default. |
| `ShieldWallCommanderBackOffset` | 8.0 | When arrangement name suggests shield wall. |
| `ArcherCommanderBackOffset` | 7.0 | `QuerySystem.IsRangedFormation`. |
| `CavalryCommanderBackOffset` | 10.0 | `QuerySystem.IsCavalryFormation` or `HasAnyMountedUnit` fallback. |
| `SkirmisherCommanderBackOffset` | 9.0 | Reserved mapping when role enum exposes skirmisher horse-archer distinctly. |
| `AnchorAllowedRadius` | 4.0 | Planar distance tolerance. |
| `EnableCommanderAnchorDebug` | true | Enables throttled **debug** summary (not per-frame). |

Omitted keys in older JSON are merged in `CommanderConfigService.ApplyOmittedSlice9AnchorDefaults`.

## Non-goals

- No **agent movement**, no **native orders**, no **slot assignment**, no **formation restrictions**, no doctrine (morale / training / equipment).

## Tests

- `dotnet build -c Release`
- Manual: `docs/tests/manual-test-checklist-slice-9.md`

## Design audit (minimal)

| | Statement |
| --- | --- |
| **A** | Commander can just be attached symbolically to formation. |
| **¬A** | Commander should occupy a **command position behind** the formation nucleus. |
| **A\*** | **Compute** a preferred anchor behind each formation; **do not force movement** until a later slice wires policy + actuation. |

## See also

- [`manual-test-checklist-slice-9.md`](../tests/manual-test-checklist-slice-9.md)
- Slice 8 presence: `CommanderAssignmentService`, `CommanderPresenceResult`
- Research: `docs/research/implementation-decision-slice0.md` (formation reads)
