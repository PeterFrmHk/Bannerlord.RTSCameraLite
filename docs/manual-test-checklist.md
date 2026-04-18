# Manual Test Checklist: Slice 1

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
