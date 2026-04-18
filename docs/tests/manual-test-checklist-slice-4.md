# Manual test checklist — Slice 4 (internal commander camera pose)

Use a **pinned** Bannerlord build matching project reference assemblies (see `Bannerlord.RTSCameraLite.csproj`).

## Build test

- [ ] From repo root: `dotnet build .\Bannerlord.RTSCameraLite\Bannerlord.RTSCameraLite.csproj -c Release` completes with **0 errors**.

## Battle start test

- [ ] Deploy built `Bannerlord.RTSCameraLite.dll` + `SubModule.xml` to `Modules\Bannerlord.RTSCameraLite\` (or your symlinked layout).
- [ ] Launch game, start a **custom battle** (or other mission covered by `CommanderMissionModeGate`).
- [ ] Mission loads without CTD; module log shows commander shell / config load as in prior slices.

## Commander Mode toggle test

- [ ] If `commander_config.json` has `StartBattlesInCommanderMode: true`, commander mode is **on** at battle start.
- [ ] **Backspace** toggles commander mode off/on (and F10 fallback if enabled in config).
- [ ] When **disabled**, releasing keys does not need to do anything special — internal camera **Tick** must not run.

## Internal movement test (Commander Mode **on**)

- [ ] Hold **W** / **S**: internal pose moves forward/back along yaw.
- [ ] Hold **A** / **D**: strafe left/right.
- [ ] Hold **Q** / **E**: yaw rotates left/right.
- [ ] Hold **Left Shift** or **Right Shift** with movement: faster pan.
- [ ] Hold **R** (zoom in) / **F** (zoom out): logical **Height** changes and stays within min/max (defaults 6–60).
- [ ] **Pitch** does not change while moving (still fixed after spawn init).

## Bridge not-wired safety test

- [ ] With `CameraBridge` returning **NotWired**: **no crash** on repeated ticks.
- [ ] **At most one** `WARN` line per mission about the bridge not applying (no per-frame spam).
- [ ] `CameraBridge.TryApply` is still invoked while commander mode is active (pose hand-off path is alive).

## Battle exit cleanup test

- [ ] End battle or leave mission: no exception from cleanup / restore path.
- [ ] Start a **second** battle: one-shot logs (first internal pose, bridge warning) may appear again — acceptable “per mission” behavior.
