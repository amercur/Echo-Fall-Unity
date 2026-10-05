# Phase 1: movement lab

Implemented for **Unity 6000.5.8f1 (5cb7df797b7d)**, as recorded in `ProjectSettings/ProjectVersion.txt`. The initialized project has `Assets`, resolved packages in `Packages/packages-lock.json`, `ProjectSettings`, `Library`, `Logs`, and `UserSettings`. Its existing URP 2D settings and SampleScene are retained. Installed package declarations include Input System 1.20.0, URP 17.6.0, Unity UI 2.5.0 and Test Framework 1.7.0.

**Status (2026-10-04): implemented and verified in the live Unity Editor. All 26 EditMode and 4 PlayMode tests pass.** The original JavaScript suite also passes all 45 tests. Phase 1 still needs hands-on keyboard/controller feel review, sustained physical 144 Hz rendering, and a standalone Windows build/launch. No Phase 2 work has begun. The earlier batch-license failure is historical; the interactive Editor integration now works.

## Open and play

Open `Assets/EchoFall/Scenes/MovementLab.unity` and press Play. After scripts import, **Echo Fall > Open Movement Lab** opens the same scene. It is the first scene in Build Settings; SampleScene remains listed after it.

| Action | Keyboard | Controller |
|---|---|---|
| Move | A/D or left/right arrows | Left stick or D-pad |
| Jump / wall jump | Space | South / A |
| Short jump | Release Jump early | Release Jump early |
| Dash | K or either Shift | East / B |
| Drop through a platform | Down + Jump | Stick/D-pad down + A |
| Restart at the initial spawn | R | View / Select |

Up/W does not jump. Dash direction follows facing. Only one airborne dash is available until landing or wall jumping. There is no double jump in this phase.

The course runs left to right through variable-height jumps, three one-way platforms, a solid wall, a low ceiling, a hazard gap, a tall climbing shaft and descending platforms. The shaft uses the original Belfry wall/entry/summit dimensions, shifted horizontally into the lab. Reach its summit through wall jumps and an air dash. Pink hazards and out-of-bounds falls return to safe footing; this phase has no health, death, combat, memory or save systems.

## Files and responsibilities

| Asset | Purpose |
|---|---|
| `Assets/EchoFall/Scenes/MovementLab.unity` | Authored native scene with BoxCollider2D course geometry, camera, player prefab instance and diagnostic labels |
| `Assets/EchoFall/Prefabs/Player.prefab` | Kinematic Rigidbody2D, BoxCollider2D, input, motor and animated SpriteRenderer child |
| `Assets/EchoFall/Data/MovementTuning.asset` | Inspector-editable values converted from original pixels to Unity units |
| `Assets/EchoFall/Input/Movement.inputactions` | Move, Jump, Dash and Reset actions; keyboard/controller bindings |
| `Assets/EchoFall/Scripts/PlayerMotor.cs` | Fixed-step movement, buffered/coyote jumps, wall traversal, dashes, collision queries, one-way platforms and safe reset |
| `Assets/EchoFall/Scripts/MovementInput.cs` | Input System events buffered until one physics-step consumption |
| `Assets/EchoFall/Scripts/RoomCamera.cs` | Fixed-step exponential follow after the motor, horizontal look-ahead, vertical tracking and viewport-aware room clamps |
| `Assets/EchoFall/Scripts/PlayerVisual.cs` | Render interpolation and idle/run/jump sprite selection, independent of collision state |
| `Assets/EchoFall/Scripts/MovementLabHUD.cs`, `LabMarker.cs` | Minimal controls/status and course labels |
| `Assets/EchoFall/Editor/MovementLabBuilder.cs` | Native editor authoring helper and scene-open menu; creation refuses to replace an existing scene |
| `Assets/EchoFall/Tests/Editor/MovementTests.cs` | 18 EditMode cases using Unity Physics2D queries, including the actual shaft, room limits, prefab/material references and camera bounds |
| `Assets/EchoFall/Tests/Editor/ReferenceTrajectoryTests.cs` | Eight per-tick movement/collision/camera comparisons against captured original-game trajectories |
| `Assets/EchoFall/Tests/PlayMode/MovementPlayTests.cs` | Four runtime tests: native input/scene integration, input edge consumption, the Belfry climb with camera follow, and movement at three render targets |
| `Tools/capture_movement_reference.cjs`, `Docs/Validation/movement-reference.json` | Read-only original-harness capture and 1,430 reference simulation ticks |

