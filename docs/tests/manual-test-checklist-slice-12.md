# Manual test checklist — Slice 12 (commander rally absorption model)

Use a **custom battle** or any battle where `CommanderMissionView` runs. Enable debug logs if needed (`EnableRallyAbsorptionDebug` in `config/commander_config.json`).

- [ ] **Build passes** (`dotnet build` on `Bannerlord.RTSCameraLite.csproj`).
- [ ] **Custom battle loads** without exceptions from rally code.
- [ ] **`CommanderRallyPlanner`** runs (no crash when formations tick).
- [ ] **Commander-led formation** produces non-degraded rally state in logs when a commander is detected (`Rally absorption: …` when debug on).
- [ ] **No-commander formation** still scans safely; counts may show zero absorbable / assigned; no crash.
- [ ] **`CommanderAbsorptionZone`** classifies distances without throwing (mixed troop positions).
- [ ] **Only close troops** count as absorbable (compare in-game spread vs `CommanderAbsorptionRadius`).
- [ ] **Slot assignment skips far troops** (assigned count ≤ absorbable when troops are spread out).
- [ ] **Commander agent** is not given a front-row slot (commander excluded from assigner input list).
- [ ] **Missing rank / equipment data** does not crash doctrine, composition, layout, or assigner paths.
- [ ] **Debug output is throttled** (interval ≈ `RallyScanIntervalSeconds`, not every frame).
- [ ] **No native orders** issued by this slice (no new `OrderController` / movement calls in Slice 12 code paths).

## Optional spot-checks

- [ ] Lower **`CommanderAbsorptionRadius`** in JSON and confirm absorbable / assigned counts drop.
- [ ] Kill or rout the **commander agent** and confirm absorption degrades (`CommanderDead` states, no slot spam).
