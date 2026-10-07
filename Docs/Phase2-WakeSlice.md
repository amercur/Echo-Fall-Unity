# Phase 2: the first Wake slice

Implementation date: 2026-10-05. This milestone follows the user's expanded Phase 2 scope: combat, five connected rooms, presentation, and the first memory loop. It brings forward selected work from the original plan's phases 3, 4, and 6; it does not claim those entire phases are complete.

## Play

Phase 3 continuation (2026-10-07): this document remains the Phase 2 baseline/historical acceptance record. [Phase3-CombatCourt.md](Phase3-CombatCourt.md) describes the combat upgrade and new Court branch from Procession. The original five-room completion, memory/transfer behavior, saved anchors, player presentation and exploration remain supported; the sealed King passage is now open. Combat parity gaps mentioned below are superseded where explicitly completed in the Phase 3 notes.

Open `Assets/EchoFall/Scenes/WakeSlice.unity` and press Play. The bootstrap loads `Wake_wake` additively. The original `MovementLab`, its player prefab, and its movement tuning are preserved. `WakeSlice` is first in Build Settings; all five destinations and MovementLab are included.

Keyboard: A/D move; Space jump; K/Shift dash; J/X strike (hold then release for charged cut); W/S aim; F tap for white deflect, hold at least .48 seconds and release to counter red; Q imprint/early detonation; H hold while still to mend; C/L active memory; E interact; R voluntary transfer; Escape controls/pause. Dialogue uses buttons or 1/2/3. Controller movement and actions are bound; dialogue choices also use A/X/B. Physical controller testing remains necessary.

### Suggested first run

1. Rest at the Wake's signal anchor and remain still for .45 seconds to save. Movement, combat, damage, or opening a menu interrupts an unfinished rest. Climb the introductory steps and practice deflecting the warden. It cannot take your last integrity and is defeated by a clean deflect.
2. Speak to the wounded creature. MERCY costs two integrity, including a potentially fatal last gift. EMBER grants one extra blade damage for this life.
3. Clear all four enemies in the Pilgrim Causeway. The sentinel uses white attacks, the lancer telegraphs red attacks, and the drone fires reflectable bolts.
4. Climb the Belfry and find the Bell Keeper's record. Reach the Glass Archive and open its lift from the far side.
5. Explore the Cistern, retrieve the Drowned Engineer's record, and open the sluice. Use jump/dash and the pogo bells; water damages and returns you to safe footing.
6. Visit all five rooms, then return to either transfer glass (Wake's first raised dais or Archive's upper dais). A completed route receives the completion ending. Earlier voluntary transfer and fatal damage still enter extraction, preserving the failure path.
7. Select the one offered decision. The next life begins in the Wake and resets enemies, collectibles, shortcuts, and visited rooms.

This route is designed for a first-time 10–15 minute exploration/tutorial session. That duration is a pacing target, not a measured human playthrough. An experienced player can complete the small source layouts much faster. No artificial wait timers were added to force a duration.

## Rooms and source fidelity

| Room | Purpose | Added slice presentation/content |
| --- | --- | --- |
| Wake / Signal Well | Introductory traversal, safe practice, creature choice | Signal anchor, transfer dais, remembered creature or ember scar |
| Pilgrim Causeway / Procession | Combat | Four source encounters, faded standards, route-clear objective |
| Belfry / Broken Ascent | Vertical traversal and secret | Original wall-jump shaft, suspended broken bell, Bell Keeper record |
| Cistern / Hollow Roots | Hazard exploration, pogo, memory shortcut | Dark water, pogo bells, Drowned Engineer record, actual MERCY bridge collision |
| Glass Archive / Unfiled Lives | Narrative reflection and completion | Glass-life shelves, original lift lever, upper transfer glass |

