# Dependency policy — RTS Commander Doctrine

## Default: no hard third-party dependencies

- The shipped module is intended to run with **vanilla** dependencies declared in **`SubModule.xml`** only (see repository file for current list).
- **No** mandatory requirement on **MCM**, **ButterLib**, **UIExtenderEx**, **Harmony**, or **BLSE** for the baseline build described in this repo.

## Harmony

- **Avoid** unless Slice 0–class research shows **no** public alternative **and** maintainers accept the cost.
- If Harmony is ever added: **one** assembly (or clearly bounded subfolder), **no** scattered patches, document **target type + reason** in a slice doc and in `known-failures.md` if behavior is fragile.

## Optional libraries (future)

- **MCM / ButterLib / UIExtenderEx** — **optional later**, not baseline, unless a future slice adds in-game settings UI that requires them.
- Any adoption must update **`SubModule.xml`**, **`version-support.md`**, and the **regression matrix** (`docs/tests/regression-matrix.md`).

## BLSE / external loaders

- **BLSE** (Bannerlord Software Extender) or similar loaders are **optional** unless a future need appears (e.g. debugging hooks only available through such a host).
- Players using BLSE should still report **game version + mod version**; loader version is secondary but welcome.

## Native / TaleWorlds references

- Compile-time references are pinned via **`BannerlordRefsVersion`** in the `.csproj` (see `version-support.md`).
- That is a **build** dependency, not a player-installed NuGet package on the game PC.
