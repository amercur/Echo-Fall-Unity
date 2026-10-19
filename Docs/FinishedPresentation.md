# Finished presentation follow-up

This continues the advanced presentation pass without adding regions, encounters or progression systems. Gameplay remains authoritative: visuals never delay input, authorize hits or change collision geometry. Nothing has been committed or pushed.

## Delivered assets and behavior

- 72 new Wanderer frames across 14 clips: rise, fall, land, wall slide, wall jump, dash, hurt, death, transfer, upward strike, downward strike, charged strike, combo two and combo three. Idle, run, first strike, guard and rest retain the working source atlas. Animation selection remains immediate; existing movement tuning and attack windows are unchanged.
- Eight rendered props/characters: Keeper, Child, wounded creature, branching tree, suspended vessel, closed cage, open cage and Observatory plinth. Regional art uses existing parallax and restrained motion. Decoration adds no colliders. Remembered sacrifice opens the pictured cages; betrayal retains the existing conflicting consequence.
- 28 original synthesized WAV assets: 19 effects, five musical contexts and four regional ambience loops. All 30 AudioPalette references are populated (two intentionally share clips). Stone, metal, wood and moss footsteps follow surface metadata. Region ambience crossfades and does not restart on same-region transitions. Existing music contexts and six bounded SFX voices remain.
- Map fade-in/current-location emphasis, larger map and HUD text, and a two-line larger integrity/Resonance display. Selection remains usable during fades.
- Short shader-based death/Echo dissolution, gradual Echo recognition appearance and a disappearing remnant when an Echo refuses help. These remain visual reactions to the existing memory rules.
- Environmental motion uses accumulated active time, preventing a pause/resume phase jump.
- Standalone profiling now samples 600 frames per representative room and records the available per-frame GC allocation counter rather than relying on net managed-memory differences.

## Before / after

| Previously unfinished | Current implementation |
|---|---|
| Landing, wall, aerial and heavy attacks reused unrelated poses | Dedicated model-rendered clips with matching pivots |
| Schematic NPC markers, cages, branches and plinth | Matching low-poly rendered figures and scenery |
| Generic generated tones and shared footstep sound | Replaceable composed loops, distinct cues and material footfalls |
| Abrupt map appearance and partial text scaling | Short nonblocking fade, current-room emphasis and broader large-text coverage |
| Echoes simply faded/vanished | Brief dissolution and recognition transitions |
| Short timing samples and net memory deltas | Longer samples plus GC counter reporting where supported |

## Asset provenance and reproduction

The original JavaScript project is read-only. `Tools/ArtSource/echo-fall-reference.blend` is a workspace copy of its model source. `Tools/finish_presentation_art.py` samples that rig and authors the extra poses and prop meshes in memory; it does not save over the original Blender project. Run it with Blender 5.2 in background mode, then run `ImportFinishedPresentation.Run` through Unity Pipeline to restore sprite pivots, PPU and AudioPalette references. `Tools/compose_presentation_audio.py` reproducibly generates the sound library without third-party samples. Resource manifests record frame counts, audio lengths, peaks and RMS levels.

These are original synthesized sound assets, not recorded foley or a commissioned final soundtrack. Listening and musical direction approval still require human review; replacing any clip requires no gameplay-code changes.

## Validation and acceptance

- 65/65 EditMode tests passed, including all animation resource/pivot and audio/shader reference checks.
- 58/58 PlayMode tests passed in 234.17 seconds, including material footsteps, collider-free decoration, remembered cages and Echo cleanup, plus all existing gameplay/progression coverage.
- 45/45 original JavaScript tests passed read-only; 37/37 source/gameplay preservation comparisons passed.
- Captured all thirteen rooms, player poses in MovementLab and the Wake loop, large HUD/map and sacrifice-dependent cages. Reviewed captures for presentation and readability; this is not a hands-on controller playthrough.
- Fixed a runtime issue found by the first PlayMode run: MaterialPropertyBlock creation must occur in Awake, not a MonoBehaviour field initializer. The first run was cancelled; the subsequent complete suite passed. A screenshot-helper bootstrap race was also corrected without gameplay changes.
- Windows QA and release builds succeeded with zero errors and three previously tracked package/configuration warnings each. The standalone driver passed movement simulation, anchor save/reload, staged King/Mother/Child progression and BREAK/SHARE endings. A fresh process restored both discoveries and returned to Wake for replay with a valid archive. It uses an isolated archive and controlled encounter APIs, not a claimed human difficulty playthrough.
- The non-development release executable launched as a responsive Echo-Fall window outside Unity. The complete distributable is `Builds/EchoFall-0.7.0-Finished-Presentation-Windows.zip`; extract it and run `Windows/Echo-Fall.exe`. Checksum and launch/exit evidence are in `Docs/Validation/finished/package.json` and `release-launch.json`.
- Captures and reports are stored under `Docs/Validation/finished/` and `Docs/Validation/presentation/finished/`. Graphify's code graph was updated after the implementation changes.

## Standalone performance

| Room | Frames | Median | p95 | GC median / p95 |
|---|---:|---:|---:|---:|
| Procession | 600 | 20.000 ms | 20.020 ms | 3,448 / 3,736 B |
| Lungs | 600 | 19.998 ms | 20.071 ms | 3,444 / 3,444 B |
| Mother with recalled identities | 600 | 19.999 ms | 20.074 ms | 3,574 / 3,640 B |
| Choir | 600 | 19.999 ms | 20.068 ms | 3,420 / 3,420 B |

AMD Radeon integrated graphics; allocated memory was 183.3–185.6 MB. These are refresh-limited development-player samples, with the async QA driver's allocations included. The driver reported unreliable swap-chain timestamps and fell back to CPU timing. They do not establish GPU headroom, minimum-spec performance, allocation attribution or long-session stability. No speculative physics or renderer rewrite was made.

The existing shutdown ComputeBuffer disposal warning remains. Build warnings concern optional unlinked Unity Services, intentionally disabled player Pipeline and the System.Windows.Forms reference from the package/dependency chain. These are tracked limitations, not hidden as clean logs.

Human keyboard/controller feel, listening balance, long-session GPU profiling and an uninterrupted release-player playthrough remain acceptance work. Controlled API-driven encounters and screenshots do not establish those outcomes. No additional animation/sprite slot is intentionally left empty by this follow-up; existing source idle/run/guard/enemy/boss assets remain in use, rather than receiving a new animation set.

Status: ready for external playtesting of this presentation candidate, not final release certification. Controller rebinding, hold/toggle alternatives, localization, dynamic abandoned bodies and combat styles remain previously documented deferred work outside this presentation pass. No new region, boss, ending or major gameplay system was added. Original JavaScript files remain unchanged; no commits or pushes were made.
