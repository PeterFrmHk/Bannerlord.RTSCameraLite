# Camera hooks research (Slice 5)

## ILSpy workflow (local Bannerlord binaries)

1. Install [ILSpy](https://github.com/icsharpcode/ILSpy/releases) or the ILSpy Visual Studio extension.
2. Point ILSpy at your live game folder (or the NuGet reference assemblies used by this repo):
   - `TaleWorlds.MountAndBlade.View.dll`
   - `TaleWorlds.MountAndBlade.dll`
   - `TaleWorlds.Library.dll`
3. Open `TaleWorlds.MountAndBlade.View.MissionViews.MissionView` and confirm the virtual hook:
   - `public virtual bool UpdateOverridenCamera(float dt)` (note the Bannerlord spelling **Overriden**).
4. Follow references from `MissionView.MissionScreen` into `MissionScreen` and inspect the concrete camera type behind `CombatCamera` / any `MissionCamera`-style surface.
5. Verify which members actually move the matrix (for example `SetCameraFrame`, `SetFrame`, or a writable `Frame` property) for your exact game build, then align `CameraBridge` reflection targets if names differ.

## API documentation link

TaleWorlds hosts versioned API docs here: https://apidoc.bannerlord.com/

For this repo reference pack (**1.2.12.66233**), start at the **1.2.12** tree: https://apidoc.bannerlord.com/v/1.2.12/

Then use the site search for **`MissionView`** and navigate to **`UpdateOverridenCamera(float)`** (spelled **`UpdateOverridenCamera`** in Bannerlord). Deep links move between doc drops; ILSpy on your matched binaries remains the ground truth.

## Selected hook (Slice 5)

- **Primary hook**: `MissionView.UpdateOverridenCamera(float dt)` returning `true` when this mod owns the camera for the frame.
- **Bridge target**: `MissionScreen` public camera surface (resolved via reflection on `MissionCamera` / `CombatCamera`), then `SetCameraFrame` / `SetFrame` / `Frame` setter where available.
- **Restore path**: parameterless `MissionScreen` public methods invoked by name (for example `ActivateMainAgentCamera`) via reflection when leaving RTS mode.

## Harmony stance for Slice 5

- **No Harmony** for this slice: the implementation relies on the engine-provided `MissionView` virtual and public reflection against `MissionScreen` / camera instances instead of patching private internals.
