# Advanced presentation pass

Phase 7 is the baseline. No new world, boss, mechanic or save schema. Original JavaScript remains read-only. Graphify and the connected Unity Pipeline Editor API are used for dependency inspection and validation (no separate Unity MCP connector is exposed in this session).

## Prioritized checklist

1. Camera/render coherence: camera currently updates on fixed steps while the Wanderer uses interpolated rendering. Follow at render cadence, preserve bounds and deterministic simulation API, verify multiple frame rates and teleport resets.
2. Player silhouette: smooth presentation-only leaning and compression recovery without blending sprite frames or delaying input. Preserve motor tuning, coyote/buffer windows and immediate attack authorization.
3. UI cohesion: consistent hover/controller focus, short interruptible appearance transitions, responsive resource feedback, eliminate repeated component lookups in modal updates. Keep actual health/resonance values immediate.
4. Audio identity: differentiate strong impacts, charged counters, imprint and transfers; support replaceable regional footfall/ambience assets. Avoid introducing unlicensed assets or new audio dependencies.
5. Memory identity: restrained modal accent and immediate extraction cues, no unskippable delay or consequence spoilers.
6. Review existing parallax, atmosphere, all room compositions and framing. Preserve authored geometry; only change evidenced issues. Dedicated NPC art, directional/charged player animation and original score remain asset gaps.
7. Validate live captures, relevant regressions, full suites and Windows standalone; distinguish staged verification from human feel/balance acceptance.

## Dependencies and baseline

PlayerVisual observes PlayerMotor.RenderPosition; RoomCamera currently follows PlayerMotor.Position in FixedUpdate. SliceParallax observes the camera in LateUpdate. PlayerCameraImpulse offsets only the rendering matrix. SliceHUD observes session screen/choice state; MemoryFlow owns choice navigation, persistence and rebuilding. SliceSound observes combat/session state and has bounded voices. None of the presentation changes should authorize damage, change run/archive state or move collision geometry.

Existing dust, trails, afterimages, landing squash, actor-local impact pause, region silhouettes/parallax and room fades are retained. Phase documentation records missing bespoke animation/NPC assets, procedural audio, limited physical-device validation and short VSync-limited performance samples. Those remain explicit acceptance gaps, not completed polish.

## Implemented changes

- RoomCamera follows interpolated feet in LateUpdate. Parallax explicitly runs afterward. Pause freezes follow; room/hazard snaps and room bounds remain authoritative. The deterministic camera simulation API remains available for trajectory parity tests. No additional smoothing stage or physics latency is introduced.
- Wanderer lean follows actual horizontal velocity and eases over approximately 30 ms; squash/lean transitions ease independently from immediate sprite selection. Teleports clear the visual easing state. Movement/coyote/buffering values and combat authorization are unchanged.
- Choice rows share restrained hover/focus color and scale feedback. Modals fade from 65% to full opacity in 120 ms without disabling interaction. Transfer uses a cyan accent. Health/Resonance retain immediate numerical updates with a short settings-aware color response. Choice components are cached instead of searched for every modal frame.
- Added distinct synthesized heavy impact, counter, imprint, detonation, focus and transfer cues. AudioPalette exposes authored replacements; all use the existing bounded six-voice SFX mix. Extraction audio fires once on entering transfer, not each replacement/confirmation panel. Existing music crossfades remain intact.

## Before / after

| Before | After |
|---|---|
| Interpolated player against a fixed-step camera | Camera and parallax follow the rendered player at render cadence |
| Instant silhouette rotation/scale changes | Short visual-only easing, immediate animation/gameplay changes |
| Static choice rows and hard modal appearance | Unified pointer/controller focus and interruptible fade-in |
| Counter, imprint and extraction reuse generic cues | Distinct replaceable cues and extraction accent |

## Remaining presentation work

