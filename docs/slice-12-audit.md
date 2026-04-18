# Slice 12 audit — native order execution (minimal set)

> **Hard gate — do not implement this slice until research is done.** See [`slice-hard-gates.md`](slice-hard-gates.md) and complete [`research/native-order-hooks.md`](research/native-order-hooks.md) for your game version.

## Recommended execution order (depends on)

1. **Slice 10:** `CommandIntent` + `CommandRouter.Validate`  
2. **Slice 11:** `GroundTargetResolver` + `ResolvedGroundPosition` for **G**  
3. **Slice 12:** This slice — **`NativeOrderExecutor`** after validation  

## Audit logic (summary)

- **A:** Implement full command system now.  
- **¬A:** Native order execution is the highest-risk integration after camera bridging.  
- **A\*:** Execute only three minimal commands through a contained **`NativeOrderExecutor`** (public `OrderController` APIs; research in `docs/research/native-order-hooks.md`).

## Goal

After **`CommandRouter.Validate`** succeeds, **`NativeOrderExecutor.Execute`** issues real orders via **`OrderController`** for the **player’s** `MainAgent` team. Debug keys (**H** / **C** / **G**) run **validate → execute** while RTS is on and a **friendly** formation is selected. **G** still requires a resolved ground target (Slice 11).

## Flow

1. **`CommandRouter.ExecuteValidated`** — if validation fails, returns **`CommandExecutionResult`** with `Executed == false` and the validation message (no engine call).  
2. **`NativeOrderExecutor`** — guards `MissionEnded`, `Scene`, `MainAgent`, `OrderController`, `IsFormationSelectable`; snapshots and restores **`SelectedFormations`** around the issue call.  
3. **`TacticalFeedbackService.ShowCommandExecutionResult`** — **`[Cmd] Executed …`** or throttled **`[Cmd] Not executed …`**.

## Types

| Type | Role |
|------|------|
| `CommandExecutionResult` | `Executed`, `Message`, `Type`; `Success` / `Failure` factories. |
| `NativeOrderExecutor` | Maps `Charge` / `HoldPosition` / `MoveToPosition` to `OrderController` APIs. |
| `CommandRouter.ExecuteValidated` | Validates first, then calls the executor. |

## Non-goals (this slice)

- No advanced orders (retreat line, facing, transfers, AI control toggles).  
- No Harmony patches.  
- No Gauntlet / order UI customization.  

## Research artifact

See **`docs/research/native-order-hooks.md`** for assembly member tables and the chosen issuance path.

## Manual checks

1. **Build** passes.  
2. RTS on, valid friendly formation: **C** charges, **H** holds ground, **G** moves when ground target is valid.  
3. Invalid formation / RTS off / no main agent: **safe** `Not executed` message, **no crash**.  
4. Unsupported enum values (if introduced later): executor returns failure, **no throw**.  
5. **Battle end:** mission cleanup unchanged; avoid spamming orders after `MissionEnded` (executor rejects).  
