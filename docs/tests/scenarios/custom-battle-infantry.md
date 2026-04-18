# Scenario: custom battle — infantry focus

## Goal

Validate **commander detection**, **doctrine**, **eligibility**, **rally/absorption** counts, and **diagnostics** on a formation that is **not** cavalry-heavy.

## Setup

1. **Custom battle** — small map, short round time optional.
2. Player roster: **infantry-heavy** (shield / spear acceptable); avoid large cavalry AI stacks if you want clean `Cav —` in `[Diag]`.
3. Config: defaults from `config/commander_config.json` unless testing a specific flag; ensure `EnableDiagnostics: true` for overlay text.

## Steps

1. Start battle; confirm **no crash** through deployment.
2. Enable **commander mode** (default **Backspace** unless rebound).
3. Select a **player infantry** formation with troops deployed.
4. Wait **≥2s** for presence/doctrine scans; press **F9** to turn diagnostics **ON**.
5. Read one **`[Diag]`** refresh line: expect **`Cmd`** coherent with roster (hero captain vs sergeant vs missing per design).
6. Optional: use command-router **debug keys** documented in `docs/manual-test-checklist.md` (H/C/G/M) and confirm **`[Cmd]`** validation text only (no unintended native execution if gates off).

## Pass criteria

- No exceptions; commander toggles reliably.
- `[Diag]` shows **discipline**, **eligibility Allow…**, **rally** integers, **Anchor** token.
- Cavalry segment **`Cav —`** or planning-only state **not** claiming native sequence unless you started one.

## Evidence

- Screenshot: `[Diag]` line for infantry.
- Optional: 15s clip panning + one toggle.
