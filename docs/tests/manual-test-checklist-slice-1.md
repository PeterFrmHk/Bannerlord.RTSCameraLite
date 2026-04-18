# Manual test checklist — Slice 1 (module foundation + load safety)

**Mod:** RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)  
**Version:** `0.1.0-slice1`  
**Goal:** Verify build, deployment layout, launcher, main menu, and battle load with **default mission runtime hooks disabled** (`EnableMissionRuntimeHooks`: **false** — Crash Quarantine / CQ1). Optional: opt-in experimental mission runtime.

**Note:** Passing deployment audit (D1) does not replace **in-game** battle verification; treat battle stability as a separate sign-off.

---

## 1. Build test

- [ ] From repo root `Bannerlord.RTSCameraLite\`, run: `dotnet build -c Release` (or `Debug`).
- [ ] Build completes with **0 errors**.
- [ ] Output contains `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`, dependency assemblies present as emitted by MSBuild (e.g. `System.Text.Json*.dll` when copied), and **`SubModule.xml`** next to the DLL output for deployment packaging.

---

## 2. Folder layout test

- [ ] **Recommended:** From repo root run `powershell -ExecutionPolicy Bypass -File scripts/package-module.ps1 -Configuration Release -Clean`, then verify **`artifacts/Bannerlord.RTSCameraLite/`** matches `SubModule.xml` + `config/` + `bin/Win64_Shipping_Client/` (full DLL set).
- [ ] **Or manual:** Copy the **entire** module layout from build output: **`SubModule.xml` at module root**, **`bin\Win64_Shipping_Client\`** with **all** DLLs (main mod DLL **and** dependency DLLs such as `System.Text.Json`), and **`config\commander_config.json`** (default keeps `EnableMissionRuntimeHooks` **false**).
- [ ] Target path matches: `Mount & Blade II Bannerlord\Modules\Bannerlord.RTSCameraLite\`.
- [ ] **Optional verification:** `powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1` (read-only PASS/WARN/FAIL). See **`docs/deploy.md`**.

---

## 3. Launcher visibility test

- [ ] Open the Bannerlord **Mods** launcher list.
- [ ] **RTS Commander Doctrine** appears (from `<Name value="RTS Commander Doctrine"/>`).

---

## 4. Main menu load test

- [ ] Enable **Bannerlord.Harmony** and **RTS Commander Doctrine** with valid dependency order (`SubModule.xml`: Native, SandBoxCore, Sandbox, StoryMode, CustomBattle, **Bannerlord.Harmony** before this mod per `docs/research/local-bannerlord-load-order.md`).
- [ ] Start the game (singleplayer); reach **main menu** without crash.
- [ ] **Harmony scaffold:** with defaults (`EnableHarmonyPatches` **false**), expect **no** Harmony patch activity from this mod. Launcher “dependency satisfied” for Harmony does **not** mean patches run.

---

## 5. Battle load test (runtime hooks **disabled** — default)

- [ ] Confirm `config/commander_config.json` has `"EnableMissionRuntimeHooks": false` (shipped default).
- [ ] From main menu, start a **Custom Battle** (or any supported battle flow you use for regression).
- [ ] Mission **loads** without crash.
- [ ] **Expectation:** no experimental commander mission shell from this mod (behavior not attached). No RTS camera / doctrine / extra mission logic from this mod **unless** you enable hooks and flags per other slices.

---

## 6. Optional — experimental mission runtime hook test

- [ ] Set `"EnableMissionRuntimeHooks": true` in `config/commander_config.json` (and any other flags your scenario needs per `docs/configuration.md`).
- [ ] Relaunch and enter a supported battle.
- [ ] Confirm mission loads; treat commander / doctrine / camera behavior as **experimental** until signed off on slice-specific checklists.

---

## 7. Disable mod test

- [ ] Disable **RTS Commander Doctrine** in the launcher and confirm the base game still reaches main menu.

---

## Sign-off

| Field | Value |
| --- | --- |
| Tester | |
| Bannerlord / Native version | |
| Date | |
| Result (Pass / Fail) | |
