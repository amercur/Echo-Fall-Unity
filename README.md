# ECHO//FALL Unity

A 2D action exploration game built around precise movement, readable combat, and choices that persist across lives. Explore interconnected ruins, face their inhabitants, and carry a memory forward when a life ends.

This repository contains the Unity port of the original JavaScript/Canvas version, reimplementing its gameplay in C# with native Unity scenes and systems.

## Development status

In active development. The movement lab and first five-room Wake vertical slice are implemented, including combat, the initial memory loop, checkpoint saves, and memory selection. EditMode and PlayMode validation is documented in [the Wake slice notes](Docs/Phase2-WakeSlice.md). Full feature parity, broader content, physical controller testing, and standalone build validation remain in progress.

## Implemented gameplay

- Movement with variable-height jumps, wall jumps, dashes, and one-way platforms.
- Blade combos, charged and directional attacks, deflects, counters, projectile reflection, pogo strikes, and healing.
- Five connected rooms: Wake, Pilgrim Causeway, Belfry, Cistern, and Glass Archive, with enemies, hazards, collectibles, dialogue, and shortcuts.
- Death and voluntary transfer, persistent RETURN/MERCY/EMBER memories, equipped abilities, and memory-driven world changes.
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

## Original browser version

The original JavaScript/Canvas game is maintained separately. This repository is dedicated to the Unity port; browser source, builds, and their development workflow remain in the original project.
