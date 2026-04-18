# Base game formation layout & geometry scan (research only)

**Scope:** Research only. No `src/` production code was changed. Findings are from **local Steam-installed** `TaleWorlds.MountAndBlade.dll` (game **Native `SubModule.xml` → `v1.3.15`**) via **PowerShell reflection** (`Assembly.LoadFrom`, `GetProperty`, `GetMethods`). Patterns only — no decompiled bodies.

**Related:** [`base-game-order-scan.md`](base-game-order-scan.md), [`native-cavalry-command-sequence.md`](native-cavalry-command-sequence.md), [`implementation-decision-slice0.md`](implementation-decision-slice0.md).

---

## 1. Environment

| Field | Value |
| --- | --- |
| **Inspect path** | `<install>\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll` |
| **Tool** | PowerShell 5.x + .NET reflection |
| **Date** | **2026-04-18** |

---

## 2. Core type: `Formation`

`Formation` is the **simulation** object for one tactical block. It exposes **width/depth**, **spacing**, **order anchor positions**, **facets** (`FacingOrder`, `ArrangementOrder`, …), and hooks when units or spacing change.

### 2.1 Width, depth, spacing (public surface)

| Member | Kind | Notes |
| --- | --- | --- |
| `Width` | property | Overall formation width along arrangement. |
| `Depth` | property | Formation depth. |
| `UnitSpacing` | property | Spacing between units (also see `OnUnitSpacingChanged`). |
| `MinimumWidth` / `MaximumWidth` | properties | Bounds for width changes. |
| `UnitDiameter` | property | Typical unit footprint — useful for spacing heuristics. |

**UNCERTAIN:** exact coordinate frame (world vs formation-local) for `Width`/`Depth` without in-game logging; treat as **native-defined** and validate before UI math.

### 2.2 Position & facing anchors

| Member | Kind | Notes |
| --- | --- | --- |
| `CurrentPosition` | property | Current formation origin / anchor (**semantic detail UNCERTAIN**). |
| `OrderPosition` | property | Order-driven anchor. |
| `OrderGroundPosition` | property | Ground-projected order position. |
| `OrderLocalAveragePosition` | property | Local-space average of order-related positions. |
| `OrderPositionIsValid` | property | Guard before reading positions. |
| `SmoothedAverageUnitPosition` | property | Smoothed centroid of units. |
| `CachedAveragePosition` / `CachedMedianPosition` | properties | Cached aggregates (tick-aligned — **UNCERTAIN** refresh cadence). |
| `FacingOrder` | property | Current facing facet; set via `SetFacingOrder`. |
| `ArrangementOrder` | property | Current arrangement facet; set via `SetArrangementOrder`. |

### 2.3 Row / rank / slot layout (via `Arrangement`)

| Member | Kind | Notes |
| --- | --- | --- |
| `Arrangement` | property | Type: **`IFormationArrangement`**. Primary API for **per-unit slots**, ranks, and shape. |
| `SetPositioning` | method | Adjusts positioning parameters (exact semantics **UNCERTAIN** without ILSpy body read). |

**`IFormationArrangement`** (interface; public methods observed include):

- **Rank / file geometry:** `get_RankCount`, `get_RankDepth`, `get_Width`, `get_Depth`, `get_FlankWidth`, `get_MinimumFlankWidth`, `get_MinimumWidth`, `get_MaximumWidth`.
- **Per-unit queries:** `GetWorldPositionOfUnitOrDefault`, `GetLocalPositionOfUnitOrDefault`, `GetLocalPositionOfUnitOrDefaultWithAdjustment`, `GetUnit`, `GetAllUnits`, `GetPlayerUnit`, `GetOccupationWidth`.
- **Shape events:** `add_OnWidthChanged` / `remove_OnWidthChanged`, `add_OnShapeChanged` / `remove_OnShapeChanged`.

**Interpretation:** **Row/rank data** is not exposed only as primitive `int` fields on `Formation`; the **`Arrangement`** graph is the **safest read path** for slot-aware layout.

### 2.4 Order position lock

| Member | Kind | Notes |
| --- | --- | --- |
| `OrderPositionLock` | property | Reflection on this install reports **`PropertyType` as `System.Object`** with a public getter/setter in metadata — **UNCERTAIN** whether this is metadata noise, an opaque handle type, or a toolchain artifact. **BLOCKER** for *typed* mod code until verified in ILSpy against **your** exact DLL. |
| `GetReadonlyMovementOrderReference` | method | Read movement facet without cloning — **UNCERTAIN** threading/lifetime rules. |

---

## 3. Facet types (layout-adjacent)

### 3.1 `ArrangementOrder`

Static helpers observed include `GetUnitSpacingOf(ArrangementOrderEnum)`, `GetUnitLooseness(ArrangementOrderEnum)`, `TransposeLineFormation(Formation)`, defensiveness scoring methods, and equality operators.

**Pattern:** spacing tightness is derived from **arrangement enum** + helpers, not only from `Formation.UnitSpacing`.

### 3.2 `FacingOrder`

- Factory (observed signature text): **`FacingOrderLookAtDirection(Vec2)`** — verify exact static method spelling in ILSpy on your DLL (reflection logs can truncate).

**UNCERTAIN:** full factory list — scan again with ILSpy for overloads.

---

## 4. Unit-level placement under pending orders

`Formation` exposes multiple overloads of:

- `GetUnitPositionWithIndexAccordingToNewOrder(...)`
- `GetOrderPositionOfUnit(...)`
- `GetUnavailableUnitPositionsAccordingToNewOrder(...)`
- `CreateNewOrderWorldPosition(...)`

**Use:** predicting where units **would** stand after an order — **high value** for RTS ghost markers. **Risk:** calling with inconsistent order state may be expensive or stale; **UNCERTAIN** performance without profiling.

---

## 5. Events (read-only telemetry)

| Event | When useful |
| --- | --- |
| `OnUnitSpacingChanged` | Detect native spacing changes. |
| `OnWidthChanged` | Width-driven UI / doctrine gates. |
| `OnAfterArrangementOrderApplied` / `OnBeforeMovementOrderApplied` | Ordering pipeline instrumentation (**read-only subscribe** in research sense). |

---

## 6. Recommended “safest formation data path” (for later slices)

1. **Coarse pose:** `OrderPositionIsValid` → `OrderPosition` / `OrderGroundPosition` + `FacingOrder`.
2. **Shape / ranks:** `Formation.Arrangement` → `RankCount`, `RankDepth`, `Width`, `Depth`, `GetWorldPositionOfUnitOrDefault`.
3. **Spacing policy:** combine `UnitSpacing` with `ArrangementOrder` + static `ArrangementOrder` helpers.
4. **Avoid** hard-coding `OrderPositionLock` semantics until ILSpy confirms the real CLR type and legal transitions.

---

## 7. Follow-ups

1. ILSpy `Formation.OrderPositionLock` — confirm actual type and when the engine sets/clears it (cavalry sequence).  
2. In-game log: `CurrentPosition` vs `OrderPosition` during Move → Charge transitions.  
3. Align any mod `Formation.Team` assumptions with [`base-game-order-scan.md`](base-game-order-scan.md) (`PlayerOwner` vs `Team` discrepancy).
