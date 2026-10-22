# ECHO//FALL Unity

A 2D action exploration game built around precise movement, readable combat, and choices that persist across lives. Explore interconnected ruins, face their inhabitants, and carry a memory forward when a life ends.

This repository contains the Unity port of the original JavaScript/Canvas version, reimplementing its gameplay in C# with native Unity scenes and systems.

## Development status

The current chapter contains thirteen connected rooms, three regular enemy archetypes, the King and Mother, the noncombat Child, nine memory types and four replayable endings. Phase 7 adds build tooling, settings, keyboard remapping, audio transitions and stabilization. See [Phase 7 validation and known issues](Docs/Phase7-ReleasePolish.md) for the current build's acceptance status; automated checks do not replace a first-time controller playthrough.

## Implemented gameplay

The [Phase 8 opening](Docs/Phase8-OpeningExperience.md) adds a title screen, contextual field guide, an oath-bound practice encounter, and a clearer first choice/transfer/return sequence. The [experience audit](Docs/Phase8-ExperienceAudit.md) separates verified problems from remaining review, and the [identity guide](Docs/Phase8-IdentityGuide.md) establishes the art and narrative direction. Custom character acting and first-time player acceptance remain unfinished.

The [advanced presentation pass](Docs/PresentationPass.md) adds render-cadence camera following, player silhouette easing and menu feedback. The [presentation follow-up](Docs/FinishedPresentation.md) adds dedicated player poses, NPC and regional art, material footsteps, original synthesized music/ambience, broader text scaling and Echo dissolution, with validation and remaining acceptance work.

- Movement with variable-height jumps, wall jumps, dashes, and one-way platforms.
- Blade combos, charged and directional attacks, deflects, counters, projectile reflection, pogo strikes, and healing.
- Thirteen connected rooms across Wake, Court, Cradle, Last Garden and Choir, with optional routes, secrets, hazards and shortcuts.
- Death and voluntary transfer, nine persistent memory types with five retained slots, equipped abilities, forgetting, delayed consequences and NPC reactions.
- King prediction combat, Mother's recalled identities, Child dialogue, and RESET/BREAK/REMEMBER/SHARE endings.
- Signal-anchor checkpoint saves, run restoration, HUD, pause and memory menus, camera tracking, lighting, parallax, and procedural audio.

## Unity and technologies

- **Unity 6000.5.8f1** (Unity 6).
- C# and Unity Physics 2D.
- Universal Render Pipeline (URP) with 2D lighting and sprite rendering.
- Unity Input System for keyboard and controller bindings.
- Unity UI, JSON save data, and Unity Test Framework.

## Open and run

1. Install **Unity 6000.5.8f1** through Unity Hub.
2. In Unity Hub, add this repository's root folder as an existing project and open it with that editor version.
3. Wait for asset import, package resolution, and script compilation to finish.
4. Open `Assets/EchoFall/Scenes/WakeSlice.unity` and press **Play**.

Use **A/D** to move, **Space** to jump, **K/Shift** to dash, **J/X** to strike, **F** to guard, **E** to interact, and **Escape** for controls and pause. For isolated movement practice, open `Assets/EchoFall/Scenes/MovementLab.unity`.

Hold/release strike for a charged attack; **W/S** aims directional attacks. Tap **F** to deflect white attacks; hold at least .48 seconds and release to counter red attacks. **Q** imprints/detonates, **H** holds mend, **C/L** uses the active memory, **M** opens the map, and **R** offers voluntary transfer. Controller: left stick movement/aim, A jump, B dash, X strike, Y interact, LB guard, RB imprint, LT memory, RT mend, Start pause, Back transfer, right-stick press map. Menus support number keys, mouse and D-pad/A navigation.

Pause > Settings controls volume, music/SFX balance, shake, hit flashes and larger dialogue, HUD and map text. Eight primary keyboard action bindings can be replaced; movement, menu and controller bindings remain fixed. Rest still for .45 seconds at a signal anchor to save. Reload restores that anchor; later unsaved exploration is lost. Death/transfer clears the run checkpoint while preserving the archive. Ending discoveries persist across future loops.

## Windows build

With Unity **6000.5.8f1** and Windows Build Support installed, use **Echo-Fall > Build Windows Playtest**. The output is `Builds/Windows/Echo-Fall.exe` plus its accompanying data and runtime files; distribute the entire folder. The build is version `0.7.0`, windowed and resizable. Its explicit scene list excludes MovementLab and SampleScene. The QA build menu produces a separate development build; use the release folder for playtesting.

Windows saves remain in `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Echo-Fall/`: `wake-slice-v1.json` for the archive and `settings-v1.json` for preferences. Keep `.bak` files when diagnosing recovery. Builds, logs, Library and Temp are ignored by Git.

Run the complete EditMode and PlayMode suites in Unity Test Runner. Reproducible validation tools and reports live in `Tools/` and `Docs/Validation/phase7/`. The development-only standalone driver uses an isolated archive and is excluded from the release executable.

## Known limitations

Human approval of the authored synthesized audio/music mix, controller rebinding, hold/toggle alternatives and localization remain pending. Dynamic abandoned-body placement and combat styles are deferred parity work. Physical controller feel, subjective balance and an uninterrupted human release-player run still need external QA. macOS/Linux have not been certified. Dedicated player/NPC art and regional presentation assets are now supplied; see the follow-up report for their provenance and validation limits.

## Original browser version

The original JavaScript/Canvas game is maintained separately. This repository is dedicated to the Unity port; browser source, builds, and their development workflow remain in the original project.
