# Dependency policy — RTS Commander Doctrine

## Launcher / `SubModule.xml`

- **Official modules** remain required: Native, SandBoxCore, Sandbox, StoryMode, CustomBattle.
- **Bannerlord.Harmony** (BUTR Workshop module id `Bannerlord.Harmony`) is a **declared dependency** so the launcher can resolve load order and supply **runtime `0Harmony.dll`** to the game. Install **Bannerlord.Harmony** before this mod (Workshop or equivalent).
- **No** mandatory **MCM**, **ButterLib**, or **UIExtenderEx** for this mod’s baseline.

## Harmony (compile vs runtime)

- **Compile-time:** the project references **`Lib.Harmony`** NuGet with **compile-only** assets so **`0Harmony.dll` is not bundled** with this mod’s output; runtime Harmony comes from the **Bannerlord.Harmony** module.
- **Runtime patches:** **`EnableHarmonyPatches`** defaults **false** in `commander_config.json`. When **false**, no Harmony instance is applied for patching. A future slice may register patches; they remain **off** until explicitly enabled and documented.
- **Scaffold gate:** even when **`EnableHarmonyPatches`** is **true**, **`HarmonyPatchService`** only initializes after **`EnableMissionRuntimeHooks`** is **true** (same policy as mission experimental code). Scaffold v1 registers **zero** patches.

## Optional libraries (future)

- **MCM / ButterLib / UIExtenderEx** — **optional later**, not baseline, unless a future slice adds in-game settings UI that requires them.
- Any adoption must update **`SubModule.xml`**, **`version-support.md`**, and the **regression matrix** (`docs/tests/regression-matrix.md`).

## BLSE / external loaders

- **BLSE** (Bannerlord Software Extender) or similar loaders are **optional** unless a future need appears (e.g. debugging hooks only available through such a host).
- **ButterLib** may reference **`BLSE.AssemblyResolver`** in its own XML; that is unrelated to this mod’s dependency list unless we add ButterLib later.

## Native / TaleWorlds references

- Compile-time references are pinned via **`BannerlordRefsVersion`** in the `.csproj` (see `version-support.md`).
- That is a **build** dependency, not a player-installed NuGet package on the game PC.
