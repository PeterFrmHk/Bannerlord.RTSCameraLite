# Manual test checklist — Slice 2 (mission shell & commander mode)

**Mod:** `Bannerlord.RTSCameraLite`  
**Goal:** Verify battle-only attach, default-on commander mode, Backspace toggle, no camera/commands.

---

## Prerequisites

- [ ] Game version matches your pinned `SubModule.xml` / `ModConstants.SupportedGameVersion` policy.
- [ ] Module built and deployed to `Modules\Bannerlord.RTSCameraLite\` (or your symlinked dev layout).
- [ ] **Custom Battle** (or any normal field battle) available from main menu.

---

## A. Supported mission — attach & default on

- [ ] Start **Custom Battle** (deployment or battle phase).
- [ ] Check **Debug** output (Visual Studio / log) for: `RTS Commander Mode active`.
- [ ] Check in-game **information** toast: **"RTS Commander Mode active"** (once per mission load).
- [ ] Confirm **no** camera jump or RTS camera movement (Slice 2 does not move camera).

---

## B. Backspace toggle

- [ ] With commander mode on (default), press **Backspace** once → mode **off** (one debug line: `DISABLED`).
- [ ] Press **Backspace** again → mode **on** (`ENABLED`).
- [ ] `ToggleCount` increases only on toggles (inspect log text).

---

## C. Emergency F10 (optional)

- [ ] If Backspace is blocked by another mod, press **F10** once → same toggle behavior (debug-only path).
- [ ] Do **not** document F10 to players as supported (development fallback only).

---

## D. Unsupported missions — no attach

- [ ] Enter a **tournament** / **arena duel** / **conversation** mission (or any mission where `MissionMode` is not Battle or Deployment).
- [ ] Confirm **no** `CommanderMissionView` attach logs for Slice 2 shell (no `RTS Commander Mode active` from this slice).

**Note:** Exact menu paths depend on campaign/progress; if uncertain, use `MissionMode` logging in a scratch build — Slice 2 gate is conservative.

---

## E. Mission end

- [ ] Finish or exit the battle.
- [ ] Confirm cleanup log (commander mode forced off) and **no** errors on return to campaign/menu.

---

## Acceptance summary

| Criterion | Pass |
| --- | --- |
| Build passes | [ ] |
| Custom battle loads | [ ] |
| `CommanderMissionView` attaches | [ ] |
| RTS Commander Mode **starts enabled** | [ ] |
| **Backspace** disables | [ ] |
| **Backspace** enables again | [ ] |
| Unsupported missions **do not** attach | [ ] |
