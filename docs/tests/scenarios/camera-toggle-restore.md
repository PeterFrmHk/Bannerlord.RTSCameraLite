# Scenario: RTS camera toggle — restore native control

## Goal

Verify **commander mode OFF** returns to **native** camera / input expectations without requiring mission restart.

## Setup

1. Any custom battle with commander-capable mission.
2. Default **Backspace** toggle (or your rebound key).

## Steps

1. Commander **ON**; move RTS camera slightly (establish “we are not native”).
2. Toggle commander **OFF** (same key).
3. Confirm **native** camera control returns (mouse / keys per vanilla).
4. Toggle **ON** again; confirm RTS camera re-initializes without error.

## Pass criteria

- No duplicate camera or frozen view (best-effort visual check).
- Logs: no repeated error spam from `CameraBridge` restore path.

## Evidence

- Before/after stills from same map position if possible.
- 20s clip: ON → OFF → ON.
