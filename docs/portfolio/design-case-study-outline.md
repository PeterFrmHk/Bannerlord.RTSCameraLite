# Design case study outline

Use this as the spine for a 800–1,500 word case study (blog, PDF, or interview prep). Numbers are **suggested** section weights.

## 1. Context (10%)

- Bannerlord’s default battle loop: what players already get from native formations and camera.
- Goal of RTS Commander Doctrine: **commander-first clarity** without replacing the whole game.

## 2. Problem framing (15%)

- Pain: commander intent is hard to express and hard to **debug** when something feels wrong mid-battle.
- Non-goals: full tactical overlay, persistent HUD clutter, unverified native hook spam.

## 3. Design pillars (20%)

- **Gated commander mode** — explicit on/off and input ownership awareness.
- **Doctrine before orders** — scores, eligibility, and spacing as data.
- **Evidence-friendly runtime** — diagnostics and throttled feedback for portfolio and QA.

## 4. Systems architecture (20%)

- Mission-time shell (`MissionView`), adapters, doctrine modules, command router, optional native orchestration.
- Reference 1–2 diagrams from `media/diagrams/` when available.

## 5. Key workflows (20%)

- Commander presence + anchor (positioning discipline narrative).
- Rally / absorption / spacing plan (logistics fantasy).
- Cavalry: assemble → pressure → contact → disengage → reform (even if some stages are planning-only until gates lift).

## 6. Tradeoffs & constraints (10%)

- Engine API variance; hard gates; `NotWired` paths; config defaults favor safety.

## 7. Results & WIP honesty (5%)

- What is demonstrable **today** in video (link clips).
- What remains on roadmap (`docs/slice-roadmap.md`).

## 8. Personal reflection (optional, 5%)

- What you would ship next if this were a team product vs solo portfolio.
