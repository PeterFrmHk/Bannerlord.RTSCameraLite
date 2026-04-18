# Recruiter summary — RTS Commander Doctrine (Bannerlord II)

## One-paragraph pitch

**RTS Commander Doctrine** is a work-in-progress *Mount & Blade II: Bannerlord* mod that adds a **commander-first battle layer**: configurable commander mode, doctrine and eligibility scans, rally and spacing planning, cavalry doctrine state machines, and guarded native command routing — all anchored in **documented slices** and **adapter boundaries** so risky engine integration stays isolated and shippable. The project doubles as a **portfolio system**: diagnostics, marker feedback, and this evidence pack make design intent and technical depth visible without pretending the full roadmap is finished.

## Skills demonstrated

- Systems breakdown under tight engine constraints (Bannerlord `MissionView`, TaleWorlds APIs).
- Gameplay-adjacent **technical design**: doctrine pipeline, state machines, throttled scans.
- Production judgment: hard gates for camera/order hooks, config-driven kill switches, defensive execution.
- Portfolio communication: slice specs, manual checklists, demo script, recruiter-facing summaries.

## Systems designed

- Commander mode shell and input ownership policy (conflict with native keys called out in docs).
- Doctrine scoring, formation eligibility, rally/absorption **planning** models.
- Cavalry doctrine (spacing, lock release, reform gating) and optional **native cavalry sequence** orchestration.
- Command router validation, native primitive executor choke point, tactical feedback and **temporary markers**.

## Technical constraints solved (or explicitly deferred)

- **Engine uncertainty:** order and camera hooks vary by build; research docs + `NotWired` / hard-gate patterns avoid silent failures.
- **Performance:** doctrine and markers run on **intervals**, not per-agent loops every frame in the hot path design.
- **Truthfulness vs marketing:** features are described against slice status — planned vs shipped is not blurred in portfolio copy.

## Target roles

- Technical designer (combat / systems)
- Gameplay programmer (C#, gameplay frameworks)
- Systems designer (RTS / tactics)
- AI / gameplay designer (squad command & encounter pacing adjacency)
- Tools / “tech-facing design” hybrid roles
