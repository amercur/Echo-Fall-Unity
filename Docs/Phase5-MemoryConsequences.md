# Phase 5: remembered consequences

Phase 4 remains the world baseline: thirteen rooms, thirty-six directed gates, five anchors, the original Wake completion loop, King, combat, map discovery and mechanical shortcuts. Phase 5 adds archive-driven variants and decisions without rewriting those scenes or changing the baseline movement tuning. The original project at `C:\Users\sivas\echo-fall` is read-only.

## Dependencies and source comparison

Graphify queries traced archive, session, transfer, checkpoint, room application, map and combat dependencies before implementation. Source inspection confirmed that `SliceSession` owns this-life state, `SliceArchive` owns remembered history, `SliceRoom.Apply` rebuilds consequences, and checkpoint validation must recognize new enemy/NPC IDs. Map and interaction locks share the memory route predicates. The graph is refreshed after implementation; serialized room relationships still require source verification.

Original references: `game.js` memory definitions, NPC interactions, extraction, abilities, enemy update and delayed creature assistance; `data/world.json` for NPC and route placement. Preserve the distinction between a decision this life, a retained memory, and an equipped ability. This implementation uses the original nine identifiers and five-slot limit. Final endings, Mother and her summoned Echo roster remain deferred.

## Decisions and consequences

| Memory | Decision | Remembered consequence | Equipped ability |
|---|---|---|---|
| RETURN | Transfer without a new decision | Familiar baseline identity | Extended deflect; existing deflect mend |
| MERCY | Give the wounded creature two integrity | Returned creature and living-root paths; after three transfers since acquisition, assistance against the King | Spirit strike |
| EMBER | Take the creature's fire | Scar replaces the creature; living-root paths close | Firebolts and fire burst |
| DEFIANCE | Open the mysterious door | Extra Archive step; early access through the breathing passage unless ORDER is retained | Extra air jump and collision-swept blink |
| ORDER | Seal the door | Cistern bridge; King has 48 rather than 40 integrity; suppresses the memory-only breathing route | Two-second ward |
| SACRIFICE | Give the Keeper two integrity | Empty, open cage silhouettes; Keeper offers restoration in later lives | Mend two integrity once per room per life |
| BETRAYAL | Strike the Keeper | Keeper refuses help; a remembered self guards the Archive lift | Fury and one integrity restored on a kill |
| KINSHIP | Listen to a previous self, or complete the Child's dialogue | Alternate recognition; the Archive Echo refuses combat and releases the lift | Remembered voice strike |
| DISTANCE | Walk away from a previous self; refuse the grateful Keeper's help | Alternate dialogue; after two transfers the Lungs voice leaves an empty place, unless KINSHIP is remembered | Shorter dash recovery and brief phase protection |

Immediate decisions are recorded before paying integrity, including fatal gifts. Creature and Keeper choices affect this-life attack power where applicable. The Child's existing three responses still open the Choir and now record KINSHIP. NPC decisions are consumed once per life and included in checkpoint saves.

MERCY uses acquisition-relative age, not total lifetime count. Re-extracting an existing memory does not reset its age. EMBER takes precedence over MERCY's living-world consequences, including assistance, preserving the Unity slice's conflict policy. Forgetting and later reacquiring a memory starts its age again. KINSHIP extraction count remains historical, as in the original; it does not secretly reactivate a forgotten consequence.

## Transfer, forgetting and persistence

The transfer screen offers every distinct decision made in the life; exactly one crosses. At five memories, adding a sixth asks which to release, then confirms the replacement. Replacement and extraction are written together before the new life begins. Failed writes keep the previous archive and offer retry. Re-extracting an existing memory requires no replacement.

At the Wake sanctuary or an anchor, Pause > Memories equips retained abilities and shows the active identity and a short ability description. All retained memories influence the world regardless of equipment. The menu supports up to nine choices using mouse, number keys, or D-pad and A. Choosing Forget requires confirmation: this implementation explicitly releases the current life/checkpoint and rebuilds at the Wake, at the same loop number. This differs from the original between-lives-only forgetting UI and prevents stale geometry or enemies surviving the removal.

The existing archive filename/schema remains backward compatible through optional fields. Old saves without acquisition dates begin aging at their loaded loop; no delayed consequence is granted based on an invented date. Pending extraction candidates are saved when a life ends, so application/session restart resumes the selection. Checkpoints now preserve all current decisions, NPC consumption, per-room SACRIFICE use, the guardian's health/defeat and lift release, alongside the existing loadout, world flags, boss and discovery state. A transfer or confirmed forget discards the checkpoint and rebuilds this-life state. Discovery retains Phase 4's per-life semantics.

