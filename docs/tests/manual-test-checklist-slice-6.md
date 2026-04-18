# Manual test checklist — Slice 6 (commander JSON config)

Use a supported battle mission (see `CommanderMissionModeGate`). Config path: `Modules/Bannerlord.RTSCameraLite/config/commander_config.json` under the game install.

- [ ] **Build passes** — `dotnet build` for `Bannerlord.RTSCameraLite.csproj`.
- [ ] **Missing config creates default config** — delete `config/commander_config.json`, launch a battle once, confirm the file is recreated with default JSON.
- [ ] **Malformed config falls back safely** — replace JSON with `{ not json`, launch battle, confirm no crash; one diagnostic warning for fallback; defaults behave like stock profile.
- [ ] **StartBattlesInCommanderMode true starts Commander Mode active** — set `true`, confirm immediate ENABLED behavior / guard enter path (logs if you use debug).
- [ ] **StartBattlesInCommanderMode false starts disabled** — set `false`, confirm commander starts off until you press the activation key.
- [ ] **ModeActivationKey changes toggle key** — set e.g. `M`, confirm Backspace no longer toggles and `M` does (release edge).
- [ ] **F10 fallback works only when enabled** — with `EnableDebugFallbackToggle` false, F10 does not toggle; with true, F10 toggles when configured as F10.
- [ ] **Camera movement speed changes after config edit** — change `MoveSpeed` / `FastMoveMultiplier` / `RotationSpeedDegrees`, restart mission, confirm `CommanderCameraController.MovementSettings` reflects new clamped values (future movement tick will consume them).
- [ ] **Invalid key names do not crash** — set `MoveForwardKey` to `"NotARealKey"` and nonsense activation string; mission loads; bindings fall back to defaults for bad entries.
- [ ] **Invalid height values are clamped** — set `MinHeight` below 2, `MaxHeight` below `MinHeight`, `MoveSpeed` negative; confirm no throw and sensible clamped tuning (min height ≥ 2, max > min, positive move/rotation, fast multiplier ≥ 1).

## Notes

- `OverrideNativeBackspaceOrders` is stored for future wiring only; this slice does not change native Backspace consumption beyond reading the flag into config.
- Re-entering a mission reloads JSON from disk (no mid-battle hot reload requirement).
