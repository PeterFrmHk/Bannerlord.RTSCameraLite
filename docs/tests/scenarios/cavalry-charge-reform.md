# Scenario: cavalry charge → reform (native sequence when enabled)

## Goal

When **`EnableNativeCavalryChargeSequence`** and native execution gates allow, observe **native orchestrator** progression; otherwise confirm **planning-only** cavalry doctrine still updates safely.

## Preconditions

- Cavalry-heavy player formation vs enemy presence on field.
- Read current `commander_config.json` for:
  - `EnableNativeCavalryChargeSequence`
  - `EnableNativePrimitiveOrderExecution`
  - `EnableNativeOrderExecution`

## Steps

1. Deploy; commander **ON**; select cavalry.
2. If using debug **native cavalry sequence** intent (see `docs/manual-test-checklist.md` / slice 16 notes): trigger only in a safe test battle.
3. Watch **`[Diag]`** `Cav` token transition across refreshes (`Nv:` / `Pl:` states).
4. Let sequence **complete or abort**; confirm mission remains stable.

## Pass criteria

- No crash on abort paths (commander dead, target lost) per orchestrator design.
- With all native gates **false**, no native sequence start — diagnostics still show planning **`Pl:`** only.

## Evidence

- 45–90s clip covering start → mid → end or abort.
- Attach config snapshot for the run.
