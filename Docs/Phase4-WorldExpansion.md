# Phase 4: the connected world

Phase 3 is the baseline. This milestone expands exploration without changing player movement values, combat rules, enemy behavior or the King fight. Start in WakeSlice. Defeat the King and use the new eastern Court arch to continue; the Court transfer glass and original five-room completion remain available.

## Source and dependency review

Graphify traced `SliceRoom.Apply -> SliceSession -> SliceArchive -> SliceCheckpoint`, plus player traversal, enemy health caching, interaction gates and HUD ownership. Six-room ID whitelists were the principal persistence risk. `WorldCatalog` now provides one authored definition for room loading, map connections and checkpoint validation. Scene objects remain authoritative for collision and interaction placement; tests compare their IDs and gates against the catalog.

The original read-only `C:\Users\sivas\echo-fall` provides twelve-room connectivity, named spawns, interactions, effective geometry (world data overridden by art manifest), the Child's three dialogue answers and run-state lifetimes. `Tools/import_world.py` imports copies only. It does not run original generators or modify original files.

## New rooms and regions

| Room | Region and exploration role |
| --- | --- |
| Cradle | Copper suspension threads, sheltered anchor, ascending platforms to the Lungs, a spaced Lancer patrol |
| Suspended Lungs | Tall machinery, wall-jump shaft, airborne Drone pressure and a separate ground Sentinel; upper cache and record; two far-side return latches |
| Immortal Chamber | Dormant radial engine; upper maintenance tread and release; optional Sentinel/Drone pairing on the eastern floor; Mother encounter deferred |
| Last Garden | Branching silhouettes, living roots and drifting pollen; quiet checkpoint and Child dialogue; upper seed record and hidden entrance |
| Quiet Observatory | Open star field, observation lens and plinth; optional elevated record, cache and the signal-well return to Wake |
| Choir | Suspended organ pipes, concentric memory halos and restrained signal dust; checkpoint and original-memory lore; ending choices deferred |
| Root Vault | New optional hidden chamber below the Garden map node; broken floor, one-way ledges, pogo bell, optional Drone, cache and gardener record |

There are thirteen connected rooms including the preserved six. Region identity uses distinct architectural composition, silhouettes, layered parallax, lighting and atmospheric motion; it does not rely only on palette swaps.

## Routes, shortcuts and secrets

Main route: Wake → Procession → Court → Cradle → Immortal Chamber → Last Garden → Choir. The original route through the Lungs returns above the Chamber. Garden → Observatory → Wake completes the long return loop. Existing Belfry, Cistern, Archive, sluice and Archive lift routes remain intact.

- Lungs maintenance lever opens both ends of the Lungs–Chamber lift.
- A high latch in the Lungs opens both ends of the breathing passage back to Archive. This replaces the original DEFIANCE/ORDER gate for this exploration milestone; those identities are not falsely granted.
- The Observatory well lever opens both ends of the long return to Wake.
- The Garden causeway release is above the western Chamber stair. It substitutes for the original Mother victory gate while Mother combat remains deferred. It never records a Mother defeat.
- The Garden's upper root opening leads to Root Vault. Its map node remains hidden until entered. A bent root is the environmental clue; there is no conspicuous passage arch.
- Lungs/Observatory caches, the upper Garden seed, and Vault cache/record reward optional exploration. Source style records remain readable lore; they do not unlock unimplemented combat styles.

Cradle/Garden stairs require controlled jumping and horizontal commitment; the Choir's separated platforms reward air dash. The Lungs shaft uses the unchanged wall-jump motor. One-way platforms allow deliberate drop-through and return drops. Vault traversal combines ledges, an air-dash option and a downward-strike pogo target. Retained MERCY adds a safe living-root bridge across the Vault; retained EMBER suppresses it, matching the existing Cistern consequence. The original Chamber's 137-pixel upper step exceeds the unchanged 124-pixel jump apex, so one maintenance tread was added.

