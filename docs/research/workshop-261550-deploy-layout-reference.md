# Steam Workshop `261550` deploy layout reference

**App ID:** `261550` (Mount & Blade II: Bannerlord)  
**Content root:** `steamapps\workshop\content\261550\<workshopItemId>\`

This note summarizes patterns observed from **installed Workshop modules** (same layout as manual `Modules\<Id>\` copies) used to validate that **Bannerlord.RTSCameraLite** packages match Bannerlord expectations.

## Common layout (managed C# mods)

| Element | Convention |
|--------|--------------|
| **Root** | Numeric folder name = Workshop item id (e.g. `2859188632`); for **manual** installs, folder name usually equals **Module Id** (e.g. `Bannerlord.RTSCameraLite`). |
| **SubModule.xml** | Single file at **item root** (not nested under `bin`). |
| **`<Id value="..."/>`** | Stable module id; launcher deduplication and load order use this string. |
| **`<DLLName>`** | Primary managed assembly name, resolved under `bin\Win64_Shipping_Client\`. |
| **bin\Win64_Shipping_Client** | All managed dependencies the mod ships (e.g. `System.Text.Json.dll`). **Harmony mods** typically do **not** ship `0Harmony.dll` when they depend on **Bannerlord.Harmony** (Workshop `2859188632`), which supplies runtime Harmony. |

## Reference items (from local manifest scan)

| Workshop id | Module Id | Notes |
|-------------|-----------|--------|
| `2859188632` | `Bannerlord.Harmony` | BUTR Harmony; ships `Bannerlord.Harmony.dll` + `0Harmony.dll` + tooling assemblies. |
| `2859222409` | `Bannerlord.UIExtenderEx` | Depends on `Bannerlord.Harmony`. |
| `3596692403` | `RTSCamera` | RTS Camera; `DependedModuleMetadata` for Harmony load order. |

## Our mod (`Bannerlord.RTSCameraLite`)

- **Package output:** `artifacts/Bannerlord.RTSCameraLite/` mirrors the Workshop pattern: `SubModule.xml`, `config\`, `bin\Win64_Shipping_Client\*.dll`.
- **Audit:** use `scripts/audit-steam-deployment.ps1` with **`-ModuleRoot`** pointing at that folder to verify deployability **before** copying into `GameRoot\Modules\`.

```powershell
powershell -ExecutionPolicy Bypass -File scripts/package-module.ps1 -Configuration Release -Clean
powershell -ExecutionPolicy Bypass -File scripts/audit-steam-deployment.ps1 `
  -GameRoot "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord" `
  -ModuleRoot "C:\...\Bannerlord.RTSCameraLite\artifacts\Bannerlord.RTSCameraLite"
```
