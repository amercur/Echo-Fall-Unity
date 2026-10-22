# Phase 8: the listening glass

This milestone concentrates on the Wake opening and its existing exploration loop. Read [the experience audit](Phase8-ExperienceAudit.md) and [identity guide](Phase8-IdentityGuide.md) for evidence and direction. No new region, boss, memory type or combat mechanic was introduced.

## Playable sequence

1. **Title:** ECHO//FALL, the premise, a Wanderer witness image, and Wake/Continue, Field Guide and Settings. Continuation respects an existing anchor, pending extraction or saved ending. Title/settings/guide never advance the life. D-pad/A now navigates short menus as well as long ones; numbered choices and mouse still work.
2. **Awakening:** the player can move immediately. “You know how to hold the blade. You cannot remember whose hand taught you.” One contextual card introduces movement and the first ledge. Successful movement, jump and dash are acknowledged briefly; nothing locks until a lesson is completed.
3. **The old promise:** a split ivory/rose standard identifies the practice warden. The nearby lesson first suggests the blade, then explains facing and the white deflect. Existing nonfatal damage and deflect-only defeat are retained. Its release reads as an oath fulfilled.
4. **A small, warm thing:** the creature uncovers its ember. Choices describe giving two integrity or taking the ember, without promising a reward or explaining a later bridge. Leave remains available. The current-life figure departs/disappears; EMBER leaves a warm scar. This changes presentation only, not remembered-world geometry.
5. **The listening glass:** the return objective points to the reachable first ledge. The handprint motif connects the action to the mystery. Transfer remains optional; the original exploration/completion route and R/Back transfer remain available.
6. **One thing crosses:** extraction offers “the life I spared” or “the ember I took,” alongside the existing identity name. It uses the existing atomic pending-decision/transfer save flow and five-memory capacity rules.
7. **The following life:** a short arrival callback points to the old resting place. MERCY returns a creature and a living trace; EMBER leaves an absence and a blackened trace. Interaction gives a restrained recognition/absence passage. The Cistern retains its actual memory-dependent bridge and EMBER conflict.
8. **Explore:** the Belfry teaches wall jumps, the Cistern teaches downward bell strikes, and combat contexts introduce imprint/mend. The Bell Keeper and Engineer records now connect hands, bodies and burdens instead of describing migration deferrals. Existing routes, bosses and later consequences remain available.

The target remains 10–15 minutes for a newcomer exploring the surrounding loop. The first choice/transfer can happen sooner. No artificial wait, tutorial lock, required death or forced moral choice was added.

## Presentation and teaching

- A broken signal dial identifies the first glass. Distant spires/vault strokes are subdued; a low fracture shelters the creature. Existing collision rims, arches and platform positions remain intact.
- Rose/ivory practice markings connect the warden to the Wanderer's scarf; cyan marks recorded influence. Added scenery is renderer-only. It is composition work, not a new finished character art set.
- The title has its own hierarchy and Wanderer image. Gameplay no longer continuously lists five advanced combat inputs; a one-topic card and a field-guide reminder replace that wall of commands.
- Twelve revisitable lesson entries cover move, jump, dash, strike, deflect, anchor, wall jump, pogo, imprint, mend, choice and transfer. Recognition follows successful gameplay events or observed movement, rather than requiring a button press inside a tutorial modal. The full controls remain in pause.
- Learned entries are optional preferences, separate from archive memories and checkpoints. They survive transfers/restarts during normal play. Old settings remain compatible; ephemeral/isolated QA does not write the player's knowledge. All controls work before a lesson is marked.
- The corrupted MERCY option text was replaced. Existing short fades, pose selection, hit feedback, audio palette and camera interpolation are retained. Arrival uses a restrained existing signal cue.
- Contextual teaching sits above the playfield rather than over the protagonist and is hidden during active boss encounters. The field guide remains available from pause.

## Validation

Validation completed on 2026-10-22 using Unity 6000.5.8f1 and the live CLI/Pipeline connection. Reports and captures are stored in `Docs/Validation/phase8/`.

