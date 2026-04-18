# Slice 10 audit — command routing skeleton (validate only)

## Recommended execution order (depends on)

1. **Slice 6:** Input ownership guard  
2. **Slice 7:** Config file + control profile  
3. **Slice 8:** Formation query + camera focus  
4. **Slice 9:** Lightweight tactical feedback  
5. **Slice 10:** This slice — `CommandIntent` + validation only  

## Audit logic (design rationale)

- **A:** You could start issuing native orders immediately.
- **¬A:** Bannerlord command execution is fragile; you want a **validated intent** boundary before touching engine order APIs.
- **A\*:** Build **`CommandIntent`** and **`CommandRouter.Validate`** first; defer real `IssueOrder` / formation AI wiring to a later slice.

## Goal

Convert **selected formation + requested command** into a **`CommandIntent`**, run **`CommandRouter.Validate`**, and surface the outcome via **`TacticalFeedbackService`**. **No real Bannerlord orders** are issued; no Harmony; no overlay; no mouse picking (fallback position = RTS camera pose / agent).

## Flow

1. **Debug keys** (RTS on, seeded camera): **H** → `HoldPosition`, **C** → `Charge`, **G** → `MoveToPosition` (uses context fallback `Vec3`).
2. **`CommandContext`** carries `Mission`, `SelectedFormation`, `RtsModeEnabled`, and fallback world position.
3. **`CommandRouter.Validate`** rejects: RTS off, null mission, unusable/null formation (for non-`None` types), missing position/direction when required, non-finite vectors.
4. **`TacticalFeedbackService.ShowCommandValidation`** prints `[Cmd] OK:` or `[Cmd] Rejected:`.

## Types

| Type | Role |
|------|------|
| `CommandType` | Enum of supported high-level commands. |
| `CommandIntent` | Request payload (type, formation, optional position/direction, flags, source tag). |
| `CommandContext` | Mission + selection + RTS flag + camera fallback `Vec3` (no mouse pick yet). |
| `CommandValidationResult` | `Valid` / `Invalid` factory helpers. |
| `CommandRouter` | `Validate` only; try/catch; never calls native order APIs. |
| `FormationSelectionState.TryGetSelectedFormation` | Optional helper for callers that need a guarded selection check. |

## Non-goals (this slice)

- No `IssueOrder`, no agent AI changes, no Gauntlet / MCM.

## Manual checks

1. **Build** passes.
2. **Custom battle** loads; **RTS** on; **select/cycle** a formation.
3. **H** / **C**: validation line appears (`[Cmd] OK:` or `[Cmd] Rejected:`).
4. Command with **no selected formation**: safe **Rejected** (no crash).
5. **RTS off**: debug keys → **Rejected** (e.g. RTS mode is off).
6. **No native order**: troops unchanged; codebase has **no** `IssueOrder` / order execution in this path.
