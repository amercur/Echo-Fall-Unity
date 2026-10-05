# Phase 2: the first Wake slice

Implementation date: 2026-10-05. This milestone follows the user's expanded Phase 2 scope: combat, five connected rooms, presentation, and the first memory loop. It brings forward selected work from the original plan's phases 3, 4, and 6; it does not claim those entire phases are complete.

## Play

Open `Assets/EchoFall/Scenes/WakeSlice.unity` and press Play. The bootstrap loads `Wake_wake` additively. The original `MovementLab`, its player prefab, and its movement tuning are preserved. `WakeSlice` is first in Build Settings; all five destinations and MovementLab are included.

Keyboard: A/D move; Space jump; K/Shift dash; J/X strike (hold then release for charged cut); W/S aim; F tap for white deflect, hold at least .48 seconds and release to counter red; Q imprint/early detonation; H hold while still to mend; C/L active memory; E interact; R voluntary transfer; Escape controls/pause. Dialogue uses buttons or 1/2/3. Controller movement and actions are bound; dialogue choices also use A/X/B. Physical controller testing remains necessary.

### Suggested first run

1. Rest at the Wake's signal anchor. Climb the introductory steps and practice deflecting the warden. It cannot take your last integrity and is defeated by a clean deflect.
2. Speak to the wounded creature. MERCY costs two integrity, including a potentially fatal last gift. EMBER grants one extra blade damage for this life.
3. Clear all four enemies in the Pilgrim Causeway. The sentinel uses white attacks, the lancer telegraphs red attacks, and the drone fires reflectable bolts.
4. Climb the Belfry and find the Bell Keeper's record. Reach the Glass Archive and open its lift from the far side.
5. Explore the Cistern, retrieve the Root Keeper's record, and open the sluice. Use jump/dash and the pogo bells; water damages and returns you to safe footing.
6. Visit all five rooms, then return to either transfer glass (Wake's first raised dais or Archive's upper dais). A completed route receives the completion ending. Earlier voluntary transfer and fatal damage still enter extraction, preserving the failure path.
7. Select the one offered decision. The next life begins in the Wake and resets enemies, collectibles, shortcuts, and visited rooms.

This route is designed for a first-time 10–15 minute exploration/tutorial session. That duration is a pacing target, not a measured human playthrough. An experienced player can complete the small source layouts much faster. No artificial wait timers were added to force a duration.

## Rooms and source fidelity

| Room | Purpose | Added slice presentation/content |
| --- | --- | --- |
| Wake / Signal Well | Introductory traversal, safe practice, creature choice | Signal anchor, transfer dais, remembered creature or ember scar |
| Pilgrim Causeway / Procession | Combat | Four source encounters, faded standards, route-clear objective |
| Belfry / Broken Ascent | Vertical traversal and secret | Original wall-jump shaft, suspended broken bell, Bell Keeper record |
| Cistern / Hollow Roots | Hazard exploration, pogo, memory shortcut | Dark water, pogo bells, Root Keeper record, actual MERCY bridge collision |
| Glass Archive / Unfiled Lives | Narrative reflection and completion | Glass-life shelves, original lift lever, upper transfer glass |

`Tools/import_wake_slice.py` reads the original `data/world.json` and the overriding exported geometry in `assets/art-manifest.js`. It copies six PNG assets and writes a normalized fixture consumed by `WakeSliceBuilder`. Original files are never written and source generators are never run. Source hashes are recorded in `Validation/phase2-source-hashes.json`.

Original room bounds, ground, platforms, walls, hazards, spawns, normal enemy placement, and in-slice connections are used. The original gate model remains interaction-driven. The extra Cistern record, completion objective, root bridge, and repositioned transfer glass are deliberate slice adaptations. Gates beyond the five rooms are sealed lore interactions. Additional forms, keeper/Echo choices, and the DEFIANCE door are not exposed as functional content.

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

Benches currently restore this life only; there is no mid-run checkpoint save. On application restart the persistent archive loads into a fresh Wake. Full five-slot archive/forgetting, loadout switching, historical body records, and all other memories remain deferred.

## Validation and remaining work

Final test and visual evidence is recorded in `Docs/Validation/phase2-*`. Tests distinguish direct system/transition checks from native input checks; programmatic completion is not a timed manual playthrough.

Remaining before calling this release-polished: a timed first-time keyboard/controller playthrough, difficulty and encounter spacing tuning, full original combat timing boundary coverage (including hitstop), bench checkpoint continuation, map fog, audio controls/mix, accessibility/rebinding, standalone build/performance measurement, and richer unique art for the creature and narrative props. Training retains a simple practice state machine; the lancer does not yet reproduce the original moving lunge, and enemy ally shielding is not ported.

Recommended next milestone: validate and tune this exact five-room loop with players, then complete checkpoint/loadout persistence and combat parity before adding King or further chapter rooms.
