# Public deployment scan — Bannerlord mod layout (v1)

**Purpose:** Record public references for **module folder layout**, **DLL placement**, and **packaging conventions** used to validate this repo’s `scripts/` and docs.

**Date compiled:** 2026-04-18 (access date for web sources below).

---

## Sources

| URL | Date accessed | Type | What it confirms | Deployment claim extracted |
| --- | --- | --- | --- | --- |
| https://docs.bannerlordmodding.com/_intro/folder-structure.html | 2026-04-18 | Community docs (modding hub) | Only **module directory + valid SubModule.xml** are required for launcher detection; **`bin/Win64_Shipping_Client`** holds compiled binaries; optional asset/GUI/ModuleData folders. | Managed mod DLLs live under **`ModuleName/bin/Win64_Shipping_Client/`**; see also Basic C# mod tutorial link from same site. |
| https://docs.bannerlordmodding.com/_xmldocs/submodule | 2026-04-18 | Community docs | SubModule XML schema reference for launcher. | **`SubModule.xml`** at **module root** defines Id, dependencies, and managed DLL entry. |
| https://github.com/BUTR/Bannerlord-Mod-Template | 2026-04-18 | Example / community standard | BUTR template ecosystem; typical MSBuild post-build copy into **`$(GameFolder)/Modules/$(ModuleName)/`** pattern in ecosystem tooling. | Binaries target **`Modules/<Module>/bin/Win64_Shipping_Client`**; `_Module` content copies to module root in template workflows. |
| https://github.com/BUTR/Bannerlord-Mod-Template/blob/master/src/EXAMPLE/EXAMPLE.csproj | 2026-04-18 | Example repo file | Illustrative csproj uses **GameFolder**-style deployment paths in template projects. | Confirms **game-relative** `Modules/<name>/bin/Win64_Shipping_Client` as deploy destination for builds. |
| https://github.com/haggen/bannerlord-module-template | 2026-04-18 | Example repo | Alternate template; documents extracting/copying module into game **Modules** directory. | Manual install = full module folder under **`…/Mount & Blade II Bannerlord/Modules/`**. |
| https://www.nexusmods.com/mountandblade2bannerlord/mods/32 | 2026-04-18 | Community hub | Nexus “Module Template” description references editing csproj / deployment for Bannerlord modules. | Reinforces **VS/dotnet** build + deploy into game module folder (tooling-specific). |
| https://steamcommunity.com/app/261550/discussions/0/2796125475568148993/ | 2026-04-18 | Community (Steam) | Players locate **`Modules`** under the game install (Steam “Browse local files”). | Install path is always under **game root**, not Documents (for module binaries). |

**Official TaleWorlds:** No separate official URL was required for this slice beyond what the community **Bannerlord Documentation** site mirrors; TaleWorlds launcher behavior is observed via game + **Native** module layout on disk (see `docs/research/local-steam-mod-scan.md`).

---

## .NET Framework / PackageReference DLL copy expectations

- SDK-style projects with `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` (this repo) copy **NuGet** dependency assemblies (e.g. **System.Text.Json**) next to the output DLL under **`bin/Win64_Shipping_Client`**.
- **TaleWorlds.\*** references marked **`Private=false`** are **not** copied to output; the game loads them from its own `bin` — deploy scripts must **not** copy engine DLLs from the game into the mod package.

---

## Conventions applied in this repository

1. **Module root:** `SubModule.xml`, optional `config/`, **`bin/Win64_Shipping_Client/*.dll`** (mod + copied NuGet deps only).
2. **Do not** ship a “DLL-only” manual install without the rest of **`bin/Win64_Shipping_Client`** when PackageReference assemblies are required.
3. **Zip / folder** installs should preserve that tree so the launcher resolves `DLLName` and dependencies consistently.

---

*End of public deployment scan v1.*