Live review confirms distinct existing silhouettes and clear traversable surfaces across thirteen rooms. Cradle's circles/cages, Garden's branches, Observatory's flat central block and the Child marker still look schematic beside the architecture kit; these need original art and composition work. Existing scenery is retained in this increment rather than claiming a completed environment overhaul.

Dedicated authored landing/wall/dash/hurt/death/transfer clips, rising/falling sequences, directional/charged/distinct combo clips and Keeper/Child/creature art remain missing. Existing atlas adaptations are preserved. Final score, original sound design, physical material-specific footsteps and region-specific authored ambience are still needed; this pass does not relabel synthesized tones as final sound assets. Map animation, full HUD scaling, further Echo dissolve effects and new scenery/lighting are not yet implemented in this pass.

Human keyboard/controller evaluation must assess camera lag, turning/landing feel, telegraph clarity, competing SFX, menu focus and busy Mother encounters. Static screenshots and controlled API-driven progression do not establish those subjective outcomes. Full-scene GPU profiling, per-frame allocation attribution, long-session performance and cross-device testing remain separate from short standalone frame samples.

## Validation

- 63/63 EditMode tests passed, including camera convergence at 30/60/144 steps per second and all prior trajectory/bounds checks.
- 56/56 PlayMode tests passed (221.09 seconds), including immediate modal choice handling and previous combat, boss, room, memory, save and movement coverage. An earlier run was interrupted by the Editor closing and is not counted.
- 45/45 read-only JavaScript reference tests and 37 baseline preservation comparisons passed.
- Live room capture covered all thirteen rooms, settings and extraction. The first attempt overlapped an in-flight bootstrap room; later captures assert room identity, wait for rendering and replace that invalid set. The review tool now waits for existing bootstrap loads and rejects multiple SliceRoom instances.
- MovementLab and the five Wake rooms were captured using the existing pose-review tool. This exercises animation states, attacks, guard, deflect, hurt and transfer without writing the archive. Pose capture emitted duplicate-global-light warnings while staging scenes; these are recorded in the review console, not silently treated as a clean run. Fresh standalone logs determine whether normal boot reproduces them.
- No C# compilation errors. Unity Purchasing emitted an unrelated Editor authentication error at startup; the game uses no purchasing services.

Windows QA build succeeded with zero errors and three existing package warnings. The visible standalone passed movement simulation, Wake checkpoint reload, Cradle/Garden anchors, staged King/Mother/Child progression, BREAK and SHARE. A separate process restored both discoveries and returned to Wake through extraction. No duplicate global-light warnings or gameplay exceptions appeared in that standalone log; the existing ComputeBuffer disposal warning remains at shutdown.

At 1280x720 on AMD Radeon (TM) Graphics, the display was independently reported at **50 Hz**. Short 120-sample measurements:

| Room | Median | p95 |
|---|---:|---:|
| Procession | 20.000 ms | 20.019 ms |
| Lungs | 19.991 ms | 20.084 ms |
| Mother with recalls | 20.000 ms | 20.268 ms |
| Choir | 20.000 ms | 20.358 ms |

Allocated memory remained approximately 186 MB. These show pacing near this display's refresh interval, not a comparison against Phase 7's 60 Hz run or measured CPU/GPU headroom. The driver logged unreliable swap-chain timestamps and switched to CPU timing. Net managed-memory deltas are not per-frame allocation measurements. No broad performance rewrite is justified by these samples.

Evidence and captures: `Docs/Validation/presentation/`. No changes were committed or pushed.

The non-development Windows build also succeeded (zero errors, three existing warnings), launched as a responsive Echo-Fall window and exited normally. The complete candidate is `Builds/EchoFall-0.7.0-Presentation-Windows.zip`; extract it before running `Windows/Echo-Fall.exe`. SHA-256 is in `package.json`. This is an incremental external-playtest candidate, **not a claim that the entire advanced presentation checklist or human playtesting is complete**. Next priority is dedicated environment/NPC art, followed by authored material/region audio and physical-device feel review.