The scene is already authored; no generator needs to run on first Play. Runtime-created content is limited to diagnostic text/font setup. Geometry, collision objects, sprite slicing, the player prefab, input actions and tuning are saved assets.

## Movement conventions

- **100 original game pixels = 1 Unity unit.** Root position is the foot center; Unity Y is upward. Collider size is 0.22 × 0.40 units, offset upward by 0.20.
- Fixed timestep is **1/120 seconds** and the maximum catch-up interval is 0.10 seconds. Both are stored in TimeManager. The motor uses explicit axis-by-axis BoxCast queries against native colliders and writes the resolved position to a kinematic Rigidbody2D. Sprite interpolation is separate; Rigidbody interpolation is disabled to avoid applying interpolation twice.
- Base speed is 2.65 units/s; ground/air acceleration is 22/12.5 units/s². Gravity is 14.5 units/s²; fall speed caps at 8.3. Jump launch is 6, release caps upward speed at 2.2.
- Jump buffer is 0.14 seconds, coyote time 0.105, wall grace 0.11, wall push lock 0.055. Wall steering uses stronger acceleration for 0.35 seconds to support repeated jumps on the same wall.
- Dash speed is 7.2 units/s, duration 0.15 seconds, cooldown 0.58, input buffer 0.12. Wall collision terminates a dash. One airborne dash is refreshed by landing or wall jumping.
- Dedicated layers are `EchoSolid`, `EchoPlatform`, `EchoHazard`, and `EchoPlayer`. One-way and hazard colliders are triggers; the motor explicitly includes them in relevant queries. One-way platforms only block descending feet above their top, and are ignored for 0.23 seconds after drop-through. No global collision toggles are used.
- Room bounds are 32 units wide and 12 high, beginning at Y = -0.88. The reference camera is orthographic size 2.7, matching a 540-pixel viewport. Camera clamps account for current aspect ratio.
- Only `wanderer-smooth.png` and its JSON manifest were copied from the original. The 40 atlas frames are sliced at 192 × 192 with the converted foot pivot. Sprites use 100 PPU and a 0.34 visual scale, applied once. Idle/run/jump are the only used animation states.

Two small defensive adaptations are deliberate: a press and release between physics ticks still produces a short jump, and safe reset positions require support under both sides of the feet so a pit-edge recovery cannot immediately fall again. Remaining differences in collision feel must be evaluated in Unity before Phase 1 is accepted.

## Verification and remaining acceptance

Completed outside the editor:

- All four assemblies (runtime, editor helper, EditMode tests, PlayMode tests) compile against installed Unity/package assemblies, with compiler warnings treated as errors.
- Structural validation checks native asset IDs, GUID references, assembly definition names, atlas dimensions/frame references, input bindings, timestep, build scene registration and copied-asset hashes.
- The original `game.js`, sprite sheet and JSON manifest still match their recorded hashes. No command writes to the original project.

Live Editor results are recorded in `Docs/Validation/editmode-results.json` and `playmode-results.json`:

- **26/26 EditMode cases pass.** Coverage includes acceleration/braking, short/held jumps, coyote and buffered jumps, same-wall climbing on both sides, wall-slide speed, ceiling/thin-wall collision, one-way/drop-through behavior, one air dash and cooldown buffering, hazards, room limits, camera bounds at 4:3/16:9/21:9, atlas pivots and the authored Belfry route. The scene also has exactly one connected player, valid sprite/material references, no missing scripts/prefabs and correctly linked camera/HUD.
- **Eight of those tests compare 1,430 source ticks**, captured by executing the unchanged original `game.js` in its in-memory harness: run/brake, held jump, short jump, jump/dash, platform/drop, thin wall, ceiling, and camera travel. Every sampled position, velocity and camera coordinate is within 0.001 Unity units (0.1 source pixels); grounded/air-dash flags match exactly. Fixtures include the source SHA-256. These are matched synthetic collision fixtures, not a claim that all original rooms were imported.
- **4/4 PlayMode cases pass.** Real Input System events drive movement/jump/dash/reset and consume each edge once. The saved Belfry shaft is climbed during live FixedUpdate simulation with camera tracking. A 120-step movement/jump/dash sequence ends at the same position for 30/60/144 render targets.
- Render-target checks do not certify monitor frame pacing. One observed run rendered 29, 60 and 62 frames respectively during those approximately one-second trials, so sustained physical 144 Hz remains unverified. The separate accumulator test exercises exact simulated 30/60/144 frame schedules.
- The original suite passes **45/45**, recorded in `Docs/Validation/original-tests.txt`. Source and copied-atlas hashes still match. No original files were modified.
- Final visual evidence is `Docs/Validation/MovementLab-play.png`; the course renders with its intended material colors and the camera frames the room. `scene-verification.json` records the saved scene state. The final live console check reported zero errors/warnings and no compile failures. Play Mode was exited and the temporary background-running setting restored.

