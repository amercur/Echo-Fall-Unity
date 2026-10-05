# Echo-Fall: original project analysis and Unity migration plan

Analysis date: 2026-10-03. This document preserves the initial analysis and phased plan.

Implementation follow-up (2026-10-04): Unity is **6000.5.8f1**. The existing Phase 1 movement lab has been repaired and verified with **26 passing EditMode tests and 4 passing PlayMode tests**, including original-game trajectory comparisons and a live Belfry climb. See [Phase1-Movement.md](Phase1-Movement.md) for evidence and the remaining hands-on/controller, physical 144 Hz, and standalone-build checks. The analysis below preserves the original pre-initialization snapshot. No Phase 2 work has begun.

Original, read-only reference: `C:\Users\sivas\echo-fall`.
Destination: `C:\Users\sivas\Downloads\Unity Projects\Echo-Fall`.

## Direction and scope

Preserve the existing 2D game's movement, readable combat, authored exploration, and decisions that survive a transfer. Reimplement gameplay in C# with Unity's 2D rendering, collision, input, animation, UI, audio, scenes, and persistence facilities. Start with the existing rendered sprites. Converting the game into a fully 3D project would be a separate design decision.

Build in small, independently playable milestones. The first implementation should be a movement test room; the first complete gameplay slice should be the Wake's opening memory loop. Do not import the full chapter or build every subsystem at once.

Original files must remain untouched. Any later asset conversion or Blender export must operate on copies inside the new project or a dedicated staging directory. Original generator scripts can overwrite models and generated data, so do not run them in the original directory.

## Evidence and current project state

Inspected original source: `game.js`, `data/world.json`, `world-data.js`, `index.html`, `style.css`, package metadata, README and implementation notes, model workflow documentation, art manifests, test harness, and test suite. Visually inspected the original asset preview and existing `art-review/wake.png`; that render is a reference artifact, not a newly captured playthrough.

- Original package version is 0.5.0. It is plain JavaScript with Canvas rendering, DOM menus, Web Audio synthesis, and browser local storage. Gameplay resides in one approximately 1,280-line closure in `game.js`.
- `node --test tests/game.test.cjs`: **45 passed, 0 failed**. The harness uses a simulated DOM/Canvas and in-memory storage; it does not write original saves or game files.
- `node --check game.js`: passed.
- No hands-on browser playthrough or Unity editor/build validation was performed in this analysis. Controller feel and audio balance still need real-device review.
- Destination has a starter `Assets/Scenes/SampleScene.unity` with an orthographic camera and Global Light 2D, URP 2D settings, and an input actions asset. No custom C# gameplay scripts were found under Assets.
- Manifest declares Input System 1.19.0, URP 17.6.0, 2D Animation 10.1.4, Unity UI 2.0.0, and Test Framework 1.4.5. These are manifest declarations, not proof of successful package resolution.
- Active input handler is the new Input System; current fixed timestep is 0.02 seconds (50 Hz).
- `ProjectSettings/ProjectVersion.txt` is absent. Confirm the actual installed/editor version and package compatibility before implementation; do not guess an editor version from package numbers.
- Destination is not currently a Git working tree. Establish version control and a Unity-appropriate ignore file during setup, excluding Library, Temp, and Logs.

## What the existing game does

### Core loop and state ownership

Explore rooms, fight, rest, and make decisions. Death or voluntary transfer presents the decisions made during that life; preserve exactly one. With no decisions, the fallback is RETURN. The archive holds five distinct memories. A sixth distinct memory requires forgetting one, with confirmation. Extracting an already-held memory still advances the loop; it does not consume another slot.

The selected memory becomes equipped after extraction. Every archived memory may influence the world, regardless of equipment. Equipment controls abilities and movement modifiers. The original has three distinct predicates worth preserving explicitly:

- `remembered(id)`: exists in the persistent archive.
- `did(id)`: remembered OR decided during this run.
- `active(id)`: currently equipped identity.

