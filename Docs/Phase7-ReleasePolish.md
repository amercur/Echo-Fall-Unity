# Phase 7: playable-build stabilization

Follow-up: [advanced presentation pass](PresentationPass.md) records the subsequent camera/animation/UI/audio changes and its separate Windows candidate and validation.

Phase 6 remains the gameplay baseline. No new region, boss, combat style, movement tuning or ending condition is introduced. The original JavaScript project remains read-only.

## Dependencies and scope

Graphify traced the player/combat/session/archive/UI/camera/audio dependencies before edits. `SliceSession` is the main integration point: it owns screen state, room transitions and save boundaries. Settings remain a separate preferences file; they cannot rewrite an archive. Boss discovery was repeatedly searching the scene from HUD and audio, and nearest-interaction discovery allocated a component array every frame. Those paths now use lifecycle-managed boss references and a room-local interaction cache. Dynamic remembered geometry continues to rebuild through the existing room application path.

The original `game.js` audio/settings routines and `art-review/wake.png` were reviewed. The original uses restrained synthesized cues and volume/shake preferences. Unity retains the established architecture, Wanderer frames, enemy telegraphs and responsive animation ownership. No animation delay or global hit-stop was introduced.

## Changes

- Pause > Settings exposes master/music/SFX volume, camera shake, hit-flash strength and larger modal text. Levels cycle through bounded values, including zero. Telegraph colors remain readable with hit flashes disabled.
- Primary keyboard bindings can be changed for jump, dash, attack, guard, imprint, mend, memory and interaction. Movement/aim, menu keys, alternate keys and controller bindings remain fixed. Conflicting/reserved keys are rejected. HUD action prompts and the pause controls text use the selected keys. Escape cancels capture.
- Settings save independently with atomic replacement and backup recovery. A failed write retains the session's applied setting and reports the failure. Existing archive paths and checkpoint semantics are preserved.
- Losing application focus pauses active gameplay. The settings menu includes an explicit quit confirmation explaining the last-anchor restore boundary.
- Audio has six bounded SFX voices, separate master/music/SFX gains, quieter locomotion cues and a two-source music crossfade. Exploration, combat, bosses, extraction and endings have distinct synthesized placeholders. Pausing and ordinary room changes preserve the music transport.
- An optional `Resources/EchoAudio` AudioPalette asset replaces individual cues and music without changing gameplay code. Authored final music and sound design remain pending.
- Dead and refusing recalled Echoes are destroyed instead of accumulating inactive actors through a long Mother encounter. Persistent room enemies retain their existing defeat/save behavior.
- Fixed the atmosphere shader's missing URP per-sprite color multiplication. The old shader rendered colored, translucent mist as opaque white, washing out the Wake. The corrected standalone capture restores the intended dark teal separation without changing authored scene geometry.
- The release builder explicitly includes WakeSlice and all thirteen destinations, excluding MovementLab and SampleScene. Development scenes remain available to the Editor regression suite. Release builds exclude the opt-in standalone QA driver.

## Build and validation

Use **Echo-Fall > Build Windows Playtest** for `Builds/Windows/Echo-Fall.exe`; use **Build Windows QA** for the instrumented development build. Version is `0.7.0`, windowed 1600x900 and resizable. The company identifier stays unchanged to preserve the existing Windows save directory. Builds and Unity caches remain ignored by Git.

Evidence is in `Docs/Validation/phase7/`:

- **62/62 EditMode** and **55/55 PlayMode** tests passed. New regressions cover settings backup recovery, bounded preferences, 250 archive transfers with replacement/forgetting, release scene inclusion, music continuity, zero-shake behavior and repeated recalled-Echo cleanup. Existing movement/combat, all room gates, anchors, memory consequences and ending tests remain included.
- **45/45 original reference tests** and **37 preservation comparisons** passed. All authored scenes, movement tuning/motor, combat rules, ending eligibility and save DTOs remain unchanged.
- The first PlayMode run caught a new boss-cache regression: disabling an AI component hid its still-present actor. The cache now checks the GameObject's presence rather than component enablement; the entire suite passed afterward.
- Final menu review verified nine settings choices, larger text, binding labels and a rebound jump through the Input System. Settings, bindings, BREAK and SHARE captures were visually inspected; final captures and `menu-review.json` supersede the interrupted first-build recapture.
- The rendered Windows QA player (`0.7.0`) passed isolated movement simulation, Wake anchor reload, Cradle/Garden saves, staged King/Mother victories, Child permission, BREAK, SHARE and archive validation. A fresh process restored the saved ending and both discoveries, then returned to Wake through extraction. These are controlled API-driven sequences, not uninterrupted human combat playthroughs.
- Hidden-window checks passed logic but produced black captures and artificially short frame timings. They are retained as diagnostic attempts and excluded from rendering/performance conclusions. The visible run and its `standalone-rendered/runtime.json` and `restart.json` are the valid standalone evidence.

