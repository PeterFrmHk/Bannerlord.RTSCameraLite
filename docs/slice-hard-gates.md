# Slice hard gates (research before implementation)

These slices touch **fragile engine surfaces**. Implementation work must **not** start until the listed research is done for **your** Bannerlord build (ILSpy / apidoc.bannerlord.com + local binaries, as described in each research doc).

## Slice 5 — Real camera application + restore behavior

**Do not proceed** until:

- [ ] **`docs/research/camera-hooks.md`** has been followed: `MissionView.UpdateOverridenCamera`, `MissionScreen` camera bridge targets, and restore entry points are **verified** on local assemblies matching `docs/version-lock.md` (or your CI reference pack).
- [ ] Any **reflection targets** in `CameraBridge` (or successor) are **named and signed off** against that inspection (no guessing private offsets or method names).

**Artifact:** `docs/research/camera-hooks.md`  
**Audit / tests:** `docs/slice-5-audit.md`, `docs/manual-test-checklist.md` (camera sections as applicable).

---

## Slice 12 — Native order executor

**Do not proceed** until:

- [ ] **`docs/research/native-order-hooks.md`** is filled out from **local** `TaleWorlds.MountAndBlade.dll` / `TaleWorlds.Engine.dll` (or equivalent reference assemblies): `OrderController`, `OrderType`, `Team` order accessors, `WorldPosition`, and any **chosen** issuance path are **confirmed public** for your version.
- [ ] **Harmony requirement** is explicitly decided (default: **not required** if public APIs suffice).

**Artifact:** `docs/research/native-order-hooks.md`  
**Audit / tests:** `docs/slice-12-audit.md`, manual checklist slice 12 when present.

---

## Rationale

Skipping research invites **silent mismatches** across Bannerlord patches (renamed members, behavior changes) and **policy violations** (Harmony when public APIs exist). The hard gate keeps **research artifacts** ahead of **shipping code** for these two slices.
