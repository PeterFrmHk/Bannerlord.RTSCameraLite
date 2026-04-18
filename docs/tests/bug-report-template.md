# Bug report template — Bannerlord.RTSCameraLite

Paste into GitHub / email / tracker. Remove placeholder lines.

## Summary

One sentence: what broke?

---

## Environment

| Field | Value |
| --- | --- |
| **Game version** | (Bannerlord — Steam branch or exe version) |
| **Mod version** | (`InformationalVersion` / git SHA / release tag) |
| **Other installed mods** | (List all; “Native only” if none) |
| **OS** | |
| **Hardware notes** | (GPU / ultrawide / etc. if relevant) |

---

## Reproduction steps

1.
2.
3.

**Expected frequency:** always / often / rare  

---

## Expected behavior

What should have happened?

---

## Actual behavior

What happened instead? Include timing (e.g. “within 5s of mission start”).

---

## Logs / screenshots / video

- `rgl_log_*.txt` snippets (search for module id / `[Diag]` / exceptions).
- Screenshots: before / after / settings.
- Short clip link if available.

---

## Suspected system

Check all that apply:

- [ ] RTS / commander camera
- [ ] Input / Backspace / keybind conflict
- [ ] `commander_config.json` / load merge
- [ ] Commander detection / anchor
- [ ] Doctrine / eligibility
- [ ] Rally / absorption
- [ ] Cavalry doctrine / native sequence
- [ ] Native order executor (`OrderController` path)
- [ ] Targeting / ground resolve / markers
- [ ] Tactical feedback / diagnostics text
- [ ] Other: ___

---

## Workaround (if any)

---

## Regression matrix cell (optional)

Link to row in `docs/tests/regression-matrix.md` / scenario ID from `test-scenarios.md`.