`Tools/import_wake_slice.py` reads the original `data/world.json` and the overriding exported geometry in `assets/art-manifest.js`. It copies six PNG assets and writes a normalized fixture consumed by `WakeSliceBuilder`. Original files are never written and source generators are never run. Source hashes are recorded in `Validation/phase2-source-hashes.json`.

Original room bounds, ground, platforms, walls, hazards, spawns, normal enemy placement, and in-slice connections are used. The original gate model remains interaction-driven. The completion objective, root bridge, and repositioned transfer glass are deliberate slice adaptations. Gates beyond the five rooms are sealed lore interactions. Additional forms, keeper/Echo choices, and the DEFIANCE door are not exposed as functional content.

## Systems and presentation

- Separate session, combat, enemy, projectile, interaction, archive, room, HUD, presentation, sound, and parallax components. The scene builder is Editor-only.
- One additive room scene at a time with persistent bootstrap player/camera/UI. Enemy health/defeats and shortcut flags survive backtracking within a life. Room transitions set both spawn and safe recovery point.
- Three-hit blade chain, immediate press attack plus held-release charged cut, upward/downward attacks, recovery input buffer, dash recovery cancel, white deflect, charged red counter, fracture debt, Resonance, imprint, stationary mend, and pogo. Combat uses readable original timing/value baselines; complete combat parity is not yet certified.
- Modal dialogue, pause, completion/failure extraction, objectives, interaction prompts, six integrity pips, Resonance, life/identity display, and a compact static room graph. The graph is not full map fog/navigation.
- Original Wanderer and bestiary atlases, preserved pixel scale and corrected foot pivots, attack/guard animation, telegraph/recovery enemy frames, blade arcs, signal pulses, firebolts, and memory-strike feedback.
- Original arch/spire/ledge assets; separate collision and decorative hierarchy; layered cathedral silhouettes, vault ribs, parallax, masonry/cornices, foreground piers, glass shelves, bell, banners, candles, dust and window-light bands.
- Sprite-Lit materials, global and local URP 2D lights, and 2D shadow casters. Procedural low-volume ambience and a four-voice pool of bell/blade/impact cues follow the source's synthesis approach. Full AudioMixer settings and authored score are deferred.

## Memory consequence and persistence

Current-life decision and persistent archive are separate. The offered extraction is exactly one of `mercy`, `fire` (displayed as EMBER), or fallback `return`. Transfer advances the life counter and equips that memory. Repeated extractions do not duplicate archive entries.

- **MERCY:** the creature returns, and a visible living-root bridge plus the Cistern–Archive route become available on the next life. Active MERCY provides a nearby spirit strike.
- **EMBER:** the creature is replaced by a lit charred scar, ordinary non-downward strikes become firebolts, and the remembered root bridge/route close. EMBER overrides MERCY if both are present in a seeded archive.
- **RETURN:** a no-choice transfer equips the longer white-deflect window.

The three supported memory IDs are validated. A versioned JSON archive lives at `Application.persistentDataPath/wake-slice-v1.json`. Writes use a temporary file and atomic replacement with backup; corrupt primary data can recover the backup. Read recovery does not overwrite a corrupt original. A failed save leaves the transfer screen available to retry. Tests use ephemeral archives and isolated temporary save files, not the user's real archive.

The Wake and Archive signal anchors now save a completed bench snapshot. Rest for .45 seconds without moving, attacking, guarding, mending, taking damage, or opening a menu. A completed rest restores six integrity, clears fracture and transient combat state, grants at least one Resonance, and atomically saves the current decision, visited rooms, enemy health/defeats, collectibles, and shortcuts across all five rooms. Progress after that rest is not saved until another completed rest. Save failures are reported and can be retried by resting again.

Restart automatically resumes a compatible checkpoint at its authored anchor, with full integrity and the saved Resonance; projectiles, attack timers, and enemy attack phases reset. Death, voluntary transfer, and completion clear the checkpoint; transfer starts a fresh Wake and resets run progress. An invalid checkpoint falls back to a fresh Wake without discarding a valid archive. Backup recovery restores archive identity only, deliberately discarding a potentially stale pre-death checkpoint. If a write fails while ending a life, the on-screen message asks the player to retry transfer before closing; disk state cannot be invalidated until storage is writable.

