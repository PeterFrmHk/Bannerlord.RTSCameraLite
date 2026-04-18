# RTS Commander Doctrine — manual test documentation (Slice 21)

This folder holds **repeatable manual regression** material for **Bannerlord.RTSCameraLite**: matrices, scenarios, acceptance templates, capture plans, and bug-reporting aids. It does **not** replace automated CI; Bannerlord missions still require human verification.

## How to use

1. **`regression-matrix.md`** — Smoke vs deep pass: mark each cell when a build or release candidate is validated.
2. **`test-scenarios.md`** — Pick scenarios that match your change (e.g. cavalry slice → cavalry-heavy custom battle).
3. **`scenarios/`** — Step-by-step scripts per battle type or edge case.
4. **`slice-acceptance-template.md`** — Copy per slice PR or release note; attach evidence (screenshots, short clips).
5. **`known-failures.md`** — Track deferred issues so regressions are not mis-filed as new.
6. **`bug-report-template.md`** — Paste into GitHub issues / internal tracker.
7. **`gameplay-capture-test-plan.md`** — Portfolio / demo: one scripted ~90s path plus optional per-feature clips.

## Conventions

- **Game build**: note Steam branch or executable version (e.g. 1.2.x).
- **Mod version**: `SubModule.xml` / `InformationalVersion` in `.csproj` when reporting.
- **Config**: tests assume `config/commander_config.json` unless the scenario says otherwise (e.g. invalid-config recovery).

## Related project docs

- Root manual checklist: `docs/manual-test-checklist.md`
- Per-slice checklists: `docs/tests/manual-test-checklist-slice-*.md`
- Slice roadmap / audits: `docs/slice-roadmap.md`, `docs/slices/`, `docs/slice-*-audit.md`
