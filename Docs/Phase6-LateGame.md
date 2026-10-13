# Phase 6: the voices converge

Phase 5 is the preserved baseline. Start from `WakeSlice`: its five-room objective and voluntary transfer remain available. The current chapter now also supports King, Mother, Child and the four Choir endings. The original JavaScript project is read-only.

## Progression and source adaptations

Graphify inspection traced boss/health dependencies through `SliceEnemy`, `SliceCombat`, projectiles, room loading, checkpoints and HUD, and traced memory/NPC/ending state through the archive and session. Source review used original `game.js` boss update, recalled identity skills, Keeper interaction, Child/Choir dialogue, ending/extraction functions, `data/world.json`, the art manifest and reference tests. The main risks were King-specific targeting assumptions, transient Echoes entering save snapshots, modal escape bypassing an ending, and conflating archived history with this-life flags.

The main route remains Wake → Procession → Court → Cradle → Mother → Garden → Choir, with the existing Archive/Lungs, Observatory return, Root Vault and mechanical shortcuts intact. Mother victory opens `garden-path`; Phase 4's upper maintenance release still works. The player can reach later regions early and revisit either sovereign in a different order. The Choir's final decision requires **King defeat, Mother defeat and Child permission in this life**. This explicit convergence check adapts the original's reliance on progression gates to Unity's preserved alternate routes; a shortcut cannot accidentally skip the chapter's major resolutions.

Mother's scene gains her source atlas and encounter. The existing Sentinel/Drone pair moves into the eastern vestibule, outside the arena, to leave room for recalls. Base collision, traversal platforms, named spawns, room connections, and other scenes are preserved.

## Encounters

### King

The existing prediction fight is retained: ordinary grounded attacks are predicted, while aerial attacks, dash follow-ups, charged attacks, deflect exposure and imprint/strain break the rhythm. Phase two begins at half health or three prediction breaks. ORDER retains the 48-integrity variant; mature MERCY retains its delayed assistance. Shared boss identification replaces King-only combat assumptions so those mechanics also reach Mother without altering his patterns.

### Mother

Original eight-frame `mother.png`, foot pivot and .65 visual scale are imported from a copy. The chamber's floor encounter starts inside the 3.5–12-unit arena; upper/outer approaches remain available until entry. Arena seals and the motor bounds hold the encounter until defeat or transfer.

- 48 integrity normally; 38 if SACRIFICE was remembered or decided before creation. A new Keeper gift after Mother has been cached reduces her remaining integrity by five and uses that value as her new maximum, matching the original interaction-time consequence.
- .9-second anticipation marks a falling signal's target. White patterns fire an aimed three-shot spread and a falling bolt. Every third pattern fires red ground shots in both directions plus a red falling shot. Red signals require evasion or a charged counter; reflected signals damage Mother.
- Half health begins a 1.4-second phase transition, then reduces recovery from 2.25 to 1.8 seconds and recalls an Echo after every pattern rather than every second pattern. At most three living recalled Echoes are present.
- Recall cycles through retained identities, using RETURN if the archive is empty. BETRAYAL increases recalled integrity from six to nine. Recalled selves pause to recognize the player before attacking.
- MERCY/SACRIFICE identities restore Mother's integrity; EMBER fires bolts; DEFIANCE blinks; ORDER wards; RETURN can parry light hits; BETRAYAL heals on damaging contact; DISTANCE lunges faster; KINSHIP refuses and grants Resonance. These are remembered identities, not recordings of full runs or following companions.
- Mature MERCY can assist against Mother on the existing acquisition-relative schedule. EMBER's Phase 5 conflict policy still suppresses that living-world assistance.
- Victory clears projectiles, imprint and recalls, heals two integrity once, records defeat, releases the arena and opens the Garden causeway. Her phase, recall index, attack sequence, maximum and remaining integrity use the shared run/checkpoint model; live projectiles and summoned actors do not enter save data.

### Child

The Child remains an interaction without a damageable enemy or health bar. The original three correct answers grant permission and record KINSHIP. Wrong answers leave the current stage available to reconsider. Retained KINSHIP changes recognition; BETRAYAL changes the opening response. Current-life permission is saved at anchors and resets on transfer. Answering correctly at the final encounter does **not** replace the prior remembered relationship required for SHARE.

## Ending conditions — internal reference

These requirements are developer documentation, not a checklist shown to the player.

