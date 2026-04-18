# Manual test checklist — Slice 20 (formation / portfolio diagnostics)

- [ ] `dotnet build` succeeds.
- [ ] With `EnableDiagnostics: true`, press **F9** (or configured `DiagnosticsToggleKey`): **`[Diag] ON`** then **`[Diag] OFF`** appears (not every frame).
- [ ] With diagnostics **ON** and commander mode **ON** (when `ShowDiagnosticsInCommanderModeOnly` is true), **`[Diag]`** lines refresh at roughly **`DiagnosticsRefreshIntervalSeconds`** (default 1s), not every frame.
- [ ] With **`ShowDiagnosticsInCommanderModeOnly: true`**, commander mode **OFF** suppresses multi-formation refresh lines (toggle ack may still show).
- [ ] Set **`EnableDiagnostics: false`**: no diagnostics messages; no crash.
- [ ] Invalid / empty formations: no throw; empty snapshot list yields no refresh line.
- [ ] **`Include*`** flags trim segments as expected (compare one line with all true vs toggling individual false in JSON).
- [ ] Mission end / behavior remove: no throw; diagnostics stop after cleanup.
- [ ] Useful for capture: lines show commander presence, discipline, eligibility hints, rally counts, cavalry state, target hint, native gate flags.