The standalone driver initially used rendered-frame waits for anchor rest; this was shorter than .45 seconds when the hidden player ran uncapped. It now waits for grounded state and actual rest completion. This was a validation-driver defect, not evidence of a gameplay save failure.

## Performance findings

Visible development player at 1280x720 on AMD Radeon (TM) Graphics, 120 short samples per room:

| Scene | Median frame | 95th percentile |
|---|---:|---:|
| Procession | 16.668 ms | 16.684 ms |
| Lungs | 16.668 ms | 16.686 ms |
| Mother with three recalls | 16.669 ms | 16.683 ms |
| Choir | 16.669 ms | 16.688 ms |

Unity reported approximately 185–186 MB allocated across these scenes. Net managed-heap changes were small and sometimes negative after collection; they are **not** per-frame allocation measurements. These short, VSync-limited samples show stable 60 Hz pacing on this machine, not a long-duration stress test, a GPU/CPU headroom measurement or minimum-spec certification. The cache changes remove repeated hierarchy searches, while recalled-Echo cleanup bounds a demonstrable long-encounter accumulation problem. No speculative movement/physics rewrite was made.

Both Windows builds succeeded with zero errors and three warnings. The release report totals 133,054,803 bytes; QA totals 213,653,528 bytes. Package warnings concern unlinked optional Unity Services, intentionally disabled player Pipeline, and an unsupported System.Windows.Forms reference originating in System.ServiceProcess.dll. A ComputeBuffer disposal warning occurs at player shutdown; no Echo-Fall gameplay script allocates a ComputeBuffer. These package/render-pipeline cleanup warnings remain tracked for release engineering rather than being hidden as clean logs.

The release executable launched outside Unity, responded as an Echo-Fall window and closed normally. Its assembly was inspected to confirm the QA driver is absent. Distribute the entire `Builds/EchoFall-0.7.0-Windows.zip`, extracting before running `Windows/Echo-Fall.exe`. The archive checksum is recorded in `Docs/Validation/phase7/package.json`. Release launch evidence is in `release-launch.json`; restart/save coverage uses the separate QA executable, not a claimed manual release playthrough.

`StandaloneQA` is opt-in via `-echo-qa <report-directory>` on a development player. It uses a separate archive, tests movement simulation, anchors/reload, representative room frames, staged King/Mother/Child completion and BREAK/SHARE, then saves an ending for a separate process-restart check (`-echo-qa-resume`). It invokes encounter APIs for controlled coverage: it is not a human full-difficulty playthrough or proof of physical controller feel. The release player contains no such driver.

## Known limits and acceptance

The subsequent [finished presentation follow-up](FinishedPresentation.md) supplies dedicated NPC/player art, original synthesized score/effects, regional ambience, material footfalls, map/HUD text scaling and Echo effects. Its report supersedes the corresponding historical asset gaps below; human feel/audio acceptance and platform certification remain separate.

Status: **ready for limited external playtesting; Phase 7 is not fully complete**. The packaged candidate is suitable for collecting traversal, combat, controller and balance feedback. Full release acceptance still requires uninterrupted human playthroughs, subjective audio evaluation and longer performance/soak testing. Preserve the remaining warnings in the QA checklist.

Physical keyboard/controller playthroughs, subjective combat/audio balance and a full uninterrupted release-player run remain distinct acceptance checks. Automated regression and staged standalone progression must not be described as completing them.

Dedicated NPC art, bespoke directional player clips, authored score/mix, localization, controller rebinding, hold/toggle charge alternatives and dynamic abandoned bodies remain deferred. Existing text dialogue is always visible; there is no voiced dialogue requiring a separate subtitle track. Larger text currently applies to modal dialogue/options, not every HUD or map label.

macOS and Linux require their Unity platform modules, platform-specific builds, filesystem/save-path checks, input/audio testing and signing/notarization where applicable. Windows validation takes priority; neither platform is certified here.
