# Manual test checklist — Slice 16 (native cavalry command sequence)

Use a **cavalry-heavy** player formation, **commander mode on**, valid **commander**, and an **enemy** formation in range for debug key **N**.

Enable in `commander_config.json` (both required to start):

- `EnableNativePrimitiveOrderExecution`: `true` (after executor is verified wired; until then expect start to fail at probe)
- `EnableNativeCavalryChargeSequence`: `true`

Optional: `EnableCavalrySequenceDebug`: `true` to see the five transition lines.

## Checklist

- [ ] Build passes (`dotnet build -c Release`)
- [ ] Custom battle loads with module enabled
- [ ] Cavalry sequence registry initializes (no errors on mission tick)
- [ ] Non-cavalry-heavy formation does not pass restriction / cannot meaningfully run sequence (use infantry formation with key N — expect validation or orchestrator refusal)
- [ ] No commander blocks sequence (restriction + start validation)
- [ ] `EnableNativePrimitiveOrderExecution: false` blocks start
- [ ] `EnableNativeCavalryChargeSequence: false` blocks start
- [ ] Not-wired native executor blocks start safely (probe returns `NotWired` until Slice 3 wiring is done)
- [ ] With wired executor: valid cavalry sequence advances toward target (advance/move issued on throttled ticks)
- [ ] With wired executor: native charge issued at forward-to-charge distance
- [ ] Position lock releases near target or on impact heuristic (`Cavalry lock released.`)
- [ ] Lock does not reactivate during close contact (reform gated by distance + cooldown + `CavalryReformPolicy`)
- [ ] Reform blocked before ~30m disengagement + cooldown (`Cavalry reform ready.` only after policy allows)
- [ ] Reform allowed after distance + cooldown + valid commander + safe nearest enemy when known
- [ ] Commander death aborts sequence (remove from registry on next throttled tick)
- [ ] Mission end / behavior remove clears registry (no stale sequences next mission)
- [ ] No per-frame log spam (transitions only on 0.5s cadence and only the five strings when debug flag is on)

## Debug

With commander mode on and commander mission view active, release **N** to attempt `NativeCavalryChargeSequence` against the nearest enemy formation.

Expected log strings (when `EnableCavalrySequenceDebug` is true):

1. `Cavalry advancing.`
2. `Cavalry charge issued.`
3. `Cavalry lock released.`
4. `Cavalry reform ready.`
5. `Cavalry reassembled.`
