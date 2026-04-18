# Feature proof checklist

Each row ties **evidence** to **implementation slice docs** under `docs/slices/`. Status is “proof plan,” not a claim that every path is enabled in default config.

| Feature name | What proves it | Clip needed | Screenshot needed | Diagnostic / text needed | Implementation slice |
|--------------|------------------|-------------|-------------------|----------------------------|----------------------|
| Loadable module + logging | Game starts with mod enabled; no spam exceptions | Boot → mission enter | SubModule / module list | `ModLogger` debug line once | [slice-1-foundation](../slices/slice-1-foundation.md) |
| Mission shell attached | `MissionView` tick without crash on supported mission types | 5s idle in battle | — | Log: commander shell initialized | [slice-2-mission-runtime-shell](../slices/slice-2-mission-runtime-shell.md) |
| Internal camera pose | Commander camera moves with keys; pose stable | Pan + move | Pose overlay or debug line | Internal pose log (if enabled) | [slice-4-internal-camera-pose-movement](../slices/slice-4-internal-camera-pose-movement.md) |
| Camera bridge (hard-gated) | Apply / restore messages match slice doc | Toggle commander; camera snap | Before/after framing | `CameraBridge` result in log | [slice-5-real-camera-bridge](../slices/slice-5-real-camera-bridge.md) + `docs/slice-hard-gates.md` |
| Commander config profile | JSON keys read; defaults merge | — | `config/commander_config.json` in editor | Load result log | [slice-6-config-control-profile](../slices/slice-6-config-control-profile.md) |
| Backspace / input ownership | Commander toggle; native conflict guard behavior | Backspace double-tap | — | Guard log | [slice-7-backspace-conflict-input-ownership](../slices/slice-7-backspace-conflict-input-ownership.md) |
| Commander presence | Formation shows commanded vs not | Swap formations | Presence summary | Eligibility / presence debug | [slice-8-commander-presence-model](../slices/slice-8-commander-presence-model.md) |
| Commander anchor | Anchor zone / behind-formation logic visible in logs or markers | Short pan | Anchor debug | Anchor resolver message | [slice-9-commander-anchor-behind-formation](../slices/slice-9-commander-anchor-behind-formation.md) |
| Doctrine profile | Doctrine scan lines; profile fields | 10s with doctrine debug | Log excerpt | Doctrine score output | [slice-10-formation-doctrine-profile](../slices/slice-10-formation-doctrine-profile.md) |
| Formation eligibility | Allowed formation types change with composition | Mixed troop swap | Table in log | Eligibility result | [slice-11-formation-eligibility-rules](../slices/slice-11-formation-eligibility-rules.md) |
| Rally + absorption model | Rally radii / absorption counts in planner output | Commander move near troops | Rally debug overlay (if any) | Rally planner summary | [slice-12-commander-rally-absorption-model](../slices/slice-12-commander-rally-absorption-model.md) |
| Cavalry spacing / lock / reform (doctrine) | State machine transitions in log | Cavalry idle → contact | Doctrine state | Cavalry doctrine debug | [slice-13-cavalry-spacing-charge-release](../slices/slice-13-cavalry-spacing-charge-release.md) |
| Native primitive executor (hard-gated) | Single executor boundary; config killswitch | Issue debug command | Config flags | Native result `NotWired` / success per build | [slice-14-native-command-primitive-executor](../slices/slice-14-native-command-primitive-executor.md) |
| Command router + restrictions | Validation + blocked vs allowed | Invalid command attempt | Router log | `CommandValidationResult` text | [slice-15-command-router-restriction-integration](../slices/slice-15-command-router-restriction-integration.md) |
| Native cavalry sequence orchestrator | Sequence registry tick; refused when unwired | Start sequence debug key | — | Sequence abort / advance lines | [slice-16-native-cavalry-command-sequence-orchestrator](../slices/slice-16-native-cavalry-command-sequence-orchestrator.md) |
| Temporary markers / feedback | Throttled marker or `[Marker]` text | Move / charge / reform beat | Marker burst or text | Tactical feedback line | [slice-19-command-marker-temporary-visual-target-feedback](../slices/slice-19-command-marker-temporary-visual-target-feedback.md) |
| Portfolio diagnostics panel | Diagnostics visibility toggle; snapshot text | Toggle diagnostics | Fullscreen log | Formation diagnostics snapshot | [slice-20-formation-debug-panel-portfolio-diagnostics](../slices/slice-20-formation-debug-panel-portfolio-diagnostics.md) |

## Legend

- **Clip needed:** 3–8s segment you can lift into a reel.
- **Screenshot needed:** still that reads without audio.
- **Diagnostic text needed:** log or on-screen proof for skeptical reviewers.
