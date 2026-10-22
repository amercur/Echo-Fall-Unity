# Phase 8 experience audit

Audit: 2026-10-20. Scope: the opening Wake and its existing five-room exploration loop. Reviewed MigrationPlan, Phases 1–7, PlayerPresentation, PresentationPass and FinishedPresentation; historical deferrals are interpreted using the later completion records.

## Evidence and verified baseline

Graphify query traced SliceSession, MemoryFlow, SliceHUD, PlayerPreferences, combat, room loading and archive/checkpoint ownership. Unity CLI/Pipeline provides the live Editor connection in this environment; no separate Unity MCP tool is exposed. `Tools/Phase8Audit.cs` ran in an ephemeral Play Mode session, captured the Wake/choice screen and enumerated actual interaction positions. See `Validation/phase8/baseline-audit.json`, `before-wake.png`, `before-choice.png` and `opening-interactions.txt`.

Verified: initial scene enters Playing directly; warden damage cannot remove the last integrity; a deflect defeats the practice warden; creature dialogue offers mercy/ember/leave. Existing ground, platforms, gates and interactions are present. Prior Phase 7 tests/build evidence establishes a functioning chapter, not proof of first-time comprehension. No new human playtest has been claimed.

Original read-only reference: `C:\Users\sivas\echo-fall`. Its title introduces consciousness, repeated lives and consequence; its first transfer glass explicitly invites a short choice/return loop. Its memory and checkpoint lifetimes remain the reference. Original models and copied sprites establish proportions, but their existence does not make their designs final art.

## Findings and priorities

| Priority | Observation | Experience impact | Bounded response |
|---|---|---|---|
| P0 | No title/arrival context in Unity; immediate controls and objective | No clear mystery or intention for the first steps | Title with continuation, optional field guide and an immediate playable awakening |
| P0 | HUD lists strike/deflect/imprint/mend/memory together | Advanced actions compete with basic locomotion | One contextual teaching card, actual success acknowledgement, revisitable lessons |
| P0 | Safe warden works, but its deflect-only resolution is not explained near approach | Player can mistake low remaining HP for a broken enemy | Name its role, teach strike then white-tell deflect, retain nonfatal damage |
| P0 | Creature option visibly contains mojibake in live capture | Broken typography undermines a pivotal choice | Replace corrupted copy and frame the action without forecasting rewards |
| P1 | Wake objective sends player toward a four-enemy clear after the choice | Memory loop is overshadowed by a completion checklist | Prioritize a short return to listening glass; keep exploration optional |
| P1 | Creature decision does not immediately remove its displayed figure | Taking its fire and leaving it alive appear similar in the current life | Present absence/scar versus a departing creature while preserving decision rules |
| P1 | Returned creature sits near its old place, far from the next spawn | First-loop consequence can be missed | Arrival callback and restrained directional emphasis, then inspect the same place |
| P1 | Repeated arches/spires compete with the small protagonist | Opening lacks a dominant landmark and spatial hierarchy | Subdue distant repetition; define a broken signal dial, practice standard and creature shelter |
| P1 | Low-poly Wanderer/enemies share a mask-and-limbs construction; NPC props are static rendered studies | Silhouettes are readable but character identity and acting remain provisional | Preserve rose scarf/ceramic mask identity, distinguish the warden's oath marking; document bespoke art needs |
| P2 | Generic lore includes migration-era 'later chapter' wording | Implementation history leaks into fiction | Replace opening-loop record copy with fragments tied to hands, voices and water |
| P2 | Modals are legible but uniformly rectangular; typography is a built-in fallback | Title, choices and transfer have little hierarchy | Distinct title layout, split-signal motif, restrained memory accent and action-based extraction captions |
| P2 | Original synthesized audio works and crossfades, but short loops share timbres | Limited character and acoustic identity | Reuse restrained cues at arrival/learning; retain final composition/foley as art direction work |

## Fragile systems

SliceSession is the coupling point for input, screens, scene loads and saves. Title must not resume an unresolved ending/extraction into gameplay. Tutorial knowledge must never become a progression gate or be confused with remembered decisions. Tests/QA use isolated archives; automatic title presentation must be explicitly enabled for their opening tests. Memory effects must rebuild from retained memories, with EMBER taking precedence over MERCY as before. Current-life choice presentation must not grant remembered-world geometry. All new decoration must remain outside collision and interaction discovery must include any narrative landmarks before caching.

## Acceptance boundary

The target is an excellent first 10–15 minutes, not an enforced timer. Automated tests can verify triggers, safe practice, persistence and control dispatch. First-time comprehension, emotional impact, controller comfort and pacing require human playtesting. Custom character concept/acting, a licensed final font and a final audio mix remain art-production needs; procedural ornament is not a substitute for them.