| Lifetime | Existing contents | Proposed owner |
|---|---|---|
| Across transfers/reloads | Archive entries with acquisition loop, active identity, loop number, last seven bodies, endings, respect counter, discovered styles and equipped style | ArchiveSave |
| Within one run | Decisions, consumed interactions, visited rooms, shortcuts, boss flags, Child dialogue progress, run power, cached enemy/boss state, per-room sacrifice usage | RunState |
| Current encounter | Velocity, buffers, attack/deflect timers, projectiles, imprint target, hitstop, particles | Gameplay components |
| Bench checkpoint | Snapshot of compatible run, room and bench IDs, archive signature, room caches, decisions and flags | CheckpointSave |
| Preferences | Bindings, sound enabled, master/music/SFX volume, shake | SettingsSave |

A completed bench rest takes 0.45 seconds, restores six integrity, clears fracture, grants at least one Resonance, resets ability cooldown, and saves the run. Damage or player actions interrupt rest. Reload continuation restores the bench snapshot, not progress after it. Starting a new run, death, or reaching an ending clears the checkpoint. Styles are permanently unlocked but may only be equipped after completing bench rest. Memory switching is less restrictive: title/loadout, near a bench, or the Wake's starting safe area.

### Movement and collision

The original uses a fixed 120 Hz simulation and interpolated rendering. Its controller is explicitly authored AABB movement, not a general rigid-body simulation. Floors and walls are solid; platforms are one-way. Horizontal collision checks crossed boundaries to prevent dash tunneling. Camera tracks both axes, with look-ahead and room bounds.

Baseline tuning below is in browser pixels and seconds. At a proposed 100 game pixels per Unity unit, divide distances, speeds, and accelerations by 100; preserve durations. Browser Y increases downward.

| Parameter | Original value |
|---|---:|
| Player collision size | 22 × 40 px |
| Maximum integrity / Resonance | 6 / 3 |
| Run speed | 265 px/s |
| Ground / air acceleration | 2200 / 1250 px/s² |
| Gravity / maximum fall speed | 1450 px/s² / 830 px/s |
| Jump launch / release clamp | -600 / -220 px/s |
| Jump buffer / coyote time | 0.14 / 0.105 s |
| Wall slide cap | 95 px/s |
| Wall jump vertical / horizontal | -570 / 245 px/s away from wall |
| Wall grace / push-off lock | 0.11 / 0.055 s |
| Dash speed / duration | 720 px/s / 0.15 s |
| Dash cooldown | 0.58 s; 0.32 with DISTANCE active |
| Dash / deflect input buffers | 0.12 s each |
| Pogo vertical impulse | -535 px/s |

Support variable jump height, same-wall repeat jumps, one air dash, platform drop-through, and a DEFIANCE-only double jump. Landing, wall jumps, pogo, and aerial perfect deflects restore aerial options. Dash shortens attack recovery and grants a follow-up window. Downward attacks bounce from enemies, pogo objects, and overlapping hazards. Nonfatal hazard falls deduct integrity and return to safe footing; fatal damage triggers extraction.

### Combat

- Three-hit directional blade combo: base damage 2 / 2 / 3.5; cooldowns 0.22 / 0.22 / 0.34 seconds; combo window 0.72 seconds. Input buffers preserve follow-up taps.
- Pressing attack starts an ordinary attack immediately. Holding for at least 0.55 seconds and releasing also triggers the charged attack: base damage 5.5, recovery 0.46 seconds. This press/release behavior must not accidentally become a charge-only-on-press design.
- Upward cuts and airborne downward cuts use different hit boxes. Downward hits can pogo. Attack collision occurs when the attack executes; visuals then run through recovery. Do not introduce animation-event hit delays without an intentional gameplay change.
- White attacks permit tap deflects: guard lasts 0.30 seconds, ordinary perfect window 0.145 seconds, cooldown 0.32 seconds. Active RETURN extends the perfect window to 0.21 seconds.
- Red/heavy attacks require evasion or a charged guard release: minimum hold 0.48 seconds; counter window 0.21 seconds. Ordinary dash protection does not stop red attacks. Other invulnerability/ward checks still apply.
- Ground deflection requires facing the source; aerial deflection works from either direction. Successful deflects grant Resonance, clear fracture, add enemy strain, and refresh aerial options. Reflected projectiles return toward their source.
- A late white block adds 0.5 deferred fracture, bounded below fatal integrity. The next damaging hit adds the ceiling of accumulated fracture. A clean deflect clears it.
- Imprint spends the whole-number portion of Resonance (one to three) on a nearby target within 132 px. A second press detonates early; it automatically detonates after two seconds. Damage is `3 + 2 * charge + strain`, plus 2 when mature; strain is consumed.
- Mend requires one stationary grounded second and one Resonance for one integrity. Damage interrupts it.
- Needle, Crescent, and Breaker are permanent exploration unlocks, separate from memories. Needle is fast and narrow; Crescent gains two-sided third/charged cuts; Breaker is slow, short, stronger, and consistently staggers. EMBER replaces ordinary non-downward cuts with firebolts while retaining charged/downward form attacks.

