# Slice 19 — Command marker temporary visual target feedback

## Goal

Give the player **short-lived** awareness of command targets (ground sample, move destination, cavalry charge target, reform point) without a tactical overlay, persistent HUD labels, Gauntlet, MCM, or UIExtenderEx.

## Components (`src/UX`)

| Type | Role |
|------|------|
| `CommandMarkerType` | `GroundTarget`, `MoveTarget`, `ChargeTarget`, `ReformPoint`, `CommanderAnchor`, `RallyPoint`, `Warning` |
| `CommandMarkerState` | Active flag, type, position, label, remaining time, source, `VisualRendered`, `Reason` |
| `CommandMarkerSettings` | Feature flags + lifetimes + refresh throttle; `FromConfig(CommanderConfig)` |
| `MarkerLifetime` | TTL helper per marker type |
| `MarkerRenderResult` | Result of optional world render attempt |
| `CommandMarkerService` | `AddMarker`, `AddGroundTargetMarker`, `AddChargeTargetMarker`, `AddReformMarker`, `Tick`, `Cleanup` |

## Visual vs text

- **Visual path**: isolated in `CommandMarkerService` (`TryRenderVisual`). Uses `Mission.AddParticleSystemBurstByName` only when `OptionalMarkerParticleName` is set after Slice 0 research; otherwise returns skipped.
- **Fallback**: when visuals are off or fail and `EnableFallbackTextMarkers` is true, `TacticalFeedbackService.ShowCommandMarkerFallback` emits a throttled `[Marker]` player message.
- **Never throws**; failures do not affect command routing or native orders.

## Config (`CommanderConfig`)

| Key | Default |
|-----|---------|
| `EnableCommandMarkers` | `true` |
| `EnableFallbackTextMarkers` | `true` |
| `DefaultMarkerLifetimeSeconds` | `2.5` |
| `ChargeMarkerLifetimeSeconds` | `3.0` |
| `ReformMarkerLifetimeSeconds` | `3.0` |
| `MarkerRefreshThrottleSeconds` | `0.25` |

Legacy JSON without these keys is merged via `CommanderConfigService.ApplyOmittedSlice19CommandMarkerDefaults`.

## Mission integration (`CommanderMissionView`)

- **Ground**: while commander mode is on and camera pose exists, throttled samples use `GroundTargetResolver` + `AddGroundTargetMarker`.
- **Move**: after a validated `AdvanceOrMove` debug router check (`RunCommandRouterDebug`), `MoveTarget` marker at `TargetPosition`.
- **Charge**: when native cavalry sequence starts successfully (debug key **N** path), marker at enemy formation center (`FormationTargetResult`).
- **Reform**: when native cavalry sequence reports reform-ready (edge `ReformAllowed` false→true), marker at source formation center.
- **Tick / cleanup**: `CommandMarkerService.Tick` each mission frame branch; `Cleanup` + `TacticalFeedbackService.ResetSession` on lifecycle cleanup.

## Tactical types

- `GroundTargetResult` / `GroundTargetResolver` / `TerrainProjectionService` (existing) are compiled for this slice.
- `FormationTargetResult` — small struct for charge target world position + optional formation reference.

## Non-goals

Full overlay, persistent formation labels, radial UI, new native orders, per-agent steering.
