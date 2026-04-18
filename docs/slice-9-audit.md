# Slice 9 audit — tactical feedback (no overlay)

## Recommended execution order (depends on)

1. **Slice 6:** Input ownership guard  
2. **Slice 7:** Config file + control profile  
3. **Slice 8:** Formation query + camera focus  
4. **Slice 9:** Lightweight tactical feedback (this slice)  

## Audit logic (design rationale)

- **A:** No full UI / overlay until later.
- **¬A:** The player still needs basic feedback to test tactical interactions in battle.
- **A\*:** Add lightweight, throttled feedback now without committing to a full HUD or Gauntlet architecture.

## Goal

Give lightweight, player-visible feedback for RTS on/off, formation selection, focus attempts, and throttled warnings—using only `InformationManager` + debug text. No Gauntlet, no MCM, no HUD overlay, no markers.

## Components

| Type | Role |
|------|------|
| `TacticalFeedbackService` | `ShowModeEnabled` / `ShowModeDisabled`, `ShowFormationSelected`, `ShowFocusResult`, `ShowWarning`, `ShowDebugLine`; dedupes formation announcements by formation instance between presses; `InvalidateFormationAnnouncement` runs on next/prev so a single formation can still re-announce each cycle; `OnRtsEnabled` / `OnRtsDisabled` / `ResetSession` reset internal state. |
| `FeedbackThrottle` | Wall-clock (`DateTime.UtcNow`) cooldown per string key; `forceImmediate` bypasses cooldown for one-shot events. |
| `ModLogger` | `LogDebug` → `System.Diagnostics.Debug` only; `PlayerMessage` → debug + optional `InformationManager` when UI ready; `Info` / `SafeStartupLog` delegate to `PlayerMessage(..., allowUi: true)`. All paths try/catch so logging never crashes the game. |
| `RTSCameraMissionBehavior` | Owns `TacticalFeedbackService` + `FeedbackThrottle`; replaces ad-hoc `ModLogger` mission strings; publishes formation selection once per formation reference change; camera bridge failures use `ShowWarning` key `camera-bridge` (25s cooldown). |
| `FormationSelectionState` | `TryGetSelectionDetails` supplies list index, unit count, and a simple `group-N` label for feedback. |

## Throttle keys (current)

- `camera-bridge` — failed `UpdateOverridenCamera` apply path (25s).
- `focus-ok` / `focus-fail` — focus feedback (1.25s / 2.5s) to avoid spam while a key is held.
- `battle-end` — end-of-battle release line (`forceImmediate: true` after `ResetSession`).

## Non-goals

- No command routing, no formation markers, no full UI.

## Manual checks

1. `dotnet build` succeeds.
2. Enable RTS: **one** clear ON message; disable RTS: **one** OFF message.
3. Cycle formations: selected message appears (including single-group armies after each next/prev).
4. Failed focus produces a throttled warning (no selection / bad position).
5. Camera bridge warning does not spam every tick (~25s throttle).
6. No crash if UI messaging is unavailable (all `InformationManager` use is guarded; see `ModLogger.PlayerMessage`).
