# Manual test checklist — Slice 19 (command markers)

Prerequisites: commander mode on, camera pose available, `EnableCommandMarkers` / `EnableFallbackTextMarkers` true in `commander_config.json`.

## Checklist

- [ ] `dotnet build -c Release` succeeds
- [ ] Mission loads without exceptions from marker code
- [ ] `CommandMarkerService` initializes with mission (`OnBehaviorInitialize`)
- [ ] Marker lifetimes expire (no unbounded marker list growth; `Tick` removes expired entries)
- [ ] With particle name **unset** (default): fallback text appears for markers (throttled `[Marker]` lines)
- [ ] Ground-style marker path: commander mode + camera — throttled ground sample shows fallback or future particle when wired
- [ ] Move target: debug **M** (`AdvanceOrMove`) after validation — fallback or visual at move position
- [ ] Charge target: debug **N** starts native cavalry sequence — fallback or visual at enemy formation center (when sequence starts)
- [ ] Reform point: during native cavalry sequence when reform becomes allowed — fallback or visual near player formation center
- [ ] Marker / visual failure does not block commands or crash (swallowed exceptions, router unchanged)
- [ ] Mission end / behavior remove calls `Cleanup` (no stale markers next load)

## Notes

- Optional particle name lives as `OptionalMarkerParticleName` constant in `CommandMarkerService` (null until Slice 0 verifies a safe effect).
