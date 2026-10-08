# Phase 3: combat and the Court

Phase 4 continuation: this combat milestone is now the preserved baseline. The Court retains its transfer glass and gains the original eastern connection to Cradle. See [Phase4-WorldExpansion.md](Phase4-WorldExpansion.md) for the expanded world and the deferred Mother/ending scope. Statements below describing Court as the world boundary are historical.

Phase 2 (including checkpoint/loadout persistence and player presentation) remains the baseline. The original JavaScript project is read-only. This milestone extends the five-room Wake with the Court only; the remaining chapter is not imported.

## Entry and progression

Use WakeSlice. The original five-room completion and transfer route still works. The Procession's eastern passage now enters the Court. Walk beyond the Court threshold to start the King: the arena seals until victory, while pause and voluntary transfer remain available. Victory heals two integrity, records the King as defeated for this life, clears projectiles/imprint, releases the arena and unlocks the Court's transfer glass. Return to Procession to continue exploration or rest at an existing Wake/Archive anchor to save the victory. The original Cradle destination is deliberately represented by this milestone's victory glass.

The Court uses original King room bounds, ground, platforms, spawns, arena boundaries, and the eight-frame King atlas (.65 scale, original foot pivot). Existing Wake/Belfry/Cistern/Archive/MovementLab scenes, bootstrap, player prefab and movement tuning are preserved. Procession changes are confined to its Court gate and one existing Sentinel's placement/patrol bounds alongside the Lancer. No enemies were added to the safe Archive.

## Dependency review before implementation

Graphify identified the direct SliceCombat–SliceSession dependency and traced the shared paths through PlayerMotor, PlayerVisual, SliceEnemy, SliceProjectile, SliceRoom, SliceHUD, SliceSound, SliceArchive and SliceCheckpoint. Checkpoint validation explicitly enumerates room/enemy IDs; adding a boss therefore requires updating both runtime room loading and the saved-data validator. Enemy subclasses must remain discoverable by room health caching, imprint, melee, projectiles and defeat tracking. Transfer/new-life clearing must include boss prediction state. The five-room completion predicate must count the original rooms rather than assume that exactly five room IDs can ever be visited.

Source verification used `game.js` combat routines (530–670), movement/combat update (698–782), enemy/boss/projectile routines (786–875), `data/world.json`, and `assets/art-manifest.js`. Graph relationships were verified against source because serialized Unity scene/asset coverage is incomplete. Source hashes and a pre-change Unity file manifest are in `Docs/Validation/phase3-*`.

## Player combat

- Source directional blade volumes, 2 / 2 / 3.5 basic chain, .22 / .22 / .34 recovery, .72 combo window. Recovery taps buffer through the remaining recovery instead of expiring prematurely.
- Press attacks immediately; a .55-second hold/release adds a 5.5-damage charged third strike with .46 recovery. Existing animation ownership remains in PlayerVisual.
- Rising cuts, airborne downward cuts, enemy/object/hazard pogo, 5.35 pogo impulse, air-option and dash-cooldown restoration, and short pogo invulnerability.
- Dash shortens attack recovery to .06 and opens a .5-second prediction-breaking follow-up. Baseline locomotion constants are unchanged; charging does not slow movement.
- .30 guard, .145 ordinary perfect window (.21 with RETURN), .12 deflect buffer; charged release at .48 opens .21 counter/perfect windows. Facing matters on the ground, not in the air. White deflect and red charged counter remain different defenses.
- Deflect grants 1 Resonance / 3 strain; charged counter grants 1.5 / 5. Strain caps at 15. Aerial deflect lifts at least 2.1 upward. RETURN restores one integrity on deflect at most once per four seconds.
- Fractional Resonance: connected melee/pogo .25; friendly projectile hit .2; cap 3. Imprint spends only the integer portion, retains fractions, grants .15 invulnerability, and deals `3 + 2*charge + strain`, plus 2 when mature at two seconds. Dead targets clear the mark.
- Stationary grounded mend requires one uninterrupted second, at least one Resonance, no attack recovery and no held guard; restores one integrity and clears fracture. Damage/actions interrupt focus.
- Damage uses one-second invulnerability, .12-second knockback steering lock and source impulses (2.7 horizontal away, 1.7 upward), cancels dash/charge, and preserves nonlethal training behavior.
- RETURN active ability uses its separate .34/.24 guard/perfect windows and .9 cooldown. MERCY reaches 4.5 units (5 enemy damage / 4 boss damage). EMBER uses 2.7-damage bolts plus current-life fire power and a two-sided fireburst. Both offensive abilities have four-second cooldowns.

## Shared enemy framework and roster

`SliceEnemy` owns fixed-step state progression, bounded collision-swept movement, hit rectangles, damage/shielding, stagger, strain, knockback, atlas presentation, tells, death remnants and stable defeat IDs. `CombatRules` centralizes blade geometry and swept segment/rectangle collision. `SliceKing` specializes the same damage/imprint interface. `CombatHitResult` distinguishes immunity, block, deflect and damage so enemy hit/miss recovery is reliable.

