# Manual test checklist — Slice 25 (compatibility & load-order docs)

Documentation slice: **no gameplay code** required to pass. Verify files exist and links resolve in your editor or git.

## Docs presence

- [ ] `docs/compatibility/README.md` exists and reads coherently.
- [ ] `docs/compatibility/load-order-notes.md` exists.
- [ ] `docs/compatibility/known-conflict-zones.md` exists.
- [ ] `docs/compatibility/dependency-policy.md` exists.
- [ ] `docs/compatibility/mod-stack-testing.md` exists.
- [ ] `docs/compatibility/version-support.md` exists.
- [ ] `docs/slices/slice-25-compatibility-load-order-notes.md` exists.
- [ ] `docs/tests/manual-test-checklist-slice-25.md` (this file) exists.

## Content checks

- [ ] **`version-support.md`** mentions the same **`BannerlordRefsVersion`** pin as `Bannerlord.RTSCameraLite.csproj` (currently **1.2.12.66233** unless updated).
- [ ] **`dependency-policy.md`** states: no MCM / ButterLib / UIExtenderEx **baseline**; Harmony **avoided** unless researched and isolated.
- [ ] **`load-order-notes.md`** lists post-reorder smoke (custom battle, commander toggle, camera).
- [ ] **`known-conflict-zones.md`** covers camera, RTS camera, order/menu, command, troop AI, formation, HUD, optional stacks.
- [ ] **`mod-stack-testing.md`** includes stacks: clean → libraries → camera → command → AI.

## Cross-links

- [ ] `docs/tests/README.md` links to **`docs/compatibility/README.md`** (or otherwise discoverable).
- [ ] `docs/compatibility/README.md` links to slice 25 slice doc.

## Regression (optional, when changing pins later)

- [ ] After editing **`BannerlordRefsVersion`**, update **`version-support.md`** in the same PR.
- [ ] Run `dotnet build` and note result in PR description.
