# Version Lock

## Module

- Module ID: Bannerlord.RTSCameraLite
- Display name: RTS Camera Lite
- Slice: 1
- Mod version: 0.1.0-slice1

## Local environment

| Field | Value |
| --- | --- |
| Bannerlord install path | **Owner:** paste full path to game root (folder containing `bin\Win64_Shipping_Client`). Required for runtime tests and recommended for reference DLL parity. |
| Bannerlord game version | **Owner:** from game launcher or in-game version UI. Must match the build you test. |
| Official Modding Tools version | **Owner:** N/A if unused; otherwise paste Modding Kit version. |
| .NET SDK version | 8.0.420 (verified `dotnet --version` on 2026-04-18) |
| Reference assemblies (compile) | NuGet `Bannerlord.ReferenceAssemblies` **1.2.12.66233** when `BannerlordGameFolder` / `BANNERLORD_INSTALL` is unset; else TaleWorlds DLLs from `$(BannerlordGameFolder)\bin\Win64_Shipping_Client\`. |
| IDE | Cursor / VS / Rider — **Owner:** note what you use. |
| Verification date | 2026-04-18 (document); **Owner:** update when you complete launcher + in-game checks. |

## Module output

- **Repo build output (relative to project folder):** `bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`
- **Full path (this workspace clone):** `C:\Documents\Project Pandora\Bannerlord.RTSCameraLite\bin\Win64_Shipping_Client\Bannerlord.RTSCameraLite.dll`
- **Launcher / game expects:** entire module folder copied or linked to  
  `<Bannerlord install>\Modules\Bannerlord.RTSCameraLite\`  
  with `SubModule.xml` at that folder root and the DLL under `bin\Win64_Shipping_Client\`.

## Rules

- The local installed Bannerlord assemblies are the runtime authority.
- Public docs are guidance, not proof.
- If API docs disagree with local assemblies, local assemblies win.
- Slice 1 must not patch native methods.
- Slice 1 must not add camera behavior.
- Slice 1 must only prove module load stability.

## Next slice dependency

Slice 2 may begin only after:

- dotnet build passes
- launcher sees the module
- module can be enabled
- main menu loads
- custom battle loads
- no startup crash occurs
