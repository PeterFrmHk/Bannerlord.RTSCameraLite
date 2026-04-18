# Known conflict zones — third-party mod categories

This is a **risk map**, not a ban list. Another mod may work fine; these categories historically collide with **camera**, **orders**, or **mission view** extensions.

## High risk

| Category | Examples (illustrative) | Mechanism |
| --- | --- | --- |
| **Camera mods** | Alternative battle cameras, cinematic cameras | Competing `MissionView` camera updates or input capture. |
| **RTS / “RTS camera” mods** | Other top-down or strategy-style camera modules | Same conceptual surface as this mod’s commander camera path. |
| **Order menu / radial command mods** | Custom order wheels, expanded battle UI | `OrderController`, selection lists, or order keybinds. |
| **Battle command / formation UI** | Extra formation panels, hotkey expansions | Overlapping keys or formation selection assumptions. |

## Medium risk

| Category | Examples (illustrative) | Mechanism |
| --- | --- | --- |
| **Troop AI / behavior** | AI overhauls, charge behavior tweaks | Indirect: timing of orders vs doctrine scans; harder to bisect. |
| **Formation overhaul** | Mods that replace arrangement / spacing logic | Reads may disagree; native orders may fight custom AI movement. |
| **HUD / UI** | InformationManager-heavy overlays, custom battle HUDs | Usually lower risk unless they hook the same mission phases or steal focus. |

## Dependency stacks (if adopted later)

If the project later opts into **MCM**, **ButterLib**, **UIExtenderEx**, or similar:

- Treat them as **version-locked stacks** — one outdated DLL breaks many mods.
- Document the **exact** stack in every bug report.
- This mod’s **baseline** remains **no** hard requirement on those libraries until a slice explicitly adds them (`dependency-policy.md`).

## What we do not claim

- Compatibility with **every** Nexus mod combination.
- Stability under **unofficial** game branches or beta executables not listed in `version-support.md`.
