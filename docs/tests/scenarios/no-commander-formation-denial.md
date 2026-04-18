# Scenario: no / weak commander — policy & denial paths

## Goal

Exercise **commander presence** as **missing** or **uncertain** and observe **eligibility / router** outcomes without breaking the mission shell.

## Setup

1. Custom battle where you can field troops **without** a traditional hero-led stack if the game allows (e.g. low-tier custom roster). If the engine always assigns a hero, use **config policy** instead:
   - Tighten `RequireHeroCommanderForAdvancedFormations` / related flags per `commander_config.json` comments.
2. Keep **`EnableDiagnostics: true`** to read **`Cmd NO`** or **`UNK`** when applicable.

## Steps

1. Start battle; commander mode **ON**.
2. Select a formation expected to have **weak or no** commander per policy.
3. Read `[Diag]` **`Cmd`** segment.
4. Attempt an advanced command (if debug keys available) that **requires** commander when policy says so — expect validation **reject** / blocked messaging, **not** a crash.

## Pass criteria

- `Cmd` summary matches design (YES/NO/UNK).
- Router / restriction messages (if surfaced) are throttled and coherent.
- No unhandled exception in log.

## Evidence

- Screenshot: `[Diag]` with `Cmd NO` or `UNK`.
- Note exact config snippet used for the denial test.

## Limitation

Bannerlord custom battle may always give a player hero; this scenario is **best-effort** — document actual roster used.