### Enemies and bosses

| Actor | Existing behavior to preserve |
|---|---|
| Practice warden | First-loop sentinel in Wake; cannot take the player's last integrity; ordinary damage cannot finish it; successful deflect completes the lesson |
| Sentinel, 7 HP | Patrol, telegraph, two-hit sequence, recovery; protects nearby allies and reduces ordinary incoming damage to them; heavy stagger bypasses protection |
| Lancer, 10 HP | Longer red windup, forward lunge; missed attacks expose it for 1.5 seconds instead of 0.8 |
| Drone, 5 HP | Hover/patrol, telegraphed projectile; projectile can be reflected |
| Echo, normally 6 HP | Recognition pause/dialogue, then combat; archived identity changes behavior, such as firebolt, blink, healing, ward, parry, or betrayal lifesteal; KINSHIP Echo can refuse the fight |
| King | 40 HP, or 48 with archived ORDER. Blocks ordinary grounded cuts unless exposed or within dash follow-up; aerial/charged/bypass damage breaks prediction. Changes phase at half health or three prediction breaks; alternates sweeps, charges, and chains |
| Mother | Normally 48 HP, reduced to 38 when SACRIFICE is already remembered/decided at creation. Fires aimed spreads, falling shots and red ground shots; summons archived Echo identities. Phase two at half health increases recall frequency |
| Child | Dialogue encounter with three correct responses; no combat solution. Permission opens the Choir |

Boss arenas lock traversal during encounters. Defeats and room enemy state persist on backtracking within the run. The Mother is affected by additional keeper interaction logic; preserve both creation-time and interaction-time effects when migrating that encounter.

### Memory catalog

Keep the original IDs stable even where display names differ.

| ID / display | Equipped effect | Remembered or narrative consequence |
|---|---|---|
| return / RETURN | Parry ability; stronger ordinary perfect window and periodic healing on perfect deflect | Fallback when no decision was made |
| mercy / MERCY | Spirit strike | Creature returns; root route opens unless EMBER remembered; boss assistance after `currentLoop - acquisitionLoop >= 3` |
| fire / EMBER | Ordinary firebolts; fireburst | Creature gone, ash/scar; blocks MERCY roots |
| defiance / DEFIANCE | Double jump and blink | Archive–Lungs passage, unless ORDER remembered; extra Archive ledge |
| obedience / ORDER | Two-second ward | Cistern bridge, stronger King, blocks DEFIANCE passage |
| sacrifice / SACRIFICE | Heal two integrity once per room | Keeper/cage narrative and weaker Mother |
| betrayal / BETRAYAL | Heal on kill and two-sided strike | Keeper refuses help; Archive lift guardian; tougher Mother summons |
| respect / KINSHIP | Echo strike | Negotiates guardian passage, influences Echo recognition/refusal and SHARE ending |
| abandon / DISTANCE | Faster dash recharge and temporary invulnerability | Abandoned-self narrative |

The spirit and Echo active attacks are simple targeted strikes in this implementation; a richer personality simulation is future work. With no active memory, the ability button still defaults to a parry.

