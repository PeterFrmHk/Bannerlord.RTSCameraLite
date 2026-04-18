# Mod stack testing template

Use this template for **manual** compatibility passes. Copy the table into an issue or `slice-acceptance-template.md` attachment.

## Environment

| Field | Value |
| --- | --- |
| Game version | |
| Mod DLL / `InformationalVersion` | |
| Launcher (Steam / GOG / Xbox app) | |
| BLSE / none | |

## Stacks (run in order)

| Stack ID | Mods enabled (name + Nexus/Steam id + version) | Load order (top → bottom) | Result (PASS / FAIL) | Notes |
| --- | --- | --- | --- | --- |
| **A — Clean game** | Native + Sandbox modules only + **this mod** | Default | | |
| **B — This mod only (strict)** | Same as A; confirm no optional QoL mods | Default | | |
| **C — + common libraries** | A + *list each* (e.g. Harmony used by others — note if present) | | | |
| **D — + camera mod** | A + *one* camera mod | Try **below** then **above** this mod | | |
| **E — + command / order UI mod** | A + *one* battle UI mod | Try both relative positions | | |
| **F — + troop AI mod** | A + *one* AI overhaul | Default then one reorder | | |

## Per-stack smoke (repeat)

- [ ] Reach custom battle deployment
- [ ] Commander toggle once
- [ ] RTS camera 5s
- [ ] Toggle off → native camera

## Failure reporting

Attach: `rgl_log` excerpt, stack table row, **screenshot of launcher load order**.
