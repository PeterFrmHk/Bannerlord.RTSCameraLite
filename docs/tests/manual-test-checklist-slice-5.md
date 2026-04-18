# Manual test checklist — Slice 5 (real camera bridge)

**Mod:** RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)  
**Pre:** Build `Release`, deploy `SubModule.xml` + `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`.

---

## Build & launch

- [ ] `dotnet build -c Release` succeeds.
- [ ] Game launches (singleplayer) with mod enabled.

## Custom battle

- [ ] Custom battle loads with mod enabled.
- [ ] Commander mode **starts enabled** (shell default).

## Camera apply (if wired on your build)

- [ ] With commander mode on, RTS camera pose is **visibly** different from default third-person (height / angle).
- [ ] **W / S / A / D** move the camera over the field.
- [ ] **Q / E** yaw the camera.
- [ ] **R / F** (and mouse wheel) change height / zoom feel.

## Commander toggle & restore

- [ ] **Backspace** turns commander mode **off** — native player camera returns (no “stuck” RTS view).
- [ ] **Backspace** again turns commander mode **on** — RTS override resumes when `UpdateOverridenCamera` applies.
- [ ] Repeated toggles do **not** trap the camera.

## Stress

- [ ] Player death does not hard-crash (best-effort restore).
- [ ] Battle end / exit to map does not crash; no spam of red errors from bridge (at most **one** debug line per distinct failure).

## Notes

Record **Native module version** (e.g. v1.3.15) and any anomaly (deployment camera, siege, photo mode).

| Tester | |
| --- | --- |
| Date | |
| Native / game version | |
| Result | |
