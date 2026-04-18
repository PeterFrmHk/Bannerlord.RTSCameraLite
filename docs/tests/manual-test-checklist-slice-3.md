# Manual test checklist — Slice 3 (adapter boundaries)

**Mod:** `Bannerlord.RTSCameraLite`  
**Goal:** Verify Slice 2 behavior unchanged, adapters compile, no log spam, no real camera/orders.

---

## Prerequisites

- [ ] Build `Release` (`dotnet build -c Release`).
- [ ] Deploy module to the game `Modules` folder.

---

## A. Regression — commander mode (Slice 2)

- [ ] Custom battle: **RTS Commander Mode** still starts **active** (toast + debug).
- [ ] **Backspace** toggles **off**, then **on** again.
- [ ] Rapid Backspace / F10 spam does **not** crash or freeze.

---

## B. Adapter logging (Slice 3)

- [ ] On first mission tick with commander mode **on**, **at most one** debug line appears for **`CameraBridge` probe** (NotWired message).
- [ ] **No** per-frame flood of CameraBridge messages while standing in deployment.
- [ ] Leaving the mission logs **cleanup** (and optionally one restore NotWired line).

---

## C. No real camera / orders (Slice 3)

- [ ] Camera does **not** move to a new RTS rig (bridge not wired).
- [ ] No troop orders fire from this mod (executor not wired).

---

## D. Optional — code inspection

- [ ] `NativeOrderPrimitiveExecutor` methods exist but only return **`NotWired`**.
- [ ] `BackspaceConflictGuard.ShouldSuppressNativeBackspace()` returns **`false`**.

---

## Acceptance summary

| Criterion | Pass |
| --- | --- |
| Build passes | [ ] |
| Adapters compile | [ ] |
| No-op / NotWired results are safe | [ ] |
| Commander mode still toggles | [ ] |
| No real camera/order behavior required | [ ] |
| No repeated warning spam | [ ] |
