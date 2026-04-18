# Slice 13 audit — minimal command feedback markers

## Recommended execution order (depends on)

1. **Slice 12:** `NativeOrderExecutor` + `CommandExecutionResult`  
2. **Slice 13:** This slice — `CommandMarkerService` + optional world burst + throttled text  

## Audit logic (summary)

- **A:** A command can execute silently.  
- **¬A:** Silent tactical commands feel broken and are hard to debug.  
- **A\*:** Add minimal command marker feedback (short-lived state, optional **public** `Mission.AddParticleSystemBurstByName` when a particle name is verified for the build, plus **throttled** text fallback) **without** building the full overlay system; marker code must **never** break order execution.

## Goal

After a successful **`MoveToPosition`**, surface a **2.5s** logical marker at the issued world position (`CommandMarkerState` + `MarkerLifetime`). **Charge** / **Hold** optionally get a **throttled** “no map pin” line. No overlay, no formation labels, no persistent HUD icons, no MCM.

## Flow

1. **`NativeOrderExecutor`** returns `CommandExecutionResult.Success(..., markerWorldPosition: Vec3)` for move.  
2. **`RTSCameraMissionBehavior.ProcessDebugCommand`** calls **`CommandMarkerService.AddMarker`** when `MarkerWorldPosition` is set.  
3. **`CommandMarkerService`** tries an optional one-shot particle (constant name; **null** disables until researched); on skip/failure calls **`TacticalFeedbackService.ShowPositionalMarkerFallback`**.  
4. **`OnMissionTick`** calls **`CommandMarkerService.Tick(dt)`** to expire the marker safely.  
5. Marker service is **`Clear()`** on RTS toggle, mission end cleanup, and behavior remove.

## Types

| Type | Role |
|------|------|
| `CommandMarkerState` | `Position`, `CommandType`, `RemainingSeconds`, `Label`, `Active`. |
| `MarkerLifetime` | Default **2.5s**. |
| `CommandMarkerService` | `AddMarker`, `Tick`, optional burst + fallback text (all uncertainty internal). |

## Particle name (owner)

`CommandMarkerService.OptionalMoveMarkerParticleName` is **`null`** in repo: default path is **text-only marker feedback** (still satisfies “marker or fallback”). To enable bursts, set it to a string verified on **your** game build via ILSpy / in-game test (`Mission.AddParticleSystemBurstByName(string, MatrixFrame, bool)` is public on `TaleWorlds.MountAndBlade.Mission`).

## Non-goals

- No Gauntlet layer, no UIExtenderEx, no MCM.  
- No formation identity labels on the marker.  

## Manual checks

1. **Build** passes.  
2. Successful **G**: throttled **`[Marker] MoveToPosition`** line with coordinates (or burst if particle name enabled and valid).  
3. Rapid **G**: no crash; throttle limits spam.  
4. Marker **expires** after ~2.5s (`Tick` clears state).  
5. **C** / **H**: optional **`[Marker] … no map pin`** line, throttled.  
6. If `AddMarker` throws internally, **orders still succeed** (marker swallowed).  