SHARE requires archived KINSHIP, archived MERCY or SACRIFICE, and persistent `respect >= 1`. That counter increases on extracting KINSHIP, not merely selecting a listening dialogue option. Three other endings are RESET, BREAK, and REMEMBER. Forgetting removes a memory's future influence; it does not decrement the historical respect counter.

### World structure

The chapter has twelve authored rooms in four themed areas. There is no procedural level generation. Gates are interaction-driven room transitions with named entry spawns, not automatic edge scrolling.

| Room ID | Connected destinations and relevant gates |
|---|---|
| wake | procession, belfry, cistern; observatory after well-link |
| belfry | wake, archive; vertical wall-jump route and Needle record |
| cistern | wake; procession via sluice; archive with MERCY and without EMBER; pogo route |
| archive | belfry; procession via archive-lift; lungs with DEFIANCE and without ORDER; cistern with MERCY and without EMBER |
| procession | wake, king; archive via archive-lift; cistern via sluice |
| king | procession; cradle after King victory |
| cradle | king, lungs, mother; keeper decision |
| lungs | cradle; archive with DEFIANCE and without ORDER; mother via lung-lift; Breaker record |
| mother | cradle; lungs via lung-lift; garden after Mother victory |
| garden | mother, observatory; choir after Child permission; Crescent record |
| observatory | garden; wake via well-link |
| choir | garden; ending choices |

Shortcut levers open paired routes from their far side. BETRAYAL adds a guardian to the Archive lift that must be defeated or negotiated past with KINSHIP. Map fog shows visited rooms plus adjacent unknown rooms and distinguishes open/locked links. There are caches, three hidden records, landmarks, benches, and abandoned-body interactions.

Geometry source precedence matters: `data/world.json` / `world-data.js` supplies routes, interactions, spawns, bounds, hazards, and enemy configurations. At runtime, `art-manifest.js` overrides ground, platforms, and available solids by room ID with exported Blender geometry. Memory-specific geometry is then added by code. Import the effective combination, not JSON alone. Compare the JSON and browser bundles before creating importer fixtures.

### Art, presentation, audio, and UI

- Current Wanderer atlas: `assets/wanderer-smooth.png`, 40 frames, 192 px cells, four columns. Idle 0–3, run 4–19, jump 20–21, attack 22–29, guard 30–31, rest 32–39. `wanderer.png` is the older 16-frame fallback.
- Five bestiary atlases: sentinel, lancer, drone, king, mother. Each has eight frames in 256 px cells: four walk, two anticipation, strike, recovery. Echoes use the Wanderer presentation rather than a separate dedicated atlas.
- Architecture sprites: ruin arch, choir spire, floating ledge. All have explicit anchors in manifests. Game art combines these with procedurally drawn cathedral silhouettes, masonry, landmarks, platforms, glows, particles, blade arcs, and afterimages. Copying the PNGs alone will not recreate the scene.
- `models/echo-fall.blend` contains Wanderer, ruin kit, old four-area blockout and current twelve-room map scene. `models/bestiary.blend` contains five enemy/boss scenes. `models/map-layout.json` records exported geometry. Preserve `.blend` source copies outside Unity's initial runtime asset import; use PNGs first.
- Foot anchors prevent hovering/sliding: Wanderer anchor is (96, 161.529) in a 192 px cell, with browser draw scale 0.34. At 100 game px/unit, using 100 texture px/unit requires visual scale 0.34; alternatively bake the scale into PPU. Do not apply both. Flip left/right around the pivot; invert top-origin sprite rectangles and anchors for Unity's bottom-origin representation.
- UI uses dark navy, cyan, rose, and gold, with six integrity pips, fractured segments, Resonance diamonds, ability/dash/form state, boss health, area label, objective prompts, minimap, memory archive, active identity, loop counter, and a short narrative log.
- Modal states: title, loadout, help/pause, settings/rebinding, memory/style menus, map, dialogue choices, extraction, forgetting confirmation, ending. Gameplay pauses behind menus; presentation/audio has separate timing.
- Keyboard defaults: A/D move, W/S aim, Space jump, J/X attack, K/Shift dash, F guard, Q imprint, H mend, L/C memory, E interact, M map, R confirmed transfer, Escape pause. Controller supports stick/D-pad plus A jump, X attack, B dash, Y interact, LB guard, RB imprint, LT memory, RT mend, View map, Menu pause. Settings remap keyboard controls; menu navigation supports controller.
- Audio is synthesized in code, not shipped as a soundtrack/SFX library. Layered combat cues, area/memory-dependent music, bench music, footsteps, and ambience need deliberate recreation or offline baking. Preserve separate master/music/SFX buses, mute, variation, and bounded voices. Font styling comes from optional web fonts with system fallbacks.

