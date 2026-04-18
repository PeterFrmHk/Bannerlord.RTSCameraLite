# Design overview — RTS Commander Doctrine

## Design thesis

A **formation is not a shape button**. A formation is a **discipline structure** produced by **commander presence**, **morale**, **training**, **equipment**, **rank**, **spacing**, and **battlefield pressure**. Geometry is an **outcome** of doctrine, not the primary input.

## Player fantasy

You are the **commander nucleus**: your position and state matter. Troops **rally to you first**, then **settle into row, rank, and spacing** only when they are close enough and the situation allows. When you are absent or suppressed, the line **loses disciplined behavior**—not instantly teleporting into parade-ground poses.

## Core loop (target)

1. Battle enters **RTS Commander Mode** (planned default; see roadmap).
2. You **read** the field with the RTS camera.
3. You **position** as the formation anchor (planned: commander behind the fighting face).
4. Troops **close to you** (rally), then **absorb** into doctrinal spacing.
5. You issue **native orders** (move, hold, charge, cavalry sequences) through validated routers—not micro-puppeting every agent.
6. Pressure, casualties, and morale **bend** doctrine outcomes.

## Commander doctrine rules

1. **Battles enter RTS Commander Mode by default** — *planned* (Slice 6A); today’s build may still require an explicit toggle until that slice lands.
2. **Backspace toggles RTS Commander Mode** — *planned* (Slices 6A–6B); current default toggle key is **config-driven** (see `config/rts_camera_lite.json`; default has been **F10** until migration).
3. **Commander is the formation nucleus** — *planned* (Slice 7).
4. **Troops rally to commander first** — *planned* (Slice 9A).
5. **Absorption into row/rank/spacing only when close enough** — *planned* (Slice 9A layout layer).
6. **No commander → no disciplined formation behavior** — *planned* (Slice 9B).
7. **Cavalry uses wider spacing** — *planned* (doctrine + layout).
8. **Cavalry uses native primitives where possible** — *planned* (Slice 13): advance toward target, native charge when close, release position lock near contact, **reform only after ≥ 30 m** from attacked formation or impact zone.
9. **Prefer native order systems over custom puppeteering** — **engineering principle**; `OrderController` / `Formation` public paths are preferred (see research docs).
10. **Version-sensitive APIs live behind adapters** — **engineering principle**; `CameraBridge` and order adapters isolate reflection and build drift.

## Camera and control rules

- RTS camera must **yield** on mission end, RTS off, and (planned) native **Backspace** conflict policies.
- Camera application uses **`MissionView.UpdateOverridenCamera`** and a **camera adapter** boundary; restore must be safe and repeatable.
- Input is routed through a **single ownership policy** so RTS and native controls do not fight unpredictably.

## Formation discipline rules

- **Spacing and depth** respond to unit type and doctrine profile (infantry vs cavalry).
- **Layout** is computed from commander proximity and eligibility, not from freeform “paint the line” UI in the baseline scope.

## Cavalry doctrine rules

- **Wider spacing** than dense infantry blocks.
- **Forward / advance** toward valid targets using native movement orders when available.
- **Charge** when within a doctrine-defined proximity band.
- **Release** position locks near contact to avoid pathological pinning.
- **Reform** only after **≥ 30 meters** separation from the last attacked formation or impact zone (cooldown discipline).

## Non-goals (baseline)

- Replacing the entire Bannerlord campaign loop.
- A full **Gauntlet** tactical HUD as a prerequisite for fun (optional overlays may be explored later, explicitly scoped).
- **Mandatory** Harmony / MCM / UIExtenderEx for core delivery.
- **Cheating** multiplayer integrity or server-side fairness (single-player focus).
