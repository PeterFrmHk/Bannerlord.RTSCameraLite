# Manual test checklist — Slice 11 (formation eligibility)

- [ ] **Build passes** — `dotnet build -c Release`.
- [ ] **Custom battle loads** with eligibility code paths active.
- [ ] **Eligibility rules initialize** — no null reference from `FormationEligibilityRules` / `DoctrineScoreCalculator` on supported missions.
- [ ] **No-commander formation returns limited allowed set** — mostly **Mob**; **BasicHold** only when `NoCommanderAllowsBasicMobOrders` is true in config.
- [ ] **Commander formation returns expanded allowed set** — more behaviors appear as doctrine/composition improve (debug lines).
- [ ] **Low discipline denies advanced formations** — reduce discipline inputs (e.g. battered troops) and confirm **AdvancedAdaptive** / tight shapes drop from allowed.
- [ ] **Shield-light formation denies ShieldWall** — low shield ratio fails shield-wall rule even with commander.
- [ ] **Cavalry-heavy formation allows MountedWide if threshold passes** — high mounted / cavalry-proxy ratio with commander and sufficient discipline.
- [ ] **Horse-archer-heavy formation allows HorseArcherLoose if threshold passes** — mounted + bow/crossbow mix meets `MinimumHorseArcherRatioForHorseArcherLoose`.
- [ ] **Missing doctrine data returns safe uncertain/failure** — `Evaluate` with null doctrine returns **Uncertain** with minimal allowances; null formation returns **Failure** (no throw).
- [ ] **Debug output is throttled** — eligibility lines appear on the **presence scan interval** (~2.5s), not every frame (`EnableEligibilityDebug` true).
- [ ] **No native orders issued** — code review: eligibility path has no `OrderController` / movement order calls.

## Config

Tune thresholds under `commander_config.json` Slice 11 keys; set `EnableEligibilityDebug` false to silence per-formation logs.