| Enemy | Completed behavior |
| --- | --- |
| Practice warden | Nonfatal teaching attacks; ordinary damage cannot finish it; deflect completes training |
| Sentinel (7 HP) | .55 patrol, .46 anticipation, two lunging white strikes separated by .32 anticipation, .7 recovery; moves to protect nearby allies, reducing light damage to 30%; heavy stagger bypasses protection |
| Lancer (10 HP) | .78 red anticipation, 4.3-speed .26 lunge, .8 recovery on connection versus 1.5 on miss; punishable after evasion or charged counter |
| Drone (5 HP) | .3 patrol, .18 hover amplitude, .7 anticipation, aimed 2.35-speed white bolt, 2.5 recovery; reflectable and susceptible to melee/imprint |
| King (40 HP) | Prediction guard, three attack patterns, two phases, arena lock, shared damage/imprint/deflection and persistent defeat |

Source hit reactions use .12 light / .55 heavy-or-third-hit stagger, .1 horizontal displacement, visible hit flash and bounded motes. Enemy impact holds pause that actor briefly; player input and movement continue. Deflects replace regular enemies' current attacks with readable .25/.65 recovery windows. Sentinel shield and attack-direction indicators are thin, restrained lines; heavy tells are red. The existing Procession roster provides the mixed Sentinel/Lancer encounter; Drone pressure remains in Procession and the traversal rooms.

## King

Ordinary grounded cuts are predicted unless the King is exposed or the player is in dash follow-up. Aerial, charged and explicit bypass hits work. Changing among aerial/dash/charge approaches counts prediction breaks. Half health or three breaks starts a 1.4-second exposed phase transition.

Phase one rotates white charges, chains, and red low sweeps. Phase two reorders the patterns and speeds the white anticipation/chain cadence. Charges move at 4.6 units/second; red sweeps cover the low 42-pixel band and can be jumped. Completed patterns expose the King for .9 seconds; deflect/counter exposes .65/1.8 seconds. The HUD shows health, phase, breaks and the current defensive cue. Original tell/strike/recovery sprites, intro/phase notifications, impact feedback, arena seals and a fading defeat remnant provide presentation without input locks.

## Persistence and presentation

Version-1 archive saves and earlier integer-Resonance checkpoints remain compatible. Checkpoints now accept fractional Resonance and Court room/boss IDs plus bounded prediction metadata. Existing atomic writes, backup recovery, archive validation and safe-area loadout restrictions remain in place. Combat timers, projectiles and attack phases do not resume from disk. Victory survives backtracking and bench reload; death/transfer clears the checkpoint and all run victories, while retained identities and the active loadout follow the existing rules.

Existing player animation/effects are retained. Audio cues route through the bounded procedural voice pool for attacks, tells, impacts, armor, counter, imprint, detonation, mend, intro/phase and defeat. These are synthesized placeholders, not an authored final mix.

## Validation

Validation completed on 2026-10-07; results and render evidence are in `Docs/Validation/phase3-*`.

- EditMode: **38/38 passed**, including MovementLab geometry and combat/save rules.
- PlayMode: **31/31 passed**, covering native-input combo/charge buffering, rising/pogo, deflect/counter, reflected projectiles, every archetype, Sentinel/Lancer shielding, King patterns/phases and actual player damage, victory/backtracking, combat death/transfer, and checkpoint/loadout restoration. Existing movement and five-room regressions remain included.
- Original JavaScript suite: **45/45 passed**. All 13 comparisons against the Phase 2/3 source hash manifests match; the original project was not modified.
- Eight-scene audit: no missing scripts or sprites, all gate destinations included in build settings, and all eight King frames available with the source pivot. Baseline scene/prefab/tuning preservation is recorded separately.
- Live scripted render review advanced from frame 101 to 296, exercised the mixed encounter and both King phases, confirmed victory unlock, and returned to Wake after transfer. Reviewed the boss HUD, white/red tells, phase transition, defeat and transfer captures. These are staged presentation checks, not a human difficulty playtest.

See [Court HUD](Validation/phase3/court-hud.png), [mixed encounter](Validation/phase3/sentinel-lancer.png), and the machine-readable test, scene-audit and preservation reports in `Validation/`.

## Deliberate limits and remaining parity gaps

- Hit-stop is actor-local impact pause plus player pose hold, not the original global simulation freeze. Inputs remain live and animation never authorizes damage. Player combat windows run on the existing rendered-frame clock; enemy movement/patterns and projectiles use the fixed step.
- Charging does not apply the original .58 movement slowdown, preserving the requested responsive Phase 2 movement baseline.
- Needle, Crescent, Breaker, Echo identity skills, ORDER's 48-HP King variant, and acquisition-aged MERCY boss aid require the broader style/memory archive and remain deferred. Only RETURN/MERCY/EMBER are exposed.
- Enemy phases/positions reset on room entry; health/defeat persists. Regular deflects deliberately provide a fresh recovery state instead of resuming an interrupted attack after source stagger.
- The Court glass is a bounded progression adaptation; Cradle/Mother and the full world are not implemented.
- Dedicated player directional/charged/combo, hurt, wall, dash and death animation assets remain the gaps documented in PlayerPresentation.md. The King uses all eight existing source frames; no King sprite is missing.
- Automated/native-input checks and scripted render review do not replace first-time keyboard/controller difficulty playtests, physical-device latency assessment, standalone performance or audio-mix validation.

## Recommended Phase 4

First playtest and tune the Wake/Court combat loop, including telegraph readability and mixed-encounter spacing. Then add combat styles and the broader archive/loadout rules (capacity/forgetting, acquisition metadata, ORDER/DEFIANCE consequences) with save migrations. Only after those systems are stable, extend one bounded exploration segment toward Cradle/Lungs. Keep Mother and full-world expansion as later milestones.
