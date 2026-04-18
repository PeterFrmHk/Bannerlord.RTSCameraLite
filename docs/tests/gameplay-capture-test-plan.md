# Gameplay capture test plan — portfolio & regression evidence

Ties **manual regression** to **short clips** and **still frames** so reviewers can trust doctrine + commander behavior without sitting in the mission.

## Conventions

| Term | Meaning |
| --- | --- |
| **Clip proves** | Minimum footage that shows the behavior unambiguously |
| **Debug overlay** | In-game text: `[Cmd]…`, `[Marker]…`, `[Diag]…` (Slice 20), `InformationManager` lines — not a Gauntlet HUD |
| **Before / after** | Still: commander OFF vs ON, or config off vs on |

## Feature → evidence map

| Feature | What clip proves | What overlay should show | Before / after suggestion |
| --- | --- | --- | --- |
| **RTS camera** | Formation visible; camera moves with keys; height/yaw sane | Optional: none required | Commander OFF (native) vs ON (RTS pose) |
| **Backspace toggle** | Single press toggles mode; no stuck keys | Mode messages if enabled | Same scene: overlay line or log reference |
| **Config fallback** | Broken JSON still boots; defaults applied | Log warning once (if observed) | N/A or split config file demo |
| **Commander detection** | Selected formation; `[Diag]` Cmd YES/NO consistent with troops | `[Diag]` segment `Cmd …` | Captain vs no leader roster |
| **Doctrine profile** | `[Diag]` discipline + role change when roster differs | `Disc`, `Role` segments | Two formations different class |
| **Eligibility** | `[Diag]` `Allow…` changes when doctrine changes | `Allow` abbreviated list | Low vs high discipline setup |
| **Rally absorption** | `[Diag]` rally counts change as troops spread | `Rally a/b abs … asg …` | Troops tight vs spread |
| **Cavalry doctrine** | `[Diag]` `Cav Pl:…` or `Nv:…` | Cavalry state token | Infantry formation vs cavalry formation |
| **Native order executor** | With gates off: no orders; with gates on: validated behavior only | `[Diag]` `Native ex=… pr=…` | Same battle: flip config + restart mission |
| **Targeting** | Ground marker or `[Marker]` / `[Ground]` text | Success/fail throttled lines | Aim sky vs ground |
| **Feedback** | Command validation keys (slice 15 debug) | `[Cmd]` lines | Invalid vs valid intent |
| **Diagnostics** | F9 toggles `[Diag] ON/OFF`; refresh ~1s | Multi-formation `;;` block | ON with two+ groups |

## ~90 second demo capture path (single take)

Use **custom battle**, player **mixed** army, small field.

| Sec | Action | Capture note |
| --- | --- | --- |
| 0–10 | Launch → main menu → start custom battle | Proves launcher + mission load |
| 10–25 | Enable commander; pan RTS camera | Still: wide establishing shot |
| 25–40 | Select infantry; press **F9** on → show `[Diag]` block | Still: readable `[Diag]` line |
| 40–55 | Toggle commander **off** then **on** | Before/after camera |
| 55–70 | Select cavalry group; show `[Diag]` Cav segment | Proves multi-formation diag |
| 70–90 | Optional: trigger one throttled feedback (`[Cmd]` or marker) | Proves feedback path |

**Narration (optional):** read formation class + one `[Diag]` token so audio reinforces video.

## Shorter clip checklist (per PR)

- [ ] 10s: mission start + commander enable  
- [ ] 10s: `[Diag]` ON with refresh visible once  
- [ ] 10s: native gates visible (`ex=` / `pr=`)  

## Still frame checklist

- [ ] Commander OFF — native camera  
- [ ] Commander ON — RTS camera same scene  
- [ ] `[Diag]` line in **InformationManager** (UI visible)  

## Storage

Prefer attaching clips to the PR or `slice-acceptance-template.md` for the milestone; avoid committing large binaries into git unless the repo policy allows `LFS`.
