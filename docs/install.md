# Manual install (no scripts)

**Mod:** RTS Commander Doctrine (`Bannerlord.RTSCameraLite`)

## Requirements

- Mount & Blade II Bannerlord (Steam or other install with a `Modules` folder).
- Official modules this mod depends on: **Native**, **SandBoxCore**, **Sandbox**, **StoryMode**, **CustomBattle** (see root `SubModule.xml`).

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

## Runtime safety (mission hooks)

- **Default load-safe install:** keep `"EnableMissionRuntimeHooks": false` in `config/commander_config.json` (shipped default). Mission runtime is **experimental** and opt-in.
- **Deployment correctness does not guarantee runtime stability.** If the game crashes in battle after the main menu loads, review **Crash Quarantine** / config gates in repo docs and disable hooks before deeper debugging.

## Easier path

Use **`scripts/package-module.ps1`** and **`scripts/audit-steam-deployment.ps1`** so you do not miss dependency DLLs. See **`docs/deploy.md`**.
