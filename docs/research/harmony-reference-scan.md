# Harmony reference scan (Slice: Harmony Integration Scaffold v1)

**Date:** 2026-04-18  
**Purpose:** Verify how **Bannerlord.Harmony** is published locally and decide compile-time vs runtime Harmony assembly policy for `Bannerlord.RTSCameraLite`.

## 1. Installed Bannerlord.Harmony (Steam Workshop)

| Field | Observed value |
|-------|----------------|
| **Source / path** | `C:\Program Files (x86)\Steam\steamapps\workshop\content\261550\2859188632\` |
| **Module Id** | `Bannerlord.Harmony` |
| **Display name** | `Harmony` |
| **Version** (SubModule.xml) | `v2.4.2.0` |
| **Entry DLL** | `Bannerlord.Harmony.dll` under `bin\Win64_Shipping_Client\` |
| **Managed Harmony** | `0Harmony.dll` and Mono.* assemblies ship **inside** the Bannerlord.Harmony module folder (not in the base game `bin`) |

**SubModule.xml pattern:** `<DependedModules />` empty; `DependedModuleMetadatas` mark Native/SandBoxCore/Sandbox/StoryMode/CustomBattle as optional `LoadAfterThis`. Downstream mods use `<DependedModule Id="Bannerlord.Harmony" .../>` and/or `DependedModuleMetadata id="Bannerlord.Harmony" order="LoadBeforeThis"`.

## 2. Game `Modules\Bannerlord.Harmony`

On typical Workshop-only installs, **no** copy exists under `GameRoot\Modules\Bannerlord.Harmony`; Harmony lives only under Workshop. The launcher still resolves the module id from Workshop content.

## 3. Other mods depending on Harmony (examples from local Workshop)

- **Bannerlord.ButterLib**, **Bannerlord.UIExtenderEx**, **Bannerlord.MBOptionScreen**: hard `DependedModule` on `Bannerlord.Harmony` with version metadata.
- **RTSCamera** / **RTSCamera.CommandSystem**: `DependedModuleMetadata` for Harmony `LoadBeforeThis` (soft ordering).
- Gameplay mods (e.g. Retinues, ClanArmies): explicit `DependedModule Id="Bannerlord.Harmony"`.

## 4. Public patterns (BUTR / Lib.Harmony)

- **BUTR Bannerlord.Harmony** module ships **Bannerlord.Harmony.dll** + **0Harmony.dll** + dependencies; mods reference **`HarmonyLib`** APIs at compile time via **Lib.Harmony** NuGet (or a local ref) and rely on **runtime** 0Harmony supplied by the Harmony module (or copy-local for non-standard setups).
- **SubModule** dependency: `<DependedModule Id="Bannerlord.Harmony"/>` (optionally with `DependentVersion`) so the launcher enforces load order and presence.

## 5. Decision (this repo)

| Question | Decision |
|----------|----------|
| **Module id to depend on** | **`Bannerlord.Harmony`** (matches Workshop BUTR package) |
| **Compile-time reference** | **Lib.Harmony** NuGet with **`IncludeAssets` = `compile`** (and **`PrivateAssets` = `all`**) so **0Harmony.dll is not published** with our mod output |
| **Runtime 0Harmony.dll** | **Provided by the Bannerlord.Harmony module** loaded by the game — **do not bundle** `0Harmony.dll` in `Bannerlord.RTSCameraLite` package |
| **Post-build guard** | Remove any stray `0Harmony.dll` / `Mono.Cecil*.dll` from `bin\Win64_Shipping_Client` if MSBuild copies them despite metadata |
| **Patches** | **None** in this slice; `HarmonyPatchService` only constructs `Harmony` when config gates allow — **no `PatchAll` / no patch types** until a later slice |

## 6. Final note

Launcher **dependency satisfied** ≠ **Harmony patches active**. This mod keeps **`EnableHarmonyPatches`** default **false**; runtime patching remains opt-in and gated with **`EnableMissionRuntimeHooks`** per scaffold policy.