| Check | Result and boundary |
|---|---|
| Full EditMode suite | **66/66 passed**, including optional tutorial-knowledge persistence and older settings compatibility |
| Full PlayMode suite | **62/62 passed**, 232.83 seconds; includes native keyboard/gamepad opening input, practice deflect, pending-choice restart/extraction, MERCY return and EMBER conflict |
| Final focused PlayMode rerun | **4/4 passed**, 9.76 seconds, after the boss-HUD visibility correction |
| Original browser reference | **45/45 passed**; original source unchanged |
| Preservation manifest | **37/37 unchanged** reference entries |
| Opening live review | Title, arrival, field guide, warden lesson, large text, choice, extraction and both return variants captured; archive remains valid |
| First-ledge route | Actual collision motor reached the glass from authored spawn at `(3.62, 0.90)` and opened its interaction; no teleports, geometry edits or tuning changes in this route check |
| Boss HUD | Active King health display remains visible; contextual teaching hides correctly; final live screenshot reviewed |
| Compilation | Final HUD adjustment compiled without errors |

The full suites precede the final one-line boss-HUD visibility adjustment, which was then validated in live Play Mode and followed by a focused opening rerun. Controlled captures use isolated archives and scripted positioning except for the physical first-ledge route. They are not an uninterrupted, uncoached human playthrough. No new Windows standalone was built for Phase 8; earlier Phase 7 build evidence does not certify these new title/tutorial changes outside the Editor.

The baseline audit proved the safe warden before changes. An initial focused keyboard fixture omitted the project's background-input configuration; this was corrected before the full passing run. The interrupted prior full run was rerun from scratch. A boss capture helper initially used the display name `court` instead of the authored room ID `king`; the corrected live check passed. Visual inspection found and corrected teaching-card overlap with the player and the active boss HUD. Initial attempts remain recorded rather than being counted as passing evidence.

Graphify's AST update includes the new opening dependencies. No gameplay movement constants, combat formulas, archive schema, ending conditions or room collision geometry changed. Nothing was committed or pushed.

## Before and after

Before, the Wake began directly in play with a permanent advanced-control list, a poorly explained practice warden, a corrupted creature option and a completion objective that overshadowed the first memory loop. Now, a title and short awakening establish the mystery; contextual practice leads to a legible choice, a return to the listening glass, and a recognizably changed creature site. The existing Cistern bridge supplies the mechanical consequence. Character art remains provisional despite the stronger framing and composition.

## Required custom artwork and human review

The current character sprites are usable model renders and animation studies. This milestone does **not** certify them as finished character design. Production briefs:

- **Wanderer:** approved front/profile silhouette and material sheet; distinct ceramic mask, asymmetric coat and rose scarf; listening/reaching/recognition acting plus corresponding transition poses. Keep the existing 192px frame/pivot contract or update imports and pose tests deliberately.
- **Warden:** weathered oath-bound variant of the Sentinel, with a readable split insignia and a release/bow animation. Preserve the white anticipation silhouette and current hit timing.
- **Creature:** approved curled/standing silhouette, wounded breathing, offered ember, departure, returned greeting and ember-removal poses. Current static model and breathing drift do not convey this full acting range.
- **Opening environment:** one authored broken-signal landmark and a creature shelter/weathering sheet; replace repeated kit forms selectively. No additional collision art ambiguity.
- **Typography/audio:** licensed display/body fonts; close material foley, creature breath, warden release and a longer evolving Wake arrangement. Existing synthesized assets remain interim sound direction.

Ask first-time keyboard and controller players where to go, what the white cue means, what transfers preserve, and what changed after their choice. Measure time without coaching. Check teaching-card placement at 4:3, 16:9 and ultrawide, large text and alternate bindings. Subjective identity, emotional response, audio quality and 10–15 minute pacing need that human review.

## Staged rollout

1. **Opening acceptance:** observe 3–5 uncoached players through both choice branches; resolve confusion and approve the Wanderer/warden/creature concept sheets before expanding art production.
2. **Wake loop:** give Belfry, Cistern, Archive and Procession one authored landmark and one story purpose each. Apply the same contextual lesson pattern, then review backtracking and readable mixed combat.
3. **Cradle and Garden:** carry approved material/silhouette rules into Keeper, Mother and Child. Build interaction acting and memory-specific environmental changes around existing state, with both remembered and forgotten variants reviewed.
4. **Court and Choir:** refine introductions, prediction/recall motifs and ending callbacks using the established visual vocabulary. Preserve ending eligibility and avoid exposition dumps.
5. **Release acceptance:** final audio/typography, complete controller playthroughs, accessibility/aspect-ratio review, standalone soak/profile and save migration verification. Expand only after each stage meets the opening's tested comprehension and readability standard.
