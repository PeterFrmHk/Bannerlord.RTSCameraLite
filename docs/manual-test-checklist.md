# Manual Test Checklist: Slice 1

**Research hard gates:** Do not start **Slice 5** (real camera apply + restore) or **Slice 12** (native order executor) until the prerequisites in [`slice-hard-gates.md`](slice-hard-gates.md) are satisfied.

## Slice 1 done criteria (summary)

Mark Slice 1 done only when **all** are true:

- [ ] `dotnet build` passes
- [ ] DLL exists
- [ ] `SubModule.xml` exists at module root (next to `bin`)
- [ ] Mod appears in launcher
- [ ] Mod enables
- [ ] Game reaches main menu
- [ ] Custom battle starts
- [ ] No camera behavior exists yet
- [ ] No Harmony patches exist yet
- [ ] `docs/version-lock.md` is filled in (owner paths + game version after local test)

## Verification status

| Phase | Status | Notes |
| --- | --- | --- |
| Build (repo / CI) | **PASS** (2026-04-18) | `dotnet build` succeeded on agent host. |
| DLL + SubModule.xml in repo | **PASS** | XML at project root; DLL under `bin\Win64_Shipping_Client\`. |
| Launcher + enable + main menu + custom battle | **PENDING — owner** | Requires copying module into live `Modules` and running the game. |
| No camera movement / no Harmony | **PASS** (code review Slice 2) | `MissionView` reads F10 only; no `UpdateOverridenCamera` / no Harmony. Owner: confirm in-game. |

## Build

- [x] Run clean build (`dotnet build` or `dotnet build -c Release`)
- [x] Confirm DLL is produced (`bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`)
- [x] Confirm `SubModule.xml` exists in module root
- [x] Confirm DLL path matches `SubModule.xml` (`DLLName` + `bin\Win64_Shipping_Client`)

## Launcher

- [ ] Open Bannerlord launcher
- [ ] Confirm RTS Camera Lite appears
- [ ] Enable RTS Camera Lite
- [ ] Start game

## Main menu

- [ ] Main menu loads
- [ ] No crash
- [ ] Startup message appears or debug log confirms load

## Custom battle

- [ ] Open Custom Battle
- [ ] Start a basic battle
- [ ] Battle loads
- [ ] No mission crash
- [ ] Exit battle cleanly

## Disable test

- [ ] Disable mod
- [ ] Start game again
- [ ] Game still loads cleanly

---

## Slice 2: RTS toggle state (no camera movement)

### Done criteria (owner)

- [ ] `dotnet build` passes
- [ ] Game loads; custom battle loads
- [ ] **F10** toggles internal RTS state (on-screen / debug log)
- [ ] **F10** again shows disabled log
- [ ] No camera movement from this mod
- [ ] No crash when spamming **F10**
- [ ] No Harmony dependency

### Steps

- [ ] Launch game with mod enabled
- [ ] Start **Custom Battle** and enter battle (`Deployment` / `Battle` so the gate allows the behavior)
- [ ] Press **F10** once — confirm **ENABLED** log message
- [ ] Press **F10** again — confirm **DISABLED** log message
- [ ] Spam **F10** rapidly — confirm no crash; counts increase logically
- [ ] Exit battle cleanly
- [ ] Start another custom battle — repeat F10 on / off

### Notes

- If **F10** does nothing, the mission may not be in `Deployment` or `Battle` yet; wait until deployment or combat phase, or revisit `MissionModeGate` for your build.

---

## Slice 7: File-based config (`config/rts_camera_lite.json`)

### Audit logic (summary)

- **A:** Hardcoded controls are fine for a prototype.
- **¬A:** Camera feel depends heavily on local preference and conflicts with player bindings.
- **A\*:** File-based config before UI keeps tuning testable without MCM or extra dependencies.

### Example config (reference)

Path: `{module root}/config/rts_camera_lite.json` (repo copy: `config/rts_camera_lite.json`).

```json
{
  "ToggleKey": "F10",
  "MoveForwardKey": "W",
  "MoveBackKey": "S",
  "MoveLeftKey": "A",
  "MoveRightKey": "D",
  "RotateLeftKey": "Q",
  "RotateRightKey": "E",
  "FastMoveKey": "LeftShift",
  "ZoomInKey": "R",
  "ZoomOutKey": "F",
  "NextFormationKey": "PageDown",
  "PreviousFormationKey": "PageUp",
  "FocusSelectedFormationKey": "Home",
  "MoveSpeed": 12.0,
  "FastMoveMultiplier": 2.5,
  "RotationSpeedDegrees": 90.0,
  "ZoomSpeed": 3.0,
  "DefaultHeight": 18.0,
  "MinHeight": 6.0,
  "MaxHeight": 60.0,
  "DefaultPitch": 60.0
}
```

Key names must match TaleWorlds `InputKey` enum names (case-insensitive). See `docs/slice-7-audit.md`.

### Tests

- [ ] Build passes (`dotnet build` / `dotnet build -c Release`)
- [ ] Missing config creates default config (delete `config/rts_camera_lite.json`, enter battle, file reappears under module `config/`)
- [ ] Invalid config falls back safely (malformed JSON or nonsense floats; game loads, defaults / merged values used; check log diagnostics)
- [ ] Toggle key works from config (e.g. set `ToggleKey` to `Home`, restart mission, toggle RTS with Home)
- [ ] Movement speed changes when config changes (e.g. `MoveSpeed` 12 vs 40 while RTS camera on)
- [ ] Height clamp works (set `DefaultHeight` above `MaxHeight` or below `MinHeight`; effective height stays within bounds after sanitize / controller clamp)
- [ ] No crash from bad key names (set `ToggleKey` to a bogus name such as `NotARealKey`; should fall back to default toggle and keep running)

### Quick steps

- [ ] With mod enabled, start a supported custom battle (deployment/battle per `MissionModeGate`).
- [ ] Confirm one startup log line mentions loaded config path and effective `move` / `toggle` (first session only if using static log-once).
- [ ] Edit JSON, save, start a **new** battle (config loads at behavior init) and re-check behavior above.

---

## Slice 8: Formation cycling and focus (RTS only)

### Suggested default keys (config)

| Action | Default `InputKey` name | Notes |
|--------|-------------------------|--------|
| Next formation | `PageDown` | |
| Previous formation | `PageUp` | |
| Focus selected formation | `Home` | Avoid **`Space`** as default — high conflict risk with native controls. |

JSON: `NextFormationKey`, `PreviousFormationKey`, `FocusSelectedFormationKey` in `config/rts_camera_lite.json`. See `docs/slice-8-audit.md`.

### Audit logic (summary)

- **A:** Free camera is enough.
- **¬A:** RTS camera without formation awareness is just a flying spectator toy.
- **A\*:** Add formation query and camera focus before command routing.

### Tests

- [ ] Build passes
- [ ] Battle loads with several formations
- [ ] RTS mode enabled
- [ ] Next formation cycles (`PageDown` or configured `NextFormationKey`)
- [ ] Previous formation cycles (`PageUp` or configured `PreviousFormationKey`)
- [ ] Focus selected formation moves camera (`Home` or configured `FocusSelectedFormationKey`)
- [ ] Empty/dead formation skipped (selection clears / cycle skips without crash)
- [ ] Battle end does not crash
- [ ] No orders are issued (camera + selection only; no `IssueOrder` / command routing)

---

## Slice 9: Lightweight tactical feedback

### Recommended execution order (roadmap)

1. **Slice 6:** Input ownership guard  
2. **Slice 7:** Config file + control profile  
3. **Slice 8:** Formation query + camera focus  
4. **Slice 9:** Lightweight tactical feedback  
5. **Slice 10:** Command intent + validation (see below)  

### Audit logic (summary)

- **A:** No UI until later.
- **¬A:** Player needs basic feedback to test tactical interactions.
- **A\*:** Add lightweight throttled feedback without committing to full overlay architecture.

### Tests

- [ ] Build passes
- [ ] Enable RTS mode, one message appears
- [ ] Disable RTS mode, one message appears
- [ ] Cycle formation, selected message appears
- [ ] Failed focus produces warning
- [ ] Camera bridge warning does not spam every tick
- [ ] No crash if UI messaging unavailable (`ModLogger.PlayerMessage` / `InformationManager` paths are try/catch; debug text still goes to `System.Diagnostics.Debug` when UI is not ready)

See `docs/slice-9-audit.md` for throttle keys and component notes.

---

## Slice 10: Command intent + validation (no native orders)

### Recommended execution order (roadmap)

1. **Slice 6:** Input ownership guard  
2. **Slice 7:** Config file + control profile  
3. **Slice 8:** Formation query + camera focus  
4. **Slice 9:** Lightweight tactical feedback  
5. **Slice 10:** `CommandIntent` + `CommandRouter.Validate` (debug **H** / **C** / **G**) — **no** `IssueOrder`  

### Audit logic (summary)

- **A:** Start issuing orders immediately.
- **¬A:** Bannerlord command execution is fragile and needs validated intent first.
- **A\*:** Build `CommandIntent` and a validation layer before touching native order APIs.

### Tests

- [ ] Build passes
- [ ] Enter custom battle
- [ ] Enable RTS mode
- [ ] Select/cycle formation
- [ ] Press **H**, validation result appears (`[Cmd] OK:` or `[Cmd] Rejected:`)
- [ ] Press **C**, validation result appears
- [ ] Try command without selected formation, safe failure (rejected message, no crash)
- [ ] Disable RTS mode, command validation rejected (same debug keys while RTS off)
- [ ] No actual Bannerlord order is issued (troops do not change behavior; code path is validate + feedback only)

See `docs/slice-10-audit.md`. (After **Slice 12** is merged, real orders apply — use the **Slice 12** tests below for troop behavior.)

---

## Slice 11: Ground target resolution (validate only)

### Recommended execution order (roadmap)

1. **Slice 10:** `CommandIntent` + `CommandRouter.Validate` (debug **H** / **C** / **G**) — **no** `IssueOrder`  
2. **Slice 11:** `GroundTargetResolver` + terrain projection; **G** uses `ResolvedGroundPosition` only  

### Audit logic (summary)

- **A:** Command routing can use camera position directly.  
- **¬A:** Tactical commands need ground positions, not floating camera coordinates.  
- **A\*:** Add a `GroundTargetResolver` before command execution.  

### Tests

- [ ] Build passes (`dotnet build` / `dotnet build -c Release`)  
- [ ] RTS mode enabled (supported mission phase per `MissionModeGate`)  
- [ ] Ground target resolver runs safely (no crash if terrain API missing or reflection fails; battle stays stable)  
- [ ] **G** creates `MoveToPosition` validation intent (player message shows `[Cmd] OK:` or `[Cmd] Rejected:`; OK path uses resolved ground only)  
- [ ] Invalid target fails safely (e.g. no successful ground sample yet, or rejected formation — **G** → rejected, no crash)  
- [ ] Camera movement updates target position (after moving RTS camera, optional `[Ground] Target OK` on recover or compare coordinates in log if enabled)  
- [ ] Ground resolver path does not bypass validation (no direct `IssueOrder`; native issuance is Slice 12 `NativeOrderExecutor`).  
- [ ] No message spam (`[Ground]` lines throttled; success not repeated every tick — transition + cooldown)  

See `docs/slice-11-audit.md` for sampling cadence, terrain reflection notes, and type list.

---

## Slice 12: Native order execution (minimal set)

### Recommended execution order (roadmap)

1. **Slice 10–11:** `CommandIntent`, validation, `GroundTargetResolver` / **G** position  
2. **Research hard gate:** [`slice-hard-gates.md`](slice-hard-gates.md) + [`research/native-order-hooks.md`](research/native-order-hooks.md)  
3. **Slice 12:** `NativeOrderExecutor` after `CommandRouter.ExecuteValidated`  

### Audit logic (summary)

- **A:** Implement full command system now.  
- **¬A:** Native order execution is the highest-risk integration after camera bridging.  
- **A\*:** Execute only three minimal commands through a contained `NativeOrderExecutor`.  

### Tests

- [ ] Build passes (`dotnet build` / `dotnet build -c Release`)  
- [ ] Enter custom battle  
- [ ] Enable RTS mode  
- [ ] Select friendly formation  
- [ ] Press **C**, selected formation charges  
- [ ] Press **H**, selected formation holds  
- [ ] Move camera / ground target, press **G**, formation moves to resolved point  
- [ ] Disable RTS mode, native controls restored  
- [ ] Battle end while RTS mode active does not crash  
- [ ] Invalid command does not crash (bad formation / RTS off / no ground for **G** → `[Cmd] Not executed`, throttled; no exception)  

See `docs/slice-12-audit.md` and `docs/research/native-order-hooks.md`.

---

## Slice 13: Minimal command markers (no overlay)

### Recommended execution order (roadmap)

1. **Slice 12:** `NativeOrderExecutor` + `CommandExecutionResult.MarkerWorldPosition` for move  
2. **Slice 13:** `CommandMarkerService` + `CommandMarkerState` + throttled `[Marker]` feedback  

### Audit logic (summary)

- **A:** A command can execute silently.  
- **¬A:** Silent tactical commands feel broken and are hard to debug.  
- **A\*:** Add minimal command marker feedback without building the full overlay system.  

### Tests

- [ ] Build passes (`dotnet build` / `dotnet build -c Release`)  
- [ ] `MoveToPosition` command works (RTS on, friendly formation, resolved ground, **G** — formation moves)  
- [ ] Marker or fallback appears on successful move (`[Marker] MoveToPosition … @ (x,y,z)` and/or optional particle burst if `OptionalMoveMarkerParticleName` is set for your build)  
- [ ] Marker expires after lifetime (~**2.5s**; internal state cleared; no stuck feedback loop)  
- [ ] Repeated move commands do not crash (rapid **G** with valid targets; throttle limits `[Marker]` spam)  
- [ ] Charge / Hold feedback still works (`[Cmd] Executed …` plus optional throttled `[Marker] Charge|HoldPosition issued (no map pin…)` line)  
- [ ] Marker failure does not block command execution (invalid particle name / internal marker exception: troops still move; only log / safe swallow)  

See `docs/slice-13-audit.md`.