## Proposed Unity design

These are implementation recommendations, not claims that the systems already exist.

| Concern | Proposed native implementation |
|---|---|
| Player | Player prefab, BoxCollider2D, kinematic Rigidbody2D and explicit C# motor using 2D shape casts/contact queries; preserve authored acceleration rather than relying on default gravity/friction |
| Collision | Separate ground/solid/one-way/hazard/actor/hitbox layers; explicit one-way and drop-through policy; PlatformEffector2D only if it matches the controller tests |
| Input | Input System gameplay and UI action maps; collect input edges in rendered frames and consume once in simulation; configurable buffers and rebinding |
| Timing | Initially target 1/120 fixed simulation to match the reference, with render interpolation. Treat timer/hitstop ownership explicitly; do not halve the simulation rate before measuring parity |
| Combat | C# attack/guard state and hit queries; shared damage result model for hit, immune, guard, deflect; data assets for timings, damage, reach, and forms |
| Visuals | SpriteRenderer, sprite AnimationClips/Animator driven by gameplay state; sorting layers, URP 2D lighting, pooled particles and trails. Gameplay owns timing; animation displays it |
| Camera | Orthographic room-bound follow with horizontal look-ahead and vertical tracking; custom small component initially |
| Content | ScriptableObject definitions for memories, forms, enemies, dialogue and rooms; stable IDs for serialization; definitions never store mutable save state |
| Rooms | Persistent bootstrap/player/UI plus one loaded room scene at a time. Save RoomState on exit and restore on return; introduce additive scene loading when connecting the first two rooms |
| Decisions | Explicit effect/condition evaluators using archive, active identity, and current-run state; avoid scattered string checks in unrelated components |
| UI | Unity Canvas/UI with text, prefabs and EventSystem/Input System navigation. Central screen state governs pause and action maps; HUD subscribes to state changes |
| Persistence | Versioned JSON under Application.persistentDataPath, validated DTOs and IDs, temporary-write/replace plus backup recovery. Separate archive, checkpoint, and settings lifetimes |
| Audio | AudioSource pools and AudioMixer buses; baked/generated clips or intentional new recordings. Recreate music behavior incrementally |
| Tests | Unity EditMode for rules/serialization/data validation; PlayMode for movement, combat, transitions, and checkpoint continuation |

Do not use one giant GameManager as a transliteration of `game.js`. Keep the initial structure small: a session coordinator, player motor/input, player visuals, and tuning data. Add combat, memory, save, room, and UI responsibilities only in the milestone that needs them.

Suggested eventual folders: `Assets/EchoFall/{Scenes,Prefabs,Scripts,Data,Art,Animations,Audio,UI,Tests}`. Keep analysis and import provenance under `Docs`. No need to create empty scaffolding for every future system now.

Coordinate conversion proposal: for a browser rectangle `(x,y,w,h)`, its Unity center is `((x+w/2)/100, (452-y-h/2)/100)` and size is `(w/100,h/100)`. For a foot point, use `(x/100,(452-y)/100)`. Preserve room-local coordinates, including negative browser Y in climbing shafts. A 960×540 reference viewport corresponds to 9.6×5.4 world units and orthographic size 2.7. Keep gameplay collider dimensions independent from transparent sprite extents.

Browser saves are origin-bound local storage, not project files. Unity should initially start with a fresh save. A later optional JSON import tool can translate browser archives; it is separate from porting game mechanics and must not read or alter browser data automatically.

