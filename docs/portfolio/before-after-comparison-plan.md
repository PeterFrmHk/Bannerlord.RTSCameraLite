# Before / after comparison plan

Use this table in reels, case studies, and interviews. Wording is **intentionally cautious**: native Bannerlord already does formation combat well; the mod adds a **commander doctrine layer** and **evidence-friendly** tooling.

| Topic | Native Bannerlord behavior | Modded RTS Commander Doctrine behavior | Why it matters |
|-------|------------------------------|------------------------------------------|----------------|
| Camera & orders | Player uses native formation UI and battle camera flows. | Optional RTS-style commander camera shell with **explicit** commander mode gating; native paths preserved where required. | Reduces “magic mod” risk — behavior is opt-in and documented. |
| Commander authority | Captain/hero presence is implicit in gameplay. | **Presence**, **anchor**, and **eligibility** are modeled as data with debug visibility. | Shows systems thinking: policy before orders. |
| Doctrine vs execution | Orders issue through native UI / shortcuts. | Doctrine computes **plans and states** on throttled scans; native issuance is **routed** and can remain disabled until research gates pass. | Demonstrates adapter discipline and production safety. |
| Cavalry flow | Player charges with native commands. | Doctrine describes spacing, lock release, and reform distance; **native sequence orchestration** can layer on when executor is verified. | Shows combat UX design plus engineering boundaries. |
| Feedback | Battle log and UI feedback are engine defaults. | **Throttled** player-visible messages and **temporary markers** (text fallback by default). | Portfolio-visible “player empathy” without claiming a full HUD replacement. |
| Evidence | Hard to show design intent in a reel. | Slice docs + diagnostics + checklist-driven captures under `media/`. | Hiring managers see **traceability** from intent to code. |

## Capture tips

- **Before:** Record 5–10s of vanilla UI in the same battle type (custom battle).
- **After:** Same scenario with mod enabled and commander mode on; match camera angle where possible.
- **Narration:** Name one constraint per beat (e.g. “no Gauntlet overlay by design,” “native orders behind one executor”).
