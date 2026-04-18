# 90-second demo script — RTS Commander Doctrine

**Intent:** One continuous capture (~90s) that matches the **current repo roadmap** and honest **work-in-progress** status. Adjust narration if a subsystem is disabled in `config/commander_config.json` for a given build.

**Prep:** Custom battle, small mixed force, enemy visible. Log or on-screen notes optional. See `media/README.md` for file naming when exporting.

---

## 0–10s — Battle starts in RTS Commander Mode

- **Visual:** Load into battle; show commander shell active (or toggle on if starting disabled).
- **Say:** “This is Bannerlord with the RTS Commander Doctrine mod — commander shell and mission-time services attach through a `MissionView`.”
- **Proof cue:** Brief pause on stable camera / UI if applicable.

## 10–20s — Backspace toggle (commander mode)

- **Action:** Press **Backspace** (or profile key from config) to show commander mode **off**, then **on** again.
- **Say:** “Commander mode is gated so native hotkeys and doctrine logic do not fight silently — toggle is intentional.”
- **Proof cue:** Optional log line or player message if enabled in build.

## 20–35s — Commander anchor / doctrine diagnostics

- **Visual:** With commander mode **on**, show formation selected or idle line; if **Slice 20** diagnostics are enabled, show the **portfolio diagnostics** readout (formation summary / doctrine snapshot text).
- **Say:** “Doctrine and presence are computed on a throttled cadence — diagnostics are portfolio evidence, not a shipping HUD.”
- **Proof cue:** Freeze-frame on diagnostic block or log excerpt.

## 35–50s — Rally absorption / row-rank spacing plan

- **Visual:** Brief shot of rally-related debug or planner-driven layout context (per `docs/slices/slice-12-commander-rally-absorption-model.md` and layout doctrine slices).
- **Say:** “Rally and spacing are modeled as data and scans before any risky native order blast — the mod separates planning from execution.”
- **Proof cue:** One screenshot-worthy frame of formation spacing or rally debug.

## 50–70s — Cavalry advance / charge / lock release / reform (doctrine path)

- **Visual:** Cavalry formation; if **native cavalry sequence** is enabled and executor is verified for your build, show sequence start (debug path per slice doc); otherwise show **doctrine-only** state logs (spacing, lock release, reform gating).
- **Say:** “Cavalry work is split: doctrine and impact heuristics in code, native primitives behind a single executor boundary — some paths stay off until research sign-off.”
- **Proof cue:** Log lines for state transitions or validation messages — do not claim full battle AI replacement.

## 70–85s — Command feedback / markers

- **Visual:** Throttled **marker fallback** text or optional particle if wired (Slice 19).
- **Say:** “Feedback is throttled and non-blocking — visuals are optional; text fallback keeps the portfolio honest when engine hooks are not proven.”
- **Proof cue:** `[Marker]` or command validation line in UI/log.

## 85–90s — Architecture summary card

- **Visual:** End card (static slide or terminal): **MissionView → adapters → doctrine → command router → native executor (when enabled)**.
- **Say:** “Layered runtime: mission shell, adapter boundaries, doctrine pipeline, guarded native orchestration — built for Bannerlord’s constraints.”

---

## Post-capture checklist

- [ ] Export clip: `media/clips/YYYY-MM-DD_rts-commander-90s-demo.mp4`
- [ ] Grab 3–5 stills into `media/screenshots/` using the same date prefix
- [ ] If you draw one diagram, drop in `media/diagrams/` (e.g. `doctrine-pipeline.png`)