## Phased delivery and acceptance gates

Each phase builds on the previous one. Stop at its acceptance gate and review the playable result before broadening scope.

### 0. Confirm the Unity baseline

Confirm editor version, package resolution, input setup and a clean empty-scene build; establish version control. Record source hashes for files actually copied later. Agree on 2D presentation, 100 px/unit conversion and fixed-step baseline through the first movement prototype. Preserve the original SampleScene until a replacement is verified.

**Done when:** project opens/compiles and a minimal desktop build launches; editor/package versions and import conventions are recorded. This analysis does not claim that gate has passed.

### 1. Movement test room — the next implementation task

Create `MovementLab`, one player prefab, minimal Input System actions, a tuned C# motor, orthographic camera, solid floor/walls, one-way platforms and a hazard/reset zone. Copy only the Wanderer atlas and its needed metadata. Implement movement, variable jump, coyote/buffer, wall slide/jump, drop-through and one air dash; show idle/run/jump/guard-independent movement animation.

**Done when:** short/held jumps differ correctly; buffered/coyote jumps work; same-wall climbing is possible; ceilings and walls stop motion and dash; platform drop-through cannot fall through solid ground; exactly one air dash is available; a Belfry-sized test shaft is traversable; movement remains consistent at 30/60/144 Hz rendering. Use original trajectories and the 45-test suite as references, not an assumption that Unity physics matches automatically.

**Exclude from this phase:** enemies, bosses, full world importer, full memory catalog, endings, polished menus and soundtrack.

### 2. Combat practice

Add the three-hit chain, directional/charged cuts, recovery buffering/cancel rules, basic damage, white deflect, charged red counter, fracture, Resonance, imprint, mend, and pogo. Add one configurable practice enemy, a projectile fixture and a red-attack fixture. Drive anticipation and recovery sprites from combat state.

**Done when:** white/red defenses differ correctly; dash does not protect against red attacks; facing rules and projectile returns work; late block debt applies and clears correctly; imprint charge/strain/maturity damage matches; pogo resets aerial movement; training cannot kill the player. Test boundaries around timing windows and ensure inputs during hitstop execute at most once.

### 3. First complete slice — Wake memory loop

Build only the Wake: bench, practice warden, wounded creature, transfer glass, HUD, simple dialogue, extraction and loadout UI. Implement RETURN, MERCY and EMBER with real effects, persistent archive/loop and a small Wake bench checkpoint. Add a changed return-creature/scar presentation and the MERCY/EMBER conflict evaluator. Track local decisions separately from the archive.

**Done when:** player explores, makes a choice, transfers, chooses exactly one memory, and sees its effect on the next Wake. No-choice transfer yields RETURN; equipment changes ability but retained memories still influence the world. Reload restores archive or last completed bench; death clears the checkpoint. Test capacity/forget rules with seeded archive data even before all memories are exposed in content.

This is the first reviewable end-to-end Echo-Fall slice. Keep all other room gates visibly unavailable until their destination is implemented.

### 4. Connected exploration and remaining memory rules

Connect Wake, Belfry, Cistern, Archive and Procession incrementally. Build an editor importer for effective geometry and stable room/interaction IDs, room-state cache, shortcuts, map fog, hazards/pogo route, regular sentinel/lancer/drone, hidden records and Needle. Introduce other memory definitions and rule tests as their interactions become available; do not show unimplemented abilities in player menus.

**Done when:** backtracking preserves defeated enemies and opened shortcuts, transfers reset run state, map edges and named spawns are valid, MERCY/EMBER and DEFIANCE/ORDER route conflicts rebuild correctly, BETRAYAL/KINSHIP guardian logic works, and full multi-room bench snapshots survive reload.

### 5. Remaining chapter, bosses and endings

Add King/Court first, then Cradle/Lungs/Mother, then Garden/Observatory/Choir. Complete keeper and Echo decisions, body records, remaining memory abilities, Breaker/Crescent, boss phases and recall identities, Child dialogue, and four endings. Check each route as it becomes available.

