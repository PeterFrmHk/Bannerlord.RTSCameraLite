# Slice 11 audit — ground target resolution (validate only)

## Recommended execution order (depends on)

1. **Slices 6–9:** Input ownership, config, formations, tactical feedback  
2. **Slice 10:** `CommandIntent` + `CommandRouter.Validate` (no orders)  
3. **Slice 11:** This slice — `GroundTargetResolver` + terrain projection for positional intents  

## Audit logic (design rationale)

- **A:** Positional commands need a stable `Vec3` on the battlefield.  
- **¬A:** Hard-coding engine raycast / cursor APIs couples the mod to one Bannerlord build and risks crashes when APIs move.  
- **A\*:** Resolve a **plausible ground point** from the RTS camera pose with defensive terrain height lookup; keep cursor-to-world isolated behind `TryResolveFromCursor` (fallback to camera path until wired). **No `IssueOrder`** and no world markers in this slice.

## Goal

While RTS mode is on, periodically sample a **ground target** from the camera. **`CommandContext`** exposes `CurrentGroundTarget` and `ResolvedGroundPosition`. Debug key **G** builds a `MoveToPosition` intent **only** from `ResolvedGroundPosition` and runs validation — **no command execution**.

## Engine API notes (terrain height)

Public `Mission`/`Scene` surface types in reference assemblies can differ by game version. `TerrainProjectionService` uses:

1. Forward projection on the **horizontal plane** from camera **yaw** (same convention as `TerrainProjectionService`: `x += sin(yaw) * d`, `y += cos(yaw) * d` in TaleWorlds horizontal axes).  
2. Optional **Z** refinement via **reflection** on `mission.Scene` for instance methods `GetHeightAtPosition(Vec2)` or `GetGroundHeightAtPosition(Vec2)` when return type is `float`.  
3. If reflection fails or throws, **Z** stays at the projected value (often near camera height); the call still returns **success** when the resulting `Vec3` is finite.

**ILSpy / reference assembly:** Confirm method names on `TaleWorlds.Engine.Scene` (or equivalent) for your Bannerlord build; extend the reflection allowlist if your client exposes a different public height API.

## Flow

1. **`UpdateGroundTargetSample`** (mission tick path): runs when RTS is enabled and the camera pose is seeded, on every **4th** formation-logic tick (`_formationLogicTick % 4 == 0`) to limit work per frame.  
2. **`GroundTargetResolver.TryResolveFromCamera`** → **`TerrainProjectionService.TryProjectCameraForwardGround`**.  
3. **`TryResolveFromCursor`**: not wired; delegates to camera resolution (uncertainty isolated here).  
4. **`BuildCommandContext`** passes `_currentGroundTarget` into **`CommandContext`**.  
5. **G** → `MoveToPosition` with `TargetPosition = context.ResolvedGroundPosition` only; if the last sample failed, **`TargetPosition`** is null → router **rejects** (“requires a target position”).  
6. **`TacticalFeedbackService`**: `ShowGroundTargetResolved` / `ShowGroundTargetFailed` use **`FeedbackThrottle`**; mission behavior avoids spamming success by only calling resolved feedback on **fail → success** transition (throttle still applies inside the UX service).

## Types

| Type | Role |
|------|------|
| `GroundTargetResult` | `Success` / `Position` / `Message`; `SuccessAt`, `Failure`. |
| `TerrainProjectionService` | Camera-forward ground point + safe terrain Z via reflection or fallback. |
| `GroundTargetResolver` | Camera resolution; cursor path stub. |
| `CommandContext` | Adds `CurrentGroundTarget`, `ResolvedGroundPosition` (`Vec3?`). |
| `CommandIntent.FromResolvedGroundTarget` | Set when **G** used a resolved ground position. |

## Non-goals (this slice)

- No native orders, no Harmony, no ground marker / Gauntlet pick ray.

## Manual checks

1. **`dotnet build -c Release`** succeeds.  
2. RTS on, pose seeded: ground sampler does **not** crash when terrain API is missing or throws.  
3. **G** with valid resolved target + valid formation: **`[Cmd] OK:`** for `MoveToPosition`.  
4. **G** when `ResolvedGroundPosition` is null (e.g. sampler not run yet or failure): **`[Cmd] Rejected:`** requiring position — **no crash**.  
5. Codebase: **no** `IssueOrder` / order execution on this path.