Open Escape/Start > **MEMORIES** near either signal anchor or in the Wake's starting sanctuary. Only retained RETURN, MERCY, and EMBER are offered (RETURN is the initial default). Selection saves the active ability without changing retained-memory world effects: MERCY roots stay open with RETURN equipped, and retained EMBER still overrides MERCY even when MERCY is equipped. Loadout saves preserve the last completed bench snapshot. Full five-slot archive/forgetting, other combat styles, historical body records, and other memories remain deferred.

### Continuation: checkpoint/loadout and guard parity (2026-10-06)

Graphify dependency inspection identified `SliceSession` as the integration hub for `SliceArchive`, `SliceCombat`, room loading, interactions, and HUD. Checkpoint DTOs therefore use stable room/bench/enemy/collectible IDs, with schema version, life counter, and retained-memory signature validation. The optional checkpoint and active loadout share the archive's existing atomic file transaction; archive and run state remain separate data structures. Version-1 archive-only saves remain compatible. Unity inline JSON's handling of null classes requires an explicit checkpoint-presence marker.

Compared guard behavior against the read-only original `game.js`: successful deflect and late block now consume their guard windows and grant their brief invulnerability; existing invulnerability is checked before guard rewards, preventing multiple rewards from a single input. Charged release also deflects white attacks. Rest/room boundaries clear stale recovery, combo, cooldown, invulnerability, imprint, and input state. Full combat parity, including hitstop and the original strain/Resonance details, is still not certified.

Native menu verification found and fixed an existing numbered-choice binding bug: Unity Input System names the number-row controls `1`, `2`, and `3`, not `digit1`, `digit2`, and `digit3`. The corrected bindings cover dialogue, transfer, and loadout screens; the regression exercises all three keys, including opening Memories from pause and equipping a retained memory.

The movement motor, movement tuning, authored scenes, collision layouts, and player prefab were not changed. `MovementInput` exposes a read-only intent property so bench rest can cancel on movement intent even while the player is pushing into a wall.

### Player presentation continuation (2026-10-06)

The subsequent task was scoped to player presentation. [Wanderer presentation notes](PlayerPresentation.md) document the unified sprite owner, responsive locomotion/combat poses, landing compression/dust, pooled dash echoes, impact pose holds, render-only camera impulses, and distinct death/transfer treatments. Existing movement values, collision layouts, five-room progression, and combat rules are preserved. Impact holds affect only the player pose; full simulation hit-stop parity remains deferred.

Verification: **35/35 EditMode and 23/23 PlayMode tests passed**, including new locomotion, five-room feedback, real-hit camera/input independence, and immediate next-life presentation regressions. Actual Unity renders were reviewed in MovementLab and all five rooms; see the [pose sheet](Validation/player-presentation/pose-sheet.png) and `player-presentation-*` validation files. Missing dedicated animation art is explicitly listed in the presentation notes; adapted atlas frames currently cover those states. No new room or combat mechanic was introduced.

## Validation and remaining work

Final test and visual evidence is recorded in `Docs/Validation/phase2-*`. Tests distinguish direct system/transition checks from native input checks; programmatic completion is not a timed manual playthrough.

Continuation verification (2026-10-06):