**Done when:** King prediction rules and Mother recalls match; mercy assistance starts after the correct acquisition-relative three transfers; Child only yields to dialogue; all ending conditions and permanent style unlocks persist; all twelve rooms are reachable through valid progression; forgotten memories remove future influence.

### 6. Presentation and release validation

Complete layered scenery/landmarks, sprite polish, impact effects, sound recreation, settings, keyboard remapping, controller navigation, dynamic input prompts, shake settings and resolution scaling. Basic readable feedback should already exist in earlier phases; this phase completes coverage and polish.

**Done when:** reference scenes compare well at matching framing, controller playthrough and keyboard rebinds work, audio/mute/volumes behave correctly, corrupt/old saves fail safely, a desktop build completes the chapter, and performance is measured under full combat load. No full-campaign or procedural-generation expansion is part of this migration.

## Main risks and verification strategy

1. **Movement drift:** different collision resolution, time step or controller friction can invalidate authored jumps. Establish the controller and shaft/pogo fixtures before importing all rooms.
2. **State drift:** confusing archive, active memory and current decisions will change progression. Keep rule tests for all three concepts and explicit run-boundary reconstruction.
3. **Geometry drift:** exported geometry overrides JSON and uses a flipped vertical axis. Validate effective rectangles, named spawns, room bounds and route targets on import.
4. **Visual scale/pivot drift:** atlas pixels are scaled in the browser; setting a generic PPU is insufficient. Verify feet, facing, boss scale and each sliced frame against manifests.
5. **Timing drift:** animation transitions, low-frequency fixed updates, input polling, and global timeScale can alter deflects/hitstop. Use gameplay timers with separate UI/presentation clocks.
6. **Save drift:** live Unity object references cannot replace stable serialized IDs. Version DTOs; validate missing rooms/memories/benches; test interrupted writes and invalid checkpoints.
7. **Scope drift:** the existing game already has substantial content. Deliver one movement room and one completed memory loop before scaling out.

Port behavioral tests as each corresponding system lands. Prioritize trajectory checks, collision boundaries, hit outcomes, acquisition-relative loop conditions, save round trips and room graph invariants. Do not duplicate all 45 tests before implementation. Supplement automated checks with matched visual captures and hands-on keyboard/controller play; the original harness cannot establish subjective feel or hardware behavior.

## Source navigation for implementation

Locations below refer to the unchanged original `game.js` examined on the analysis date; function names remain the primary reference if lines later move.

| Source location | Responsibility |
|---|---|
| `game.js:30` onward | Memories, forms, save validation and lifetime variables |
| `game.js:221` onward | Checkpoint validation/write/resume and settings/UI helpers |
| `game.js:320` onward | Player defaults, run construction, room loading, enemy defaults, gates, map |
| `game.js:406` onward | Interactions and decisions |
| `game.js:473` onward | Child/Choir progression, endings, death, extraction, forgetting |
| `game.js:536` onward | Deflect, counter, damage, boss damage, attacks, imprint, memory abilities |
| `game.js:673` onward | Collision, hazard return, fixed gameplay update |
| `game.js:786` onward | Enemy, boss and projectile update rules |
| `game.js:876` onward | Asset drawing, animation selection, architecture, landmarks, HUD |
| `game.js:1239` onward | Fixed-step frame loop, keyboard input and test interface |
| `data/world.json`, `world-data.js` | Room content and browser bundle |
| `assets/art-manifest.js`, associated JSON manifests | Actual selected art, frame ranges, anchors and collision overrides |
| `models/README.md`, `tools/` | Blender generation/export workflow; inspect before using on copies |
| `tests/game.test.cjs`, `tests/harness.cjs` | Executable reference behavior and test entry points |
| `index.html`, `style.css`, `art-review/` | UI structure, styling and existing gameplay render references |

At the end of the initial analysis, only this plan had been added; original files and Unity implementation were unchanged. The subsequent Phase 1 work and its validation limits are recorded in [Phase1-Movement.md](Phase1-Movement.md).
