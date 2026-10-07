# Wanderer player presentation

The player uses the original Echo-Fall `assets/wanderer-smooth.png` atlas and its manifest: forty 192-pixel frames, four columns, source foot anchor `(96, 161.529)`, Unity pivot `(96, 30.471)`, 100 pixels per unit, and .34 scale. The existing uncompressed, bilinear import is retained.

`PlayerVisual` now owns sprite selection, tint, and visual-child transforms in both MovementLab and WakeSlice. `SlicePlayerPresentation` remains a compatibility binding for authored scenes. Combat reports accepted actions and impacts; animation never authorizes actions or delays input. The motor only gains read-only wall-jump and teleport sequence counters. Movement constants, collider dimensions, authored rooms, and combat damage/recovery rules are unchanged by this presentation pass.

| State | Source frames / treatment |
| --- | --- |
| Idle | 0–3, breathing loop |
| Run | 4–19, playback follows horizontal speed |
| Jump rise / fall | 20 / 21, selected from vertical velocity |
| Landing | 21, foot-anchored compression up to 10% for .16 seconds, small dust burst |
| Wall slide / jump | 31 / 20, restrained lean/stretch and dust |
| Dash | 21, small stretch/lean and five pooled fading afterimages |
| Attack | 22–29 over the existing recovery interval; directional blade arc, small combo/charged lean |
| Guard / deflect | 30 / 31, successful deflect flash and motes |
| Hurt | 31 briefly, red tint/blink and camera impulse |
| Bench rest | 32–39 while the existing rest transaction completes |
| Fatal death | 34–39, compression, lean, red fade |
| Voluntary transfer | 30–31, cyan lift/fade and motes |

Strong melee impacts, deflects, and imprint detonation briefly hold the **player pose only**, capped at 55 ms. Simulation and input remain live; new attacks, jumps, and dashes can interrupt that hold. This deliberately does not introduce global time-scale hit-stop or claim full original combat hit-stop parity.

Camera impulses last .16 seconds, are capped at .065 world units, and affect the render matrix only. They are clamped to the room bounds and reset after rendering, leaving the follow camera transform untouched. Transient effects expire in menus; teleport/rest/new-life boundaries clear player tint, pose timers, ghosts, and impulses. Death/transfer presentation does not delay the existing choice screen or next life.

## Asset gaps

The original atlas supplies idle, run, two airborne poses, one attack sequence, two guard poses, and rest. There are **no dedicated authored clips** for landing, wall slide, wall jump, dash, hurt, fatal death, or transfer. These states currently adapt the frames above. Separate rising/falling animation sequences, directional up/down strikes, charged attacks, and distinct combo strikes also remain art gaps. Dust, blade arcs, and afterimages are procedural and need no missing texture to function.

## Verification

Evidence is stored under `Docs/Validation/player-presentation*`. Automated coverage includes MovementLab pose transitions and collider preservation, five-room attack/guard/hurt/dash presentation, real charged-hit feedback with camera-transform preservation, immediate transfer/new-life transitions, and the existing native-input, checkpoint, completion, and movement-trajectory regressions.

Final verification on 2026-10-06: **35/35 EditMode and 23/23 PlayMode tests passed**. Inspected the [pose sheet](Validation/player-presentation/pose-sheet.png) and wide views of MovementLab, Wake, Belfry, Cistern, Archive, and Procession. The final live capture advanced from frame 179 to 672. Wide images preserve the Editor Game view's actual aspect ratio (1280×464 in this session); close-ups are 480×480 and exclude screen-space UI. An initial wide-capture aspect mismatch was corrected before saving final evidence.

The nine previously recorded original source hashes remain unchanged; the imported Wanderer PNG also matches its original SHA-256 (`4033d3a19057f7e80fdbae0212fe5594f6ad1de31806ddc0170f9a8e1262c548`). No new authored scene, prefab, collision, or movement-tuning changes were made. Unity was left stopped on WakeSlice with `runInBackground` false. Existing Unity Services/IAP authentication errors appeared during Editor startup; no presentation or test errors were reported.

`Tools/PlayerPresentationReview.cs` captures actual Unity camera renders using an ephemeral session. Close-ups deliberately use a temporary review camera; wide captures use the gameplay camera framing. This review is programmatic, not a timed human controller playthrough.

Graphify was queried before implementation to trace `PlayerVisual`, `SlicePlayerPresentation`, `PlayerMotor`, `SliceCombat`, `SliceEffects`, and `RoomCamera`. AST refresh updates code relationships; older semantic roadmap nodes and serialized scene/asset coverage still require source verification.
