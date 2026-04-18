# Manual test checklist — Slice 9 (commander anchor, compute only)

## Build

- [ ] `dotnet build .\Bannerlord.RTSCameraLite\Bannerlord.RTSCameraLite.csproj -c Release` — **0 errors**.

## Battle load

- [ ] Custom battle (or supported mission) loads with module enabled.

## Resolver / adapter safety

- [ ] No exception when `Formation` or `CommanderPresenceResult` lacks a commander (`HasAnchor == false`, safe `Reason`).
- [ ] `TryGetFormationCenter` / `TryGetFormationFacing` / `TryDetectFormationRole` / `TryGetAgentPosition` failures do **not** crash the mission tick (anchor scan wrapped in try/catch + resolver guards).

## Anchor geometry

- [ ] When center + facing + commander exist, `PreferredPosition` lies **behind** formation center along negated forward (inspect via debug breakpoint or temporary log — optional).
- [ ] `CommanderDistanceFromAnchor` and `CommanderInsideAnchorZone` update when commander walks (manual observation if logging one formation).

## Throttle / logging

- [ ] With `EnableCommanderAnchorDebug: true`, a **summary** line appears on a **~2s** cadence, not every frame.
- [ ] With `EnableCommanderAnchorDebug: false`, no anchor summary spam.

## Non-regression

- [ ] No native `SetOrder` / movement orders issued from anchor code paths (code review: `CommanderAnchorResolver` + `CommanderMissionView` scan only).