The Child uses the original three correct responses and opens the Choir by permission. Progress is saved at anchors and resets with the life. KINSHIP acquisition and ending consequences await the broader memory system; the dialogue does not overwrite the existing MERCY/EMBER decision.

## Map and persistence

M or controller right-stick press opens the map; M, Escape/Start, right-stick press or the Return button closes it. Gameplay pauses through the existing session-screen mechanism. The map shows named visited rooms, adjacent unnamed rooms, current location, anchors and open/locked connections. Distant rooms and unvisited Root Vault remain hidden.

Discovery, opened shortcuts, consumed caches, Child progress, defeated enemies, remaining enemy HP and King progression belong to the current life. They survive backtracking and completed anchor save/reload. Death/transfer clears that run and its map, as in the original; retained memories and loadout follow the existing archive rules. No new cross-life shortcut or boss persistence is introduced.

New anchors: Cradle, Last Garden and Choir. Existing interrupted-rest, healing, loadout restriction, atomic save/backup recovery and archive-only compatibility remain in place. Version-1 checkpoint records gain an optional bounded Child stage; old records default to zero. Catalog validation rejects unknown IDs, invalid room/bench pairs and unimplemented boss flags.

## Validation

Validated in Unity 6000.5.8f1 on 2026-10-07 (local time). Reports and captures use `Docs/Validation/phase4-*`.

- **45/45 EditMode tests passed**, including all existing regressions, catalog/save compatibility, the thirteen-scene audit, named entries, map fog, Cradle/Garden/Chamber stair routes, drop-through and the Lungs wall-jump route. The initial stair input fixture dashed too early and reversed during the dash; the final routes use deliberate takeoff positions with unchanged movement tuning.
- **37/37 PlayMode tests passed** (170.83 seconds): all existing movement/combat/boss regressions plus all **36 directed gates**, both ends of three new shortcuts, Child permission, secret consumption, every new anchor's save/reload, enemy health and King victory restoration, loadout restoration and combat deaths in Cradle/Garden/Choir, map pause/fog, Vault pogo and MERCY/EMBER bridge consequences.
- After the final map layout/scenery fix, **45/45 EditMode tests passed again** and the focused map PlayMode regression passed **1/1**. No gameplay code changed in that presentation fix.
- Original JavaScript suite: **45/45 passed**. Preservation audit: **34 comparisons passed**, including original-source hash manifests, unchanged movement/combat/enemy/King/archive files and all baseline scene colliders. Original project files were never edited.
- Reviewed all seven new room renders, both Vault memory states, and fogged/full maps in live Play mode with an ephemeral archive. The initial review caught a clipped widescreen map title and generic backdrop groups whose slash-containing names defeated `Transform.Find`; both were fixed and recaptured. These are scripted presentation checks, not a human exploration playthrough.
- No missing scripts or sprites were found across the thirteen room scenes. Build settings include every gate destination. Existing MovementLab and Wake regression scenes remain available.

Preview: [world map](Validation/phase4/map-full.png), [fogged map](Validation/phase4/map-fog.png), [Garden](Validation/phase4/garden.png), [Lungs](Validation/phase4/lungs.png), [Choir](Validation/phase4/choir.png), [Vault roots](Validation/phase4/rootvault-mercy.png).

## Remaining gaps and Phase 5

Mother combat, Echo identities, keeper decisions, broader memories (including original DEFIANCE/ORDER traversal), combat styles and ending choices remain deferred. Root Vault is an explicit new optional room; the remaining twelve-room topology comes from the source. New scenery uses authored procedural silhouettes and the existing architecture kit; Keeper/Child markers still need dedicated character art. Traversal checks and scripted transitions do not substitute for first-time controller exploration, difficulty tuning or standalone performance measurement. Dedicated player animation and authored audio gaps from Phase 3 remain.

Phase 5 should first playtest and tune this network's navigation, readability and return pacing, then implement the broader memory/style archive with save migration and restore its original narrative gates. Add Mother as the next major encounter after those dependencies are stable. Endings should follow their memory/Child prerequisites rather than arrive as isolated menus.
