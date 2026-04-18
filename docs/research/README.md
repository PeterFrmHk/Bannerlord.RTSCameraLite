# Research documentation

This folder holds **engineering research** for `Bannerlord.RTSCameraLite` (**RTS Commander Doctrine**). It exists so integration decisions are **traceable**, **version-aware**, and **separate** from marketing prose in the repository root.

## Source of truth

- **Local installed Bannerlord assemblies** (`TaleWorlds.*.dll` under your game `bin` and, when applicable, `Modules\Native\bin\Win64_Shipping_Client` for view assemblies) are the **authority**.
- Official web API docs are **secondary**; when docs disagree with your local DLLs, **DLLs win**.
- Research notes may cite **installed mods** for patterns only; they are **not dependencies** unless explicitly added to `SubModule.xml`.

## Research documents (current)

| Document | Purpose |
| --- | --- |
| [`base-game-camera-scan.md`](base-game-camera-scan.md) | MissionView / MissionScreen / Mission camera APIs, candidate paths, restore findings. |
| [`camera-hooks.md`](camera-hooks.md) | Slice 5 ILSpy workflow and primary hook summary. |
| [`base-game-order-scan.md`](base-game-order-scan.md) | Formation / Team / OrderController / MovementOrder / OrderType model. |
| [`native-order-hooks.md`](native-order-hooks.md) | Minimal native order issuance cheat sheet (Slice 12 gate). |
| [`installed-mod-reference-scan.md`](installed-mod-reference-scan.md) | Patterns from local Modules + Steam Workshop installs (reference-only). |
| [`public-reference-scan.md`](public-reference-scan.md) | Additional public-surface notes (if present in repo). |

## Planned / optional research (backlog)

| Topic | Goal |
| --- | --- |
| **Cavalry order sequence** | Map advance/charge/reform primitives per build for Slice 13. |
| **Commander anchor placement** | Identify safe APIs for commander offset / formation facing (Slice 7B). |
| **Morale / training readers** | Document read-only signals for doctrine eligibility (Slice 8B). |
| **Gauntlet / order UI interaction** | Only if overlay work is explicitly approved; otherwise avoid. |

When adding a new research file, link it here and cross-link from `docs/slice-roadmap.md` if it gates a slice.
