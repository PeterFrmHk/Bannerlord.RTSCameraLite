# Manual test checklist — Slice 15 (command router + restrictions)

Use a **Release** build against your pinned Bannerlord version.

- [ ] Build passes (`dotnet build -c Release`).
- [ ] Custom battle loads with the module enabled.
- [ ] Commander Mode is active (default or toggled on).
- [ ] **H** triggers `BasicHold` validation (see debug log when `EnableCommandValidationDebug` is true).
- [ ] **C** triggers `Charge` validation.
- [ ] **M** triggers `AdvanceOrMove` validation when a placeholder target can be built (formation center + camera pose when available).
- [ ] Commander Mode **off** blocks validation (`Blocked` / commander mode message).
- [ ] No commander blocks **advanced** formation commands when `BlockAdvancedCommandsWithoutCommander` is true (e.g. shield wall path).
- [ ] Invalid / empty formation is handled without crash.
- [ ] Missing target position blocks positional commands (`AdvanceOrMove` without center fails safely).
- [ ] `NativeCavalryChargeSequence` intent is denied without cavalry-heavy context + eligibility (manual test via future UI or temporary code if needed).
- [ ] `EnableNativePrimitiveOrderExecution` false prevents real native execution (executor may still return not-wired).
- [ ] Debug logs are throttled (`CommandValidationDebugLogIntervalSeconds`).
