# Manual install (no scripts)

**Mod:** RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)

## Requirements

- Mount & Blade II Bannerlord (Steam or other install with a `Modules` folder).
- Official modules: **Native**, **SandBoxCore**, **Sandbox**, **StoryMode**, **CustomBattle** (see root `SubModule.xml`).
- **Bannerlord.Harmony** (BUTR; e.g. Steam Workshop) — **required** for launcher dependency resolution and runtime **`0Harmony.dll`**. Install/load it **before** RTS Commander Doctrine in mod order (see `docs/research/local-bannerlord-load-order.md`).

## Correct layout

Do **not** copy only `Bannerlord.RTSCameraLite.dll`. The game loads dependencies from the **same folder** as the main DLL. With `CopyLocalLockFileAssemblies` / NuGet packages, **`System.Text.Json` (and any other copied dependency DLLs)** must sit next to the mod DLL.

Target structure:

```text
<Mount & Blade II Bannerlord>/
  Modules/
    Bannerlord.RTSCameraLite/
      SubModule.xml
      config/
        commander_config.json
      bin/
        Win64_Shipping_Client/
          Bannerlord.RTSCameraLite.dll
          (all other DLLs from the repo build output folder — e.g. System.Text.Json*.dll)
```

## Steps

1. Build the mod from the repo: `dotnet build -c Release` (see `README.md`).
2. Copy **the entire** `bin\Win64_Shipping_Client\` output into `Modules\Bannerlord.RTSCameraLite\bin\Win64_Shipping_Client\`.
3. Copy **`SubModule.xml`** to `Modules\Bannerlord.RTSCameraLite\`.
4. Copy **`config\commander_config.json`** to `Modules\Bannerlord.RTSCameraLite\config\`.
5. Enable the mod in the Bannerlord launcher with dependencies in a valid order.

## Runtime safety (mission hooks — Crash Quarantine)

- **Safe defaults:** `"EnableMissionRuntimeHooks": false`, **`"EnableHarmonyPatches": false`**, **`"EnableHarmonyDiagnostics": false`** in `config/commander_config.json`. Harmony **patches** do not run unless both **`EnableHarmonyPatches`** and **`EnableMissionRuntimeHooks`** are **true** (scaffold only; no engine patches in repo yet). Mission attachment: the mod **does not** add `CommanderMissionView` unless **`EnableMissionRuntimeHooks`** is **explicitly** `true` and the config file is readable (fail-closed if missing, corrupt, or unreadable).
- **Mission runtime is experimental and opt-in.** Doctrine, diagnostics, command router, markers, and native-order execution toggles also default **off** in shipped config.
- **Deployment success ≠ runtime stability.** A valid folder layout (see D1) does not prove battles are safe — **battle stability requires separate in-game verification** (manual checklists). If you crash after the main menu, keep hooks off and review `docs/configuration.md` before enabling features.

## Easier path

Use **`scripts/package-module.ps1`** and **`scripts/audit-steam-deployment.ps1`** so you do not miss dependency DLLs. See **`docs/deploy.md`**.
