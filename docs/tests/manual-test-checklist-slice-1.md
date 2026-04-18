# Manual test checklist — Slice 1 (module foundation)

**Mod:** RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)  
**Version:** `0.1.0-slice1`  
**Goal:** Verify load, enable, main menu, and custom battle with **no mission behaviors** added by this slice.

---

## 1. Build test

- [x] From repo root `Bannerlord.RTSCameraLite\`, run: `dotnet build -c Release` (or `Debug`).
- [x] Build completes with **0 errors**.
- [x] Output contains `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll` and **`SubModule.xml`** (copied next to the DLL for deployment).

---

## 2. Launcher visibility test

- [ ] Copy the **module folder** (at minimum: `SubModule.xml` + `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`) into  
  `Mount & Blade II Bannerlord\Modules\Bannerlord.RTSCameraLite\`  
  matching the layout your launcher expects (XML at module root, DLL under `bin\Win64_Shipping_Client\`).
- [ ] Open the Bannerlord **Mods** launcher list.
- [ ] **RTS Commander Doctrine** appears (from `<Name value="RTS Commander Doctrine"/>`).

---

## 3. Enable mod test

- [ ] Enable **RTS Commander Doctrine**.
- [ ] Ensure dependency mods load order is valid (Native, SandBoxCore, Sandbox, StoryMode, CustomBattle per `SubModule.xml`).
- [ ] Launcher reports no missing-dependency errors for this mod.

---

## 4. Main menu load test

- [ ] Start the game (singleplayer) with only **official** dependency modules + this mod enabled (or your standard safe set).
- [ ] Game reaches **main menu** without crash.
- [ ] Optional: after load, confirm a single informational line from the mod is acceptable (post–UI-ready message); absence is also OK if messages are disabled in your build.

---

## 5. Custom battle load test

- [ ] From main menu, open **Custom Battle** (requires Custom Battle / dependent modules as usual).
- [ ] Start a battle and confirm the mission **loads** without crash.
- [ ] **Slice 1 expectation:** no RTS camera, no formation doctrine, no extra mission logic from this mod (empty `OnMissionBehaviorInitialize` aside from `base`).

---

## 6. Disable mod test

- [ ] Exit to desktop.
- [ ] Disable **RTS Commander Doctrine** in the launcher.
- [ ] Start the game again; confirm main menu still loads (sanity check that the mod is not required for base game).

---

## Sign-off

| Field | Value |
| --- | --- |
| Tester | |
| Bannerlord / Native version | |
| Date | |
| Result (Pass / Fail) | |
