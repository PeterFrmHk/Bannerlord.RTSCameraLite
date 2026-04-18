# Load order notes — RTS Commander Doctrine

## Recommended placement (when unsure)

Bannerlord applies module behavior in **dependency order** and **user load order** (launcher / Vortex / etc.). This mod:

- Declares vanilla module dependencies in **`SubModule.xml`** (`Native`, `SandBoxCore`, `Sandbox`, `StoryMode`, `CustomBattle`) — it is not a “library” mod and should load **after** those base modules.
- Should typically sit **after** pure libraries you explicitly depend on (none required today) and **before or after** other gameplay mods depending on who must win a conflict — see risks below.

**Practical default:** enable **only** `Native` stack + this mod for first smoke; then add mods one at a time (`mod-stack-testing.md`).

## What to test after changing load order

Run this **minimum** checklist after any reorder:

1. **Main menu → custom battle** — no crash to desktop before deployment.
2. **Commander mode toggle** (default Backspace) — on/off once.
3. **RTS camera** — small pan + height change; release toggle and confirm native camera returns.
4. **Optional:** `[Diag]` (F9) one refresh line if diagnostics enabled — proves mission shell + feedback path.

If any step fails **only** after reorder, treat as **load-order / interaction** bug; capture order screenshot and mod list.

## Why camera / order / input mods are high-risk neighbors

| Neighbor type | Risk |
| --- | --- |
| **Camera mods** | Replace or fight `MissionView` camera ownership; may prevent bridge restore or double-apply frames. |
| **RTS / “free camera” mods** | Same input stack and camera hooks; may consume keys bound to commander or RTS movement. |
| **Order menu / battle command UI** | Touch `OrderController`, selection, or keybinds used for formations; can deselect formations or issue competing orders. |
| **Input rebinding / global hotkey mods** | May capture Backspace or diagnostics keys before the mission view sees them. |

**Mitigation:** document the conflicting mod name + version, try moving this mod **above** vs **below** the neighbor once each, and file a report with results (`../tests/bug-report-template.md`).
