# Manual test checklist — Slice 14 (native command primitive executor)

Prereq: build with pinned Bannerlord refs; in-game commander mode + valid player formation as needed.

- [ ] Build passes (`dotnet build` on `Bannerlord.RTSCameraLite.csproj`).
- [ ] `NativeOrderPrimitiveExecutor` constructs with `CommanderConfig` (see `CommanderMissionView` / `CommandRouter`).
- [ ] Null `Mission` in context → **`Failure`** / not executed (no crash).
- [ ] Null source `Formation` → **`Failure`** / not executed.
- [ ] `AdvanceOrMove` / `Reform` without **`TargetPosition`** → **`Failure`** (positional orders rejected).
- [ ] `EnableNativeOrderExecution == false` → **`BlockedResult`**; router + cavalry sequence do not issue orders.
- [ ] `EnableNativeOrderExecution == true`, `EnableNativePrimitiveOrderExecution == true`, allows on → advance/charge/hold/reform follow **`OrderController`** path (see slice-14 doc for signatures).
- [ ] “Not wired” reserved path: if ever returned, game does not crash (no throws from executor).
- [ ] Grep / review: no `SetOrder*` / `SetOrderWith*` calls outside `NativeOrderPrimitiveExecutor.cs` under `src/` (legacy `NativeOrderExecutor` stub does not call engine).
- [ ] Debug: success logs throttled (~2s); disabled-execution warning uses **`LogWarningOnce`**.