| Ending | Eligibility | Outcome |
|---|---|---|
| RESET — The kindest prison | In Choir; King and Mother defeated; Child granted permission | The world wakes without fear; the Child returns to the garden |
| BREAK — An unwritten morning | Same resolved progression | The Choir falls silent; the future escapes prediction |
| REMEMBER — The weight returns | Same resolved progression | The creator accepts the accumulated history instead of forgetting |
| SHARE — No single voice | Same progression **plus retained KINSHIP, retained MERCY or SACRIFICE, and at least one prior KINSHIP extraction** | Other voices participate in the final choice |

All options require confirmation. Source-compatible SHARE eligibility deliberately permits contradictory memories: BETRAYAL does not silently disappear or become forgiven. The Keeper's refusal persists in the epilogue; SACRIFICE can instead leave his cages open. EMBER leaves an absence; MERCY recalls the creature. Optional Garden/Vault discoveries add a restrained callback. Those details are snapshotted with the ending, so later forgetting cannot rewrite what happened in an already discovered outcome.

The three standard endings share the original progression requirement rather than inventing memory locks absent from the source. History controls how those outcomes are described and whether the additional shared answer exists. A final choice does not erase the archive or override prior NPC treatment.

## Persistence and replay

The archive stores unique ending IDs, the current ending screen and a bounded record for each discovered ending: loop, memories, decisions, progression flags, optional discoveries and epilogue. Reaching an ending atomically clears the checkpoint and saves that record together with the pending memory candidates. Failed writes preserve the previous archive and offer retry.

Reload resumes the saved ending. Carry a Memory Forward enters the existing extraction flow; the selected memory begins another Wake life. Discoveries remain while current-life boss defeats, Child permission, map discovery, NPC consumption and shortcuts reset as before. Repeating an ending updates its latest record without duplicating its discovery. Pause > Memories > Endings Recovered allows reviewing previous outcomes. Ending and credits screens cannot be dismissed into an invalid active life by Escape/Start.

Optional archive/checkpoint fields preserve Phase 5 save compatibility. Unknown ending IDs, malformed records and invalid Mother metadata are rejected through the existing backup/fallback path. The original archive file and atomic replacement strategy remain in use.

## Validation

Evidence is stored in `Docs/Validation/phase6/`:

- **58/58 EditMode tests passed**, including ending convergence, SHARE's remembered care and prior extraction, contradictory histories, forgetting, outcome persistence and checkpoint validation.
- **52/52 PlayMode tests passed**, including the full previous regression suite. New coverage exercises Mother's white/red patterns, phase transition, recall, charged reflection, memory assistance, death/transfer, checkpoint restoration, one-time victory handling, Child dialogue, ending reload and subsequent-loop replay.
- **45/45 original JavaScript reference tests passed**, run without changing the source project.
- **37 preservation comparisons passed**: original source/reference hashes, existing scene preservation, Mother's base collision, unchanged movement/presentation scripts, and exact copied Mother artwork.
- `Tools/Phase6Review.cs` completed an isolated live-editor review over 1,018 rendered frames with a valid archive and all four ending discoveries. It invoked King/Mother defeat and Child responses, inspected SHARE availability, credits and extraction, and staged the remaining three endings for presentation. This is controlled verification, not a hands-on full-difficulty playthrough. Captures cover Mother's anticipation, recall and defeat, Child history, Choir choices, all endings and credits.
- Unity was returned to Edit Mode after review. No player archive was written by the presentation review (`EphemeralSave`).

The full PlayMode suite retains room-transition, traversal, shortcuts, map, memory, save recovery, combat and checkpoint regression coverage. No standalone build or physical-controller run was performed for this milestone.

## Remaining work and Phase 7

Dynamic placement of the original last-seven abandoned bodies is still deferred; the authored previous-self sites and Mother's recalls carry the present Echo narrative. Combat style unlocks/forms remain a separate parity task. No new world regions or additional endings are invented.

Dedicated Keeper/Child artwork, bespoke directional player animations, authored audio/mixing, localization, settings/rebinding/accessibility, first-time physical-controller playtesting, difficulty tuning and a standalone build/performance pass remain release work. Automated encounters and staged screenshots do not establish subjective difficulty or physical input latency.

Recommended Phase 7: release-quality validation of the complete chapter—full keyboard/controller runs through different archives, Mother/recall balance and readability, save migration/recovery in a standalone build, audio/settings/accessibility, and the remaining body-remnant/style parity. Keep the thirteen-room world stable while that work is measured.
