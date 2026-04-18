# Slice 8 — Commander presence model (formations)

## Purpose

Introduce a **read-only Commander Presence Model** so later doctrine and formation gates can ask “who leads this block?” without issuing orders, moving troops, or restricting formations. Detection runs through **`FormationDataAdapter`** so TaleWorlds API drift stays in one place. **`CommanderAssignmentService`** composes policy from **`CommanderConfig`** / **`CommanderDetectionSettings`** and returns **`CommanderPresenceResult`** (`Found` / `Missing` / `Uncertain`).

## Commander hierarchy (detection order)

1. **Engine formation captain** — `Formation.Captain` (confirmed on pinned `TaleWorlds.MountAndBlade` reference assemblies). Honours **`RequireHeroCommanderForAdvancedFormations`** and **`AllowCaptainCommander`**.
2. **Hero in formation** — agents where `Agent.IsHero` is true; prefers `Mission.MainAgent` when present in the list.
3. **Captain-like** — living agents with `CanLeadFormationsRemotely`, subject to the same policy flags as (1).
4. **Configured fallbacks** — only if enabled in config:
   - **Sergeant** — `Team.IsPlayerSergeant` and `Mission.MainAgent` assigned to this formation (**UNCERTAIN** across all mission archetypes; safe try/catch in callers).
   - **Highest tier** — largest `BasicCharacterObject.Level` among enumerated agents (**UNCERTAIN** as a proxy for true troop tier / rank).
5. **Missing** — no candidate meets **MinimumCommandAuthorityScore** after scoring.

If **`Team.GeneralAgent`** exists but is **not** in the formation, the service returns **`Uncertain`** with no commander object (diagnostic only; no restriction).

## Detection strategy

- **Alive gate:** `Agent.IsActive()` and `Health > 0.01f` before any role counts.
- **Authority score (0..1):** base living weight, bonuses for hero / engine captain / `CanLeadFormationsRemotely`, normalized leadership & tactics from `BasicCharacterObject.GetSkillValue(DefaultSkills.*)` when the adapter succeeds, optional tier bump for the highest-level fallback path. Result is clamped to **[0, 1]** and compared to **`MinimumCommandAuthorityScore`**.
- **Skills:** when `TryGetAgentLeadershipTactics` fails, the service uses neutral **0.5 / 0.5** normalized placeholders and marks the skill portion as **not certain** for that candidate.

## Adapter boundaries

All version-sensitive reads stay in **`FormationDataAdapter`**:

| Method | Behavior |
| --- | --- |
| `TryGetFormationAgents` | Enumerates units via `ApplyActionOnEachUnit` (same body as legacy `TryGetAgents`). |
| `TryGetFormationCaptain` / `TryGetFormationCommander` | Reads `Formation.Captain`; commander alias duplicates captain with a distinct message for policy layers. |
| `TryGetHeroAgents` | Filters formation agents using `TryGetAgentHeroFlag`. |
| `TryGetAgentHeroFlag` | Reads `Agent.IsHero`. |
| `TryGetAgentTierOrRank` | Reads `BasicCharacterObject.Level` (**UNCERTAIN** vs true tier). |
| `TryGetAgentLeadershipTactics` | Reads `GetSkillValue(DefaultSkills.Leadership/Tactics)` (**UNCERTAIN** if skill tables or defaults differ by module/game version). |

Failures are always **non-throwing** `FormationDataResult.Failure(...)`.

## Uncertain APIs (explicit)

| Topic | Status |
| --- | --- |
| `Formation.Captain` vs product language “commander” | **GO** on ref DLL; semantics vs siege/custom battle still **UNCERTAIN** — validate in-game on pinned build. |
| `Team.Leader` vs `Team.GeneralAgent` for “who commands the army” | **UNCERTAIN** (Slice 0 research); this slice only logs **Uncertain** when general is out-of-formation. |
| `BasicCharacterObject.Level` as “tier” | **UNCERTAIN** proxy only. |
| Sergeant fallback | **UNCERTAIN** mission coverage; wrapped defensively. |
| `DefaultSkills` leadership/tactics | **GO** on reference assemblies; still treat as **UNCERTAIN** across future TaleWorlds updates — adapter catches and surfaces failure. |

## Tests

Manual checklist: `docs/tests/manual-test-checklist-slice-8.md`.

## Audit

- **A:** “Formation restrictions can be implemented immediately.”
- **¬A:** “Restrictions require reliable commander/captain/hero detection first.”
- **A\***: “Build Commander Presence Model before doctrine eligibility or formation restriction.”