Repairs made while preserving the existing scene:

1. Restored the corrupt player prefab through Unity's API with its original GUID, preserved tuning/input/40-frame visual setup, and reconnected the scene player at `(1.21, 0.02, 0)` with camera/HUD references. Unity removed the orphaned placeholder components. This preceded the continuation tests.
2. Assigned the existing `MovementUnlit` material to 19 course SpriteRenderers whose references were missing. Collider geometry and layout were preserved.
3. Restored the camera's zero-size serialized bounds to `(0, -0.88, 32, 12)`. Follow now runs at the fixed simulation rate after the motor, matching source timing, look-ahead and entry/reset anchors.
4. Resolved axis-aligned box collisions at the actual collider faces instead of the Box2D cast contact radius, eliminating a roughly 0.005-unit floor/platform gap. Kept the source's inclusive dash endpoint tick. Converted top/bottom room limits from the original top-left player coordinates to foot-center coordinates.
5. Added source-trajectory, scene-reference, room-limit and live climb/render-target tests. Test-only synthetic input temporarily bypasses Editor focus filtering and restores the input settings afterward.

Remaining acceptance work:

1. Hands-on keyboard and physical controller feel review, especially short taps, repeated wall jumps and the full climb.
2. Sustained 144 Hz presentation/frame-pacing validation on suitable hardware; inspect camera edges at actual window sizes as well as the passing mathematical bounds tests.
3. Build and launch a Windows player. `MovementLabBuilder.BuildWindows` builds only this lab. The Phase 0 standalone build gate has not been claimed as passed.

Do not begin combat or the memory loop until these acceptance checks pass.

Reproduce compiler checks from the project root:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools\Compile-Movement.ps1
python Tools\validate_movement_assets.py
node Tools\capture_movement_reference.cjs C:\Users\sivas\echo-fall
unity command run_tests -- --mode editor --filter EchoFall.Movement
unity command run_tests -- --mode playmode --filter EchoFall.Movement --async_tests true
unity command test_status
```

The PowerShell execution-policy flag applies only to that process. Compiler outputs go under ignored `Library/Phase1Validation`. `Tools/author_movement_assets.py` records the historical offline fallback; do not rerun it to repair this scene. Maintain authored assets through Unity. The live test runner stalled during the original MCP session; after reopening the Editor, the Unity CLI/Pipeline commands completed the tests. No Pipeline package code or dependencies were changed.

## Source provenance

Original read-only source: `C:\Users\sivas\echo-fall`. Relevant behavior: `game.js` player defaults, `movePlayerX`, `hazardReturn`, `update`, `drawPlayer` and frame loop; original movement tests; Wanderer manifest; Belfry collision dimensions in the art manifest.

| Original file | SHA-256 at inspection and final verification |
|---|---|
| `game.js` | `C7D2AA38539F36F43591C3FAD44EA2C5B4241B3A06E345B957FD9DCB50229050` |
| `assets/wanderer-smooth.png` | `4033D3A19057F7E80FDBAE0212FE5594F6AD1DE31806DDC0170F9A8E1262C548` |
| `assets/wanderer-smooth.json` | `40BCAB87760F0829A29442BF4BD89FE74D9461557417B94746FF2B987F69EA9D` |

Changes to existing Unity configuration are limited to four new layers, fixed timestep/catch-up settings, and placing MovementLab first in build scenes. The initialized Unity version, package list, render pipeline, existing input asset and SampleScene are preserved.
