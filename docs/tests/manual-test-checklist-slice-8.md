# Manual test checklist — Slice 8 (commander presence model)

Prerequisites: supported battle mission (`CommanderMissionModeGate`), `commander_config.json` includes Slice 8 detection keys (or rely on legacy harmonization for older files).

- [ ] **Build passes** — `dotnet build -c Release` on `Bannerlord.RTSCameraLite.csproj`.
- [ ] **Custom battle loads** — start a custom battle with the mod enabled; no crash on behavior init.
- [ ] **CommanderAssignmentService initializes** — mission log shows config load; no null-ref from `CommanderMissionView` construction path.
- [ ] **Friendly formations can be scanned safely** — debug output includes `Commander scan: X/Y formations commanded` without exceptions (throttled, not every frame).
- [ ] **Commander presence returns Found/Missing/Uncertain** — spot-check logs or temporary breakpoint: `DetectCommander` / `DetectCommanderForFormation` return structured results (no throw on empty or dead formations).
- [ ] **Missing commander does not crash** — formations with no qualifying leader still tick normally; no orders issued.
- [ ] **Dead/invalid commander handled safely if testable** — kill captain in console/cheats if available; formation should not report a living commander from the dead agent.
- [ ] **Scan does not run every frame** — interval is **2.5s** (`CommanderPresenceScanIntervalSeconds`); log lines should not appear every tick.
- [ ] **No formation restrictions applied yet** — troop control and formation access behave as before this slice.
- [ ] **No native orders issued** — presence scan is read-only; verify no new `OrderController` / movement calls from this slice.

## Notes

- Player team only (`Mission.PlayerTeam`) is scanned for the summary line.
- Legacy JSON without Slice 8 keys is harmonized in `CommanderConfigDefaults.HarmonizeLegacyDetectionFields` when all detection fields deserialize to the “unset” pattern.
