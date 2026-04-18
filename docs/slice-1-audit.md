# Slice 1 Audit: Foundation + Loadable Module

**Companion:** [`slices/slice-1-foundation.md`](slices/slice-1-foundation.md) · **Manual tests:** [`tests/manual-test-checklist-slice-1.md`](tests/manual-test-checklist-slice-1.md)

## Purpose

Establish a single-player Bannerlord module that builds, can be deployed under `Modules`, loads in the launcher, and survives a custom battle without adding RTS camera, Harmony, or meaningful mission logic.

## Function spec (Slice 1)

- `MBSubModuleBase` entry: `OnSubModuleLoad`, `OnBeforeInitialModuleScreenSetAsRoot`, and **empty** `OnMissionBehaviorInitialize` (American spelling; base call only — Slice 2 registers behaviors).
- Defensive logging: no crash if UI messaging is unavailable; on-screen messages only after UI root is marked ready.
- Module metadata and dependencies declared in `SubModule.xml` for Native, SandBoxCore, Sandbox, StoryMode, and CustomBattle.

## Implementation

- Minimal `SubModule`, `ModConstants`, and `ModLogger` under `Bannerlord.RTSCameraLite`.
- Compile references: **either** `BannerlordGameFolder` / `BANNERLORD_INSTALL` game DLLs **or** NuGet `Bannerlord.ReferenceAssemblies` (see `Bannerlord.RTSCameraLite.csproj`, `local.props.example`).

## Tests

- Automated: `dotnet build` (NuGet refs) or build against local `bin\Win64_Shipping_Client` when `BannerlordGameFolder` is set.
- Manual: **`docs/tests/manual-test-checklist-slice-1.md`** (and `docs/manual-test-checklist.md` for repo-wide notes) and Slice 1 done criteria below.

## Slice 1 done criteria (authoritative)

Mark Slice 1 done only when **all** are true:

- [ ] `dotnet build` passes
- [ ] DLL exists
- [ ] `SubModule.xml` exists at module root
- [ ] Mod appears in launcher
- [ ] Mod enables
- [ ] Game reaches main menu
- [ ] Custom battle starts
- [ ] No camera behavior exists yet
- [ ] No Harmony patches exist yet
- [ ] `docs/version-lock.md` is filled in (owner: install path + game version after local test)

## Audit

### Implemented

- Minimal Bannerlord module metadata (`SubModule.xml`)
- `MBSubModuleBase` entry point (`src/SubModule.cs`, including empty `OnMissionBehaviorInitialize`)
- Safe startup logging (`src/Core/ModLogger.cs` — `InformationManager` only when UI is marked ready; `System.Diagnostics.Debug` for always-on diagnostic text without conflicting with `TaleWorlds.Library.Debug`)
- Version constants (`src/Core/ModConstants.cs`)
- Version lock document (`docs/version-lock.md`)
- Manual test checklist (`docs/manual-test-checklist.md`)
- This slice audit document (`docs/slice-1-audit.md`)

### Not implemented

- RTS camera toggle
- Input routing
- Mission camera control
- Harmony patching
- MCM / ButterLib / UIExtenderEx / UI changes
- Formation selection
- Command routing
- Tactical overlay

### Files changed (Slice 1 scaffold)

- `Bannerlord.RTSCameraLite.csproj`
- `SubModule.xml`
- `src/SubModule.cs`
- `src/Core/ModConstants.cs`
- `src/Core/ModLogger.cs`
- `local.props.example`
- `docs/version-lock.md`
- `docs/manual-test-checklist.md`
- `docs/slice-1-audit.md`

### Known risks

- Wrong Bannerlord version may prevent loading or change APIs.
- Wrong DLL output path may prevent launcher detection relative to `SubModule.xml`.
- `SubModuleClassType` or dependency mismatch may prevent module load.
- UI logging too early can fail; logger remains defensive.
- If compile ref version ≠ installed game version, runtime mismatches are possible; prefer local game DLLs for parity.

### Next slice readiness checklist

Slice 2 may begin when every item in **Slice 1 done criteria** is checked (including owner in-game verification).

Slice 2 extends `OnMissionBehaviorInitialize` to register `MissionBehavior` types (camera shell, no Harmony in scope unless you add it later).

## PRISM check

- **A:** Start RTS camera immediately.
- **¬A:** RTS camera depends on a stable module load path and local API verification.
- **A\*:** Slice 1 establishes a clean module foundation; Slice 2 adds RTS camera toggle and mission behavior shell.

## Slice status

Slice 1 is complete when the module exists, loads, and survives custom battle without camera, Harmony, or mission **logic** (empty `OnMissionBehaviorInitialize` is allowed).

## Build and verification report (agent host)

- **Build command (default configuration):** `dotnet build` from `Bannerlord.RTSCameraLite\` (SDK **8.0.420**).
- **Build command (Release):** `dotnet build -c Release`
- **Build result:** succeeded (0 warnings, 0 errors) after SDK install on the verification host.
- **Produced DLL path:** `C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`
- **Launcher visibility:** not verified here (module folder was not copied into a live `Mount & Blade II Bannerlord\Modules\` tree on this host).
- **Custom battle load:** not verified here (game not executed in this environment).

To test locally, copy or junction the entire `Bannerlord.RTSCameraLite` project folder to `<Bannerlord>\Modules\Bannerlord.RTSCameraLite\` so `SubModule.xml` sits beside the `bin` folder, then follow `docs/manual-test-checklist.md`.
