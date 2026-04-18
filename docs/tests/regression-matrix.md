# Regression matrix — RTS Commander Doctrine (Bannerlord.RTSCameraLite)

**Legend:** ✓ = verify in this phase · — = not applicable · ◐ = optional / quick smoke only

| System / capability | Build | Launcher load | Main menu | Custom battle | RTS camera | Backspace toggle | Config fallback | Commander detection | Doctrine profile | Eligibility | Rally absorption | Cavalry doctrine | Native order executor | Targeting | Feedback | Diagnostics |
| --- |:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Build / module compiles** | ✓ | — | — | — | — | — | — | — | — | — | — | — | — | — | — | — |
| **Submodule loads (no crash to menu)** | ✓ | ✓ | ✓ | — | — | — | — | — | — | — | — | — | — | — | — | — |
| **Mission entry (custom battle)** | — | — | — | ✓ | — | — | — | — | — | — | — | — | — | — | — | — |
| **RTS camera pose / bridge** | — | — | — | ✓ | ✓ | ◐ | — | — | — | — | — | — | — | — | ◐ | — |
| **Commander mode on/off (Backspace / config)** | — | — | — | ✓ | ◐ | ✓ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ |
| **Invalid or missing JSON → defaults / merge** | ✓ | — | — | ✓ | — | — | ✓ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ |
| **Commander presence scan** | — | — | — | ✓ | ◐ | ✓ | — | ✓ | ◐ | ◐ | ◐ | ◐ | ◐ | — | — | ◐ | ✓ |
| **Doctrine profile computation** | — | — | — | ✓ | ◐ | ✓ | — | ✓ | ✓ | ◐ | ◐ | ◐ | ◐ | — | — | ◐ | ✓ |
| **Formation eligibility** | — | — | — | ✓ | ◐ | ✓ | — | ✓ | ✓ | ✓ | ◐ | ◐ | ◐ | — | ◐ | ◐ | ✓ |
| **Rally / absorption planning** | — | — | — | ✓ | ◐ | ✓ | — | ✓ | ✓ | ✓ | ✓ | ◐ | ◐ | — | ◐ | ◐ | ✓ |
| **Cavalry doctrine (spacing / lock / reform planning)** | — | — | — | ✓ | ◐ | ✓ | — | ✓ | ✓ | ✓ | ◐ | ✓ | ✓ | ◐ | ◐ | ◐ | ✓ |
| **Native order primitive executor (gated)** | — | — | — | ✓ | ◐ | ✓ | ◐ | ✓ | ✓ | ✓ | — | ✓ | ✓ | ◐ | ✓ | ◐ | ✓ |
| **Ground / formation targeting (resolver + markers)** | — | — | — | ✓ | ✓ | ✓ | — | ◐ | ◐ | ◐ | — | ◐ | ✓ | ✓ | ◐ | ◐ | ✓ |
| **Tactical feedback / markers / command validation UI text** | — | — | — | ✓ | ◐ | ✓ | ◐ | ◐ | ◐ | ✓ | ◐ | ◐ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Slice 20 diagnostics (`[Diag]`, F9, throttle)** | — | — | — | ✓ | ◐ | ✓ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ◐ | ✓ | ✓ | ✓ |

## Notes

- **Planned / future rows:** extend the table when new slices ship (e.g. full command router UX, additional doctrine hooks). Empty cells mean “not yet a dedicated regression column.”
- **Native executor:** deep pass should include both **`EnableNativeOrderExecution`** off (blocked / safe) and on (only when research-approved wiring is enabled in config).
- **Launcher load:** start game with mod enabled; reach main menu without unhandled exceptions in log (best-effort).

## Sign-off block (copy per release)

| Date | Game version | Mod ref | Tester | Matrix depth (smoke / full) | Notes |
| --- | --- | --- | --- | --- | --- |
| | | | | | |
