# Slice 5 audit (RTS camera bridge + override hook)

## User tests checklist (owner)

### Build / module hygiene

- [ ] `dotnet build` succeeds for `Bannerlord.RTSCameraLite.csproj`
- [ ] `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll` updates after changes
- [ ] `SubModule.xml` still matches the module layout (`DLLName` + `bin\Win64_Shipping_Client`)

### In-game: toggle + seeding

- [ ] Custom battle loads with the mod enabled
- [ ] Press **F10** once: RTS mode enables (log / UI message)
- [ ] Wait until `MainAgent` exists: pose seeds from the player agent (height / anchor)
- [ ] Press **F10** again: RTS mode disables and vanilla camera restore is attempted (no hard crash)

### In-game: movement + camera ownership

- [ ] While RTS enabled, **WASD** translates the RTS pose on the XY plane (world-locked forward from yaw)
- [ ] **Q/E** yaw the RTS view
- [ ] Mouse wheel / **R/F** zoom adjust height within min/max bounds
- [ ] With RTS enabled and a seeded pose, `UpdateOverridenCamera` returns **true** and the bridge applies a `MatrixFrame` to the resolved camera when methods/properties exist on your build

### In-game: stability

- [ ] Spam **F10** on/off: no crash; restore path runs on disable
- [ ] Leave battle / mission end: `OnRemoveBehavior` runs restore without crashing

### Engineering constraints (Slice 5)

- [ ] **No Harmony** dependency added for this slice
- [ ] If the bridge cannot apply, an informational message is logged **at most once** per enable session (see `UpdateOverridenCamera`)

## PRISM packet (camera integration)

- **A**: `MissionView.UpdateOverridenCamera` is the supported, versioned extension point for custom per-frame camera control in Bannerlord missions.
- **Not-A**: Private camera internals stay private; relying on them without reflection/Harmony is brittle and not the integration strategy for this slice.
- **A\***: Prefer **public reflection** against `MissionScreen` / camera surfaces plus optional owner ILSpy verification per game version; keep Harmony out unless a future slice explicitly accepts that operational risk.

## Delivery order (Slices 2-5)

1. **Slice 2**: RTS toggle state + mission wiring (no camera movement).
2. **Slice 3**: Mission mode gating + safe attachment rules (still no camera override).
3. **Slice 4**: Input snapshot + controller pose evolution (simulation-only; still not driving native camera).
4. **Slice 5**: `UpdateOverridenCamera` + `CameraBridge` apply/restore (first native camera movement slice; reflection-first; no Harmony).
