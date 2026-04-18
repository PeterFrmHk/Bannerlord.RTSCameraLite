# Public release packaging plan — RTS Commander Doctrine

**Scope:** Manual steps and **checklists** for shipping a player-installable **`Modules/Bannerlord.RTSCameraLite/`** tree. This document does **not** automate CI or publish to Nexus/Steam Workshop; add that in a later slice if needed.

## Goals

- One **obvious** folder layout that matches Bannerlord’s module loader.
- **Version strings** aligned across metadata and docs.
- **No** accidental inclusion of dev-only files (`local.props`, `.vs`, `obj/`, PDB-only symbols policy below).

## Target layout (installed game)

```text
Mount & Blade II Bannerlord/Modules/Bannerlord.RTSCameraLite/
  SubModule.xml
  bin/Win64_Shipping_Client/
    Bannerlord.RTSCameraLite.dll
    (optional) Bannerlord.RTSCameraLite.pdb   ← see PDB policy below
  config/
    commander_config.json   ← ship defaults; player edits persist here
```

Optional: include a short **`README.txt`** in the module root pointing to the GitHub repo and `docs/compatibility/` (not required for loader).

## Build prerequisites

1. **.NET SDK** able to build **net472** (see repository `TargetFramework`).
2. **References:** either
   - set `BannerlordGameFolder` / `BANNERLORD_INSTALL` in `local.props` for game DLL refs, or  
   - rely on NuGet **`Bannerlord.ReferenceAssemblies`** matching **`BannerlordRefsVersion`** in `Bannerlord.RTSCameraLite.csproj`.
3. From repo root:

```powershell
dotnet build -c Release
```

Output DLL (this repo): **`bin/Win64_Shipping_Client/Bannerlord.RTSCameraLite.dll`** (see `AppendTargetFrameworkToOutputPath` / `OutputPath` in csproj if you change them).

## Version alignment checklist

Before tagging a release, update **all** of the following in one PR or commit:

| Location | Field | Notes |
| --- | --- | --- |
| `SubModule.xml` | `<Version value="…"/>` | Player-visible in launcher |
| `Bannerlord.RTSCameraLite.csproj` | `InformationalVersion` | Matches tag convention if used |
| `Bannerlord.RTSCameraLite.csproj` | `AssemblyVersion` / `FileVersion` | Bump when you want binary identity change |
| `docs/compatibility/version-support.md` | Pin paragraph | Must match `BannerlordRefsVersion` unless you re-pin |

## Files to **include** in a public zip

- **`SubModule.xml`**
- **`bin/Win64_Shipping_Client/Bannerlord.RTSCameraLite.dll`**
- **`config/commander_config.json`** — ship repo defaults so first launch matches documented behavior (module may also recreate; keep one source of truth in repo).

## Files to **exclude** (do not ship)

- **`local.props`** (paths are machine-specific)
- **`obj/`**, **`bin/`** outside the single shipping client folder if you use multi-target outputs
- **Source** (`.cs`, `.csproj`) — not needed at runtime unless you intentionally ship a source drop (unusual)
- **Internal docs** — optional; most players never read them inside `Modules/`. Prefer linking from Nexus/Steam description to the repo.

## PDB policy

- **Nexus / public zip:** omit **`.pdb`** unless you explicitly want crash reports with line symbols (increases size; some hosts discourage).
- **Private test build:** include PDB for testers.

## Pre-publish smoke (minimum)

Run on a **clean** `Modules` folder (only vanilla + this mod):

1. Launcher loads; module appears with correct **name** and **version**.
2. **Custom battle** → deployment → commander toggle → RTS camera 30s.
3. **`config/commander_config.json`** present; toggling a visible setting (e.g. diagnostics) persists after restart.

Cross-reference: `docs/tests/regression-matrix.md`, `docs/compatibility/mod-stack-testing.md` stack **A**.

## Post-publish

- Tag git **`vX.Y.Z`** matching `SubModule.xml`.
- Attach or link **changelog** (GitHub Releases or Nexus description).
- Update **`docs/compatibility/version-support.md`** “known tested” row with **game build + mod tag** if validated on retail.

## Nexus / Steam Workshop (manual)

- **Description:** game version range, **ref pin** (`BannerlordRefsVersion`), link to compatibility README, load-order warning.
- **Dependencies:** mirror **`SubModule.xml`** `DependedModules` only; do not claim optional libraries unless you add them.
- **Screenshots:** commander on/off + one `[Diag]` line if diagnostics remain a selling point.

## Security note

Build only from **trusted** sources; do not redistribute DLLs built on unknown machines without verifying hashes.

## Non-goals (this document)

- Automated zip or Steam upload pipeline.
- Code signing or Microsoft Store packaging.
- Changing gameplay or adding dependencies.
