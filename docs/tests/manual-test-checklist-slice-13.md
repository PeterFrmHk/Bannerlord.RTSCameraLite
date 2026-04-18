# Manual test checklist — Slice 13 (cavalry spacing + charge-release doctrine)

Battle with **player cavalry** and **enemy infantry or cavalry** so distances and targets resolve. Use `EnableCavalryDoctrineDebug` in `config/commander_config.json` for doctrine logs.

- [ ] **Build passes** (`dotnet build` on `Bannerlord.RTSCameraLite.csproj`).
- [ ] **Custom battle loads** without exceptions from cavalry doctrine code.
- [ ] **`CavalrySpacingRules`** runs (no throw when analyzing composition).
- [ ] **Cavalry-heavy formation** gets `RowRankSpacingPlan.IsMountedLayout` and wider spacing than typical infantry in logs / debugger.
- [ ] **Horse-archer-heavy formation** gets `IsHorseArcherLayout` and widest spacing; **not** treated as shock cavalry for `ReleaseLockAfterCloseContact`.
- [ ] **Infantry-only formations** are **not** marked `IsMountedLayout`.
- [ ] **Position lock allowed** during assembly-related states (see `CavalryPositionLockPolicy` + plan flags).
- [ ] **Position lock release** advised when within `CavalryReleaseLockDistance` of primary enemy formation or impact heuristic fires.
- [ ] **Lock stays released** while still in close band / before reform gates (observe state transitions in logs).
- [ ] **Reform discipline blocked** before `CavalryReformDistanceFromAttackedFormation` and cooldown when commander required.
- [ ] **Reform allowed** only when policy passes (distance, cooldown, commander or `AllowCavalryReformWithoutCommander`).
- [ ] **Commander death** blocks disciplined reform when fallback is off.
- [ ] **Debug messages** are throttled / edge-triggered (not every frame spam).
- [ ] **No native commands** issued from Slice 13 paths (no new order issuance in these types).
