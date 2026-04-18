# Repeatable test scenarios — RTS Commander Doctrine

Each scenario is **manual**, **mission-based**, and should be runnable on a **clean custom battle** unless noted. Cross-link to `scenarios/` for scripted steps.

| ID | Scenario | Primary goal | Scenario doc |
| --- | --- | --- | --- |
| S-01 | **Infantry-only battle** | Commander detection, doctrine, eligibility, rally text, diagnostics on line formations | [`scenarios/custom-battle-infantry.md`](scenarios/custom-battle-infantry.md) |
| S-02 | **Cavalry-heavy battle** | Cavalry doctrine row, spacing logs, optional native sequence if enabled | [`scenarios/custom-battle-cavalry.md`](scenarios/custom-battle-cavalry.md) |
| S-03 | **Mixed force** | Multiple formation classes in one battle; multi-line `[Diag]` refresh | [`scenarios/mixed-army-command.md`](scenarios/mixed-army-command.md) |
| S-04 | **No / weak commander** | Policy + restriction paths; “no commander” denial where applicable | [`scenarios/no-commander-formation-denial.md`](scenarios/no-commander-formation-denial.md) |
| S-05 | **Invalid config** | Defaults / harmonize paths; recovery without delete reinstall | [`scenarios/invalid-config-recovery.md`](scenarios/invalid-config-recovery.md) |
| S-06 | **Native executor not wired / blocked** | `EnableNativeOrderExecution` false, or primitives blocked; no crash; documented results | *Use S-02 / S-03 + config toggles; see `known-failures.md`* |
| S-07 | **Cavalry charge → reform path** | Native sequence orchestrator when enabled; otherwise planning-only | [`scenarios/cavalry-charge-reform.md`](scenarios/cavalry-charge-reform.md) |
| S-08 | **Camera toggle restore** | Commander off → native camera restored; no stuck input | [`scenarios/camera-toggle-restore.md`](scenarios/camera-toggle-restore.md) |

## Native executor “not wired” scenario (S-06 detail)

Repeatable checks:

1. **`EnableNativeOrderExecution: false`** (keep `EnableNativePrimitiveOrderExecution` as needed for router tests): issue debug keys or doctrine paths that would call the executor — expect **blocked** / no crash, `[Diag]` may show `Native ex=False pr=…`.
2. **Research / policy blocked:** if a future build returns `NotWiredResult` for a primitive, confirm **single choke point** (`NativeOrderPrimitiveExecutor`) and document under `known-failures.md`.

## Scenario prerequisites (all)

- Module deployed to `Modules/Bannerlord.RTSCameraLite/` with matching `SubModule.xml`.
- Optional: note `Bannerlord.RTSCameraLite.csproj` `InformationalVersion` for the report.