- **35/35 EditMode tests passed**, including original movement/trajectory checks, archive-only save compatibility, checkpoint rejection rules, retained-memory/loadout conflicts, and conservative backup recovery (`phase2-checkpoint-editmode-results.json`).
- **19/19 PlayMode tests passed**, including all existing movement and slice tests plus five-room checkpoint/reload/completion, interrupted rest, failed-write retry, death invalidation, guard consumption/charged release, safe-area loadout restrictions, and native 1/2/3 modal input (`phase2-checkpoint-playmode-results.json`). Saves use isolated temporary paths; these tests never write the player's real archive.
- Inspected the [three-memory menu](Validation/phase2-checkpoint-memory-menu.png) at 1280x720. Live frame count advanced from 1,042 to 1,145 during setup using an ephemeral archive. The initial live keyboard check and subsequent regression exposed the invalid original number-key binding paths; the final native-input regression passed after correction.
- All nine original source hashes still match. Authored scenes, collision layouts, player prefab, movement motor, and tuning have no additional changes from the session's starting state. Editor left out of Play Mode with WakeSlice open and `runInBackground` false (`phase2-checkpoint-editor-state.json`).
- Graphify code graph refreshed after implementation (1,285 nodes / 2,191 edges). A saved correction identifies checkpoint/loadout as implemented; older semantic roadmap nodes can still describe the prior handoff. Serialized scene/asset coverage and the graph's previously reported extraction limitations still require source verification.

The first EditMode attempt caught Unity's null-inline-checkpoint serialization behavior; the presence marker fixed it before the passing run. Installed Unity Services/IAP packages continued to emit unrelated Editor authentication errors. These are recorded separately from test failures in `phase2-checkpoint-console.json`; this slice does not use those services.

Completed verification:

- **32/32 EditMode tests passed**, including all original trajectory/collision checks and six archive validation/save checks (`phase2-editmode-results.json`).
- **12/12 PlayMode tests passed** in the full movement-plus-slice run (`phase2-playmode-results.json`). After correcting later-life objectives and removing a duplicate Cistern collectible, **9/9 slice PlayMode tests passed**, including a new regression for completing a subsequent life with an already-remembered creature (`phase2-slice-final-playmode-results.json`). Together these cover four movement and nine slice tests.
- Scanned WakeSlice, MovementLab, and all five saved rooms: **zero missing scripts, prefab assets, or SpriteRenderer sprites** (`phase2-scene-verification.json`).
- Live Play Mode advanced from frame 2,111 through frame 17,072 during visual review. Inspected the Wake, Archive, transfer modal, and remembered Cistern bridge at the Game view's actual aspect ratio. The memory bridge was active with MERCY, and the tests verified its collider and EMBER conflict.
- All nine recorded original reference files retained their SHA-256 hashes. MovementLab, the original Player prefab, and MovementTuning have no changes from the Phase 1 baseline.
- Editor left out of Play Mode with WakeSlice open; `runInBackground` restored to its prior false value. Temporary visual-review transfers did not write the player's archive.

Preview images: [Wake](Validation/phase2-wake.png), [Archive](Validation/phase2-archive.png), [Transfer](Validation/phase2-transfer.png), [Remembered Cistern](Validation/phase2-mercy-cistern.png).

Two earlier test attempts were interrupted when the Editor left Play Mode; they are not counted as passes. A fresh Editor run completed the full suite. Earlier Editor startup also emitted Unity Services/IAP authentication errors from installed packages unrelated to the slice; no monetization or cloud service is used by the slice. An initial combat-input test caught a lost input edge; callback-based edge buffering fixed it. Sprite pivot import and later-life objective regressions were also corrected before final validation.

Remaining before calling this release-polished: a timed first-time keyboard/controller playthrough, difficulty and encounter spacing tuning, full original combat timing boundary coverage (including hitstop), map fog, audio controls/mix, accessibility/rebinding, standalone build/performance measurement, and richer unique art for the creature and narrative props. Training retains a simple practice state machine; the lancer does not yet reproduce the original moving lunge, and enemy ally shielding is not ported.

Recommended next milestone: validate and tune this exact five-room loop with players, then continue combat parity (hitstop/input timing, lancer lunge, ally shielding) and the broader archive/style loadout rules before adding King or further chapter rooms.
