# Manual test checklist — Slice 10 (formation doctrine profile)

Use a **Release** build against your pinned Bannerlord version.

- [ ] Build passes (`dotnet build -c Release`).
- [ ] Custom battle loads with the module enabled.
- [ ] `DoctrineScoreCalculator` initializes (no throw in `CommanderMissionView.OnBehaviorInitialize`).
- [ ] `FormationCompositionAnalyzer` runs without exceptions on mixed formations.
- [ ] Equipment classifier handles missing / unreadable equipment (roles may be `Unknown`).
- [ ] Rank classifier handles missing character data (fallback tier without crashing).
- [ ] Doctrine profile is produced for at least one friendly formation with units (`DoctrineScoreResult.ComputationSucceeded`).
- [ ] All doctrine scores stay within **0..1** (including discipline and casualty shock).
- [ ] Empty or invalid formation returns safely (no crash; composition / doctrine fallbacks).
- [ ] Debug summary does not spam (interval respects `DoctrineScanIntervalSeconds`, typically 3s).
- [ ] No formation restrictions added in this slice.
- [ ] No native orders issued from doctrine paths.