## World and Echo implementation

`MemoryWorld` creates additive variants under each room: ORDER bridge, DEFIANCE step, Keeper cage silhouettes and the conditional Archive Echo. Saved scenes, base colliders and existing shortcuts are preserved. DEFIANCE offers an early breathing passage, but opening the Phase 4 far-side latch still works with ORDER; it cannot strand a player who previously used that shortcut. The Archive lift is blocked by remembered BETRAYAL until its guardian is defeated or refuses under KINSHIP; its map line uses the same condition.

The Archive guardian is an adaptation combining the original betrayal guardian with previous-self behavior. Its identity is the last extracted memory, not a full recording of the player's run. It recognizes the player before attacking; EMBER casts a projectile, DEFIANCE advances, and MERCY/SACRIFICE restore its own health. KINSHIP permits a nonviolent resolution. It uses the existing damage, imprint, deflect, knockback, health caching and defeat pipeline. Forgetting its recorded identity removes that future identity too. It is room-bound and never follows the player as a companion.

The King retains his existing patterns and phases. ORDER changes maximum health and the health display. Mature MERCY strikes after two seconds in combat, then every four seconds, for 2.5 damage. Presentation uses restrained rings, existing sound hooks and the current Wanderer assets; no animation owns input timing.

## Validation

Validation evidence is recorded in `Docs/Validation/phase5/`.

- **51/51 EditMode tests passed**, including six new archive/capacity/age/forgetting/migration/checkpoint tests. Existing deterministic MovementLab trajectories and authored traversal tests remain green.
- **46/46 PlayMode tests passed in the final full rerun**, including the existing 37 tests plus nine focused Phase 5 tests: pending extraction after fresh-session load, multiple decisions, confirmed capacity replacement, failed-write rollback, forgetting/rebuilding, NPC/Echo reactions, delayed King aid/ORDER health, room mend limits, blink/air-jump collision and checkpoint restoration. This rerun includes the final callback-based menu input and corrected Wanderer Echo pose mapping.
- **45/45 original JavaScript tests passed**, running the source tests read-only.
- **36 preservation comparisons passed**: recorded source files, all authored scene files, movement tuning/input, player presentation and projectile code match the Phase 4 baseline.
- Live visual review advanced from frame **15,420 to 16,267**, using an ephemeral archive. Inspected the six-option loadout, eight-option extraction, Keeper dialogue, ORDER Cistern bridge, Archive Echo and open Cradle cages. Screenshots and `live-review.json` are retained.
- Native-input regression now tests all nine number keys and controller D-pad/A selection. This caught and fixed a missed quick key press by replacing frame polling with input callbacks. The focused rerun passed.
- The older fatal-death restart test now verifies saved extraction before entering the new life. Its previous expectation of immediate active play was incompatible with the new feature; checkpoint invalidation remains asserted.
- Compilation completed without C# errors. `git diff --check` passed. Graphify's final AST refresh reported **1,753 nodes / 3,305 edges / 93 communities**. Semantic document labels can still describe older milestones; serialized-scene coverage remains incomplete.

Previews: [memory menu](Validation/phase5/five-memory-menu.png), [extraction](Validation/phase5/extraction-choices.png), [Keeper](Validation/phase5/keeper-remembers.png), [ORDER bridge](Validation/phase5/order-cistern.png), [Archive Echo](Validation/phase5/betrayal-echo.png), [Cradle cages](Validation/phase5/sacrifice-cradle.png).

## Remaining gaps and Phase 6

- Mother-specific SACRIFICE effects and her summoned Echo identities await the Mother encounter. The Archive adaptation does not claim full summoned-Echo combat parity.
- Dynamic last-seven body placement from the original is deferred; existing authored previous-self sites carry the memory conversations.
- Dedicated Keeper/creature/Echo artwork, bespoke voice/audio and richer cage art remain presentation work; current assets and procedural silhouettes are used.
- The nine original memory types cover the present decisions. No additional destroy-object or follow-NPC memory taxonomy is invented, and final endings are not implemented.
- Extended human keyboard/controller playthroughs, balancing, localization and standalone performance testing remain release work.

Recommended Phase 6: the Mother encounter and its memory-dependent phases/Echoes, deeper Child/Choir progression, dynamic previous-self remnants, and a controlled narrative playtest across several archive configurations. Keep final endings as a separately reviewed milestone after those consequences are stable.
