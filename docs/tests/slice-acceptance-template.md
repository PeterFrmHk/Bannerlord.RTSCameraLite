# Slice acceptance — template

**Copy this file** per slice or per release candidate. Name: `acceptance-slice-NN-<short-title>-YYYY-MM-DD.md` (or attach section in PR).

## Metadata

| Field | Value |
| --- | --- |
| **Slice name / ID** | |
| **Branch / commit** | |
| **Mod version** (e.g. csproj `InformationalVersion` / git tag) | |
| **Game version** (Bannerlord build) | |
| **Tester** | |
| **Date** | |

## Build status

| Check | Pass / Fail | Notes |
| --- | --- | --- |
| `dotnet build` (Win64_Shipping_Client output) | | |
| No new compiler warnings (or listed waivers) | | |

## Manual tests

List procedures run (link `docs/tests/scenarios/…` and `manual-test-checklist-slice-*.md` as applicable).

| Test | Pass / Fail | Evidence link (screenshot / clip) |
| --- | --- | --- |
| | | |

## Expected behavior

Summarize what **should** happen for this slice (single paragraph + bullet list).

## Actual behavior

What **did** happen. Call out deltas vs expected.

## Screenshots / video captured

| Artifact | Path or URL | What it proves |
| --- | --- | --- |
| | | |

## Bugs found

| Bug ID / issue # | Severity | Linked `known-failures.md` ID (if any) |
| --- | --- | --- |
| | | |

## Pass / fail decision

- [ ] **PASS** — acceptable for merge / ship with no blocking bugs.
- [ ] **PASS with notes** — non-blocking issues filed; workarounds documented.
- [ ] **FAIL** — blocking; do not merge until resolved.

**Decision:** PASS / PASS with notes / FAIL  

**Sign-off:**  
