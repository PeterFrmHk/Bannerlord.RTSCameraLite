# Media — portfolio evidence

This directory holds **binary evidence** (video, images) for the RTS Commander Doctrine portfolio. It is intentionally **not** required for the mod to run.

## Naming conventions

Use **ISO date prefixes** so folders sort chronologically and recruiters can see freshness:

| Kind | Pattern | Example |
|------|---------|---------|
| Clips | `clips/YYYY-MM-DD_feature-name.mp4` | `clips/2026-04-18_rts-commander-90s-demo.mp4` |
| Screenshots | `screenshots/YYYY-MM-DD_feature-name.png` | `screenshots/2026-04-18_command-router-validation.png` |
| Diagrams | `diagrams/system-name.png` | `diagrams/doctrine-pipeline.png` |

**Rules of thumb**

- Use **lowercase kebab-case** for `feature-name` and `system-name`.
- Prefer **lossless PNG** for UI and log screenshots; **H.264/H.265 MP4** for clips unless your pipeline requires another container.
- Keep clips **short** (15–120s) unless you are archiving a full session elsewhere.

## Subfolders

- [`clips/`](clips/README.md) — vertical or horizontal reel segments.
- [`screenshots/`](screenshots/README.md) — stills referenced from `docs/portfolio/feature-proof-checklist.md`.
- [`diagrams/`](diagrams/README.md) — architecture or flow images exported from whiteboard tools.

## Git LFS

If clips grow large, consider [Git LFS](https://git-lfs.com/) or host videos externally and link from portfolio docs.
