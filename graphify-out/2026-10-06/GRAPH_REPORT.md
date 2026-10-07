# Graph Report - Echo-Fall  (2026-10-06)

## Corpus Check
- 75 files · ~356,869 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 406 file(s) not represented in the graph (top: .meta 224, .asset 158, .unity 9)

## Summary
- 1373 nodes · 2362 edges · 78 communities (61 shown, 17 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 73 edges (avg confidence: 0.87)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c4f28a7f`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- WakeSliceBuilder
- manifest.json
- dependencies
- SliceCombat
- PlayerMotor
- MonoBehaviour
- unityengine
- MovementTests
- packages-lock.json
- WakeVisualPass
- .Create
- SliceSession
- author_movement_assets.py
- SliceEnemy
- WakeSlice five-room memory loop
- com.unity.render-pipelines.core
- .MatchesOriginalPerTickTrajectory
- MovementPlayTests
- SliceArchive
- SliceHUD
- com.unity.microsoft.gdk.discovery
- com.unity.test-framework
- com.unity.modules.unitywebrequest
- PlayerCameraImpulse
- com.unity.ugui
- com.unity.modules.assetbundle
- com.unity.2d.tilemap
- com.unity.modules.uielements
- SliceEffects
- com.unity.ext.nunit
- com.unity.services.deployment.api
- capture_movement_reference.cjs
- dependencies
- WandererPose
- com.unity.ai.navigation
- com.unity.collections
- com.unity.modules.jsonserialize
- com.unity.modules.animation
- com.unity.modules.physics2d
- com.unity.modules.terrain
- dependencies
- com.unity.modules.imageconversion
- com.unity.modules.umbra
- PlayerVisual
- .CaptureAll
- com.unity.burst
- com.unity.nuget.mono-cecil
- com.unity.xr.legacyinputhelpers
- Q: Phase 2 checkpoint/loadout continuation status and dependencies
- SlicePlayerPresentation
- com.unity.2d.psdimporter
- com.unity.2d.spriteshape
- com.unity.2d.tilemap.extras
- com.unity.modules.physics
- com.unity.2d.tooling
- com.unity.2d.common
- Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work
- com.unity.2d.sprite
- com.unity.modules.unitywebrequestaudio
- .Strike
- The Wake gameplay validation
- Sprite Sheet
- Sprite Sheet
- Movement Lab Screenshot
- Glass Archive Screenshot
- Cistern Mercy Screenshot
- Life Transfer Screenshot
- Wake Screenshot
- Wake Visual Screenshot
- Choir spire artwork
- Drone sprite sheet
- Eight armored spear bearer poses
- Pointed stone arch with cyan accents
- Ruin ledge artwork
- Tiny pale square texture

## God Nodes (most connected - your core abstractions)
1. `SliceSession` - 59 edges
2. `PlayerMotor` - 47 edges
3. `SliceCombat` - 40 edges
4. `PlayerVisual` - 35 edges
5. `WakeSliceBuilder` - 31 edges
6. `SlicePlayTests` - 29 edges
7. `MovementTests` - 28 edges
8. `EchoFall.Movement` - 26 edges
9. `WakeVisualPass` - 25 edges
10. `SliceEnemy` - 24 edges

## Surprising Connections (you probably didn't know these)
- `MovementInput` --references--> `MovementInput`  [EXTRACTED]
  Docs/Phase1-Movement.md → Assets/EchoFall/Scripts/MovementInput.cs
- `Verification` --references--> `PlayerMotor`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerMotor.cs
- `Verification` --references--> `PlayerVisual`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerVisual.cs
- `Wanderer player presentation` --references--> `PlayerVisual`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerVisual.cs
- `Verification` --references--> `RoomCamera`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/RoomCamera.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Checkpoint continuation dependencies** — docs_migrationplan_checkpointsave, docs_migrationplan_rest, docs_migrationplan_lifetimes, docs_migrationplan_loadout [EXTRACTED 1.00]

## Communities (78 total, 17 thin omitted)

### Community 0 - "WakeSliceBuilder"
Cohesion: 0.08
Nodes (8): Decor, Enemy, Item, RectData, Room, Source, SpawnData, WakeSliceBuilder

### Community 1 - "manifest.json"
Cohesion: 0.03
Nodes (59): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+51 more)

### Community 2 - "dependencies"
Cohesion: 0.03
Nodes (60): dependencies, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape, com.unity.2d.tilemap, com.unity.2d.tilemap.extras (+52 more)

### Community 3 - "SliceCombat"
Cohesion: 0.13
Nodes (13): SliceCombat, AttackVisual, Busy, ChargingGuard, Combo, Fracture, Guarding, ImprintTime (+5 more)

### Community 4 - "PlayerMotor"
Cohesion: 0.06
Nodes (18): MovementInput, HasMovementIntent, MovementTuning, PlayerMotor, AirDashUsed, DashCooldown, Dashing, Facing (+10 more)

### Community 5 - "MonoBehaviour"
Cohesion: 0.06
Nodes (6): LabMarker, MovementLabHUD, SliceParallax, SliceSound, WakeAtmosphere, WakeCameraPresentation

### Community 6 - "unityengine"
Cohesion: 0.07
Nodes (6): SliceRoom, SliceSpawn, EchoFall.Movement.Tests, EchoFall.Movement, EchoFall.Movement.Editor, CheckpointMenuReview

### Community 8 - "packages-lock.json"
Cohesion: 0.05
Nodes (43): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+35 more)

### Community 11 - "SliceSession"
Cohesion: 0.05
Nodes (29): MovementCommand, SliceInteraction, SliceScreen, Dialogue, Loading, Pause, Playing, Transfer (+21 more)

### Community 12 - "author_movement_assets.py"
Cohesion: 0.13
Nodes (13): binding(), box(), collider(), gameobject(), guid(), meta(), ref(), renderer() (+5 more)

### Community 13 - "SliceEnemy"
Cohesion: 0.15
Nodes (6): SliceEnemy, Alive, HitRect, Red, State, SliceProjectile

### Community 14 - "WakeSlice five-room memory loop"
Cohesion: 0.07
Nodes (34): CheckpointSave, Original combat parity, Effective room geometry, Memory and style loadout, Echo-Fall Unity migration, Completed bench rest, MovementInput, MovementLab (+26 more)

### Community 15 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 16 - ".MatchesOriginalPerTickTrajectory"
Cohesion: 0.10
Nodes (7): Box, Rect, Command, ReferenceData, ReferenceTrajectoryTests, Sample, Trajectory

### Community 18 - "SliceArchive"
Cohesion: 0.10
Nodes (8): SliceCheckpoint, SliceEnemySnapshot, SliceArchive, checkpoint, RootsOpen, SliceMemoryStore, DefaultPath, SliceRuleTests

### Community 20 - "com.unity.microsoft.gdk.discovery"
Cohesion: 0.11
Nodes (18): dependencies, depth, dependencies, depth, source, url, version, source (+10 more)

### Community 21 - "com.unity.test-framework"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, depth, dependencies (+9 more)

### Community 22 - "com.unity.modules.unitywebrequest"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 23 - "PlayerCameraImpulse"
Cohesion: 0.16
Nodes (3): PlayerCameraImpulse, Offset, RoomCamera

### Community 24 - "com.unity.ugui"
Cohesion: 0.13
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 25 - "com.unity.modules.assetbundle"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 26 - "com.unity.2d.tilemap"
Cohesion: 0.22
Nodes (9): dependencies, depth, source, version, depth, source, version, com.unity.2d.tilemap (+1 more)

### Community 27 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 29 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 30 - "com.unity.services.deployment.api"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.services.deployment.api

### Community 31 - "capture_movement_reference.cjs"
Cohesion: 0.17
Nodes (8): {boot}, cases, crypto, definitions, fs, out, output, path

### Community 32 - "dependencies"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.2d.aseprite

### Community 33 - "WandererPose"
Cohesion: 0.12
Nodes (16): WandererPose, Attack, Dash, Death, Deflect, Fall, Guard, Hurt (+8 more)

### Community 34 - "com.unity.ai.navigation"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 35 - "com.unity.collections"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 36 - "com.unity.modules.jsonserialize"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 37 - "com.unity.modules.animation"
Cohesion: 0.08
Nodes (26): dependencies, depth, source, version, dependencies, depth, source, version (+18 more)

### Community 38 - "com.unity.modules.physics2d"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 39 - "com.unity.modules.terrain"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 40 - "dependencies"
Cohesion: 0.13
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 41 - "com.unity.modules.imageconversion"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 42 - "com.unity.modules.umbra"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.umbra

### Community 43 - "PlayerVisual"
Cohesion: 0.12
Nodes (6): PlayerVisual, FrameIndex, LiveGhosts, Pose, PoseHoldRemaining, StrongImpactCount

### Community 45 - "com.unity.burst"
Cohesion: 0.18
Nodes (12): dependencies, dependencies, depth, source, url, version, dependencies, depth (+4 more)

### Community 46 - "com.unity.nuget.mono-cecil"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 47 - "com.unity.xr.legacyinputhelpers"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.modules.xr, com.unity.xr.legacyinputhelpers

### Community 48 - "Q: Phase 2 checkpoint/loadout continuation status and dependencies"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Phase 2 checkpoint/loadout continuation status and dependencies, Source Nodes

### Community 49 - "SlicePlayerPresentation"
Cohesion: 0.40
Nodes (4): SlicePlayerPresentation, Asset gaps, Verification, Wanderer player presentation

### Community 50 - "com.unity.2d.psdimporter"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.2d.psdimporter

### Community 51 - "com.unity.2d.spriteshape"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.2d.spriteshape

### Community 52 - "com.unity.2d.tilemap.extras"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.2d.tilemap.extras

### Community 53 - "com.unity.modules.physics"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, version, dependencies, depth, source, version (+19 more)

### Community 54 - "com.unity.2d.tooling"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.2d.tooling

### Community 55 - "com.unity.2d.common"
Cohesion: 0.40
Nodes (5): depth, source, url, version, com.unity.2d.common

### Community 56 - "Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work, Source Nodes

### Community 57 - "com.unity.2d.sprite"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.2d.sprite

### Community 58 - "com.unity.modules.unitywebrequestaudio"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestaudio

### Community 60 - "The Wake gameplay validation"
Cohesion: 0.67
Nodes (3): Integrity and resonance HUD, Signal Well and signal anchor, The Wake gameplay validation

### Community 61 - "Sprite Sheet"
Cohesion: 0.67
Nodes (3): Eight Poses in Two Rows, Shield and Spear Sentinel, Sprite Sheet

### Community 62 - "Sprite Sheet"
Cohesion: 0.67
Nodes (3): Movement and Sword Attack Poses, Scarfed Sword Wanderer, Sprite Sheet

### Community 63 - "Movement Lab Screenshot"
Cohesion: 0.67
Nodes (3): Movement Lab Screenshot, Variable Jump and One Way Platforms, Wall Ceiling Gap and Safe Reset

### Community 64 - "Glass Archive Screenshot"
Cohesion: 0.67
Nodes (3): Belfry Interaction Prompt, Glass Archive Screenshot, Integrity and Resonance HUD

### Community 65 - "Cistern Mercy Screenshot"
Cohesion: 0.67
Nodes (3): Cistern Mercy Screenshot, Drowned Engineer Interaction, Mercy Root Bridge

### Community 66 - "Life Transfer Screenshot"
Cohesion: 0.67
Nodes (3): Archive Endures World Rebuild, Life Transfer Screenshot, Remember Mercy Choice

### Community 67 - "Wake Screenshot"
Cohesion: 0.67
Nodes (3): Signal Anchor Interaction, Wake Screenshot, Wounded Creature Objective

### Community 68 - "Wake Visual Screenshot"
Cohesion: 0.67
Nodes (3): Bright Cathedral Backdrop, Signal Anchor Interaction, Wake Visual Screenshot

## Knowledge Gaps
- **570 isolated node(s):** `SpawnData`, `Enemy`, `Decor`, `HasMovementIntent`, `Offset` (+565 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 753 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **17 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SliceSession` connect `SliceSession` to `SliceCombat`, `PlayerMotor`, `MonoBehaviour`, `unityengine`, `SliceArchive`, `SliceHUD`, `PlayerCameraImpulse`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **What connects `SpawnData`, `Enemy`, `Decor` to the rest of the system?**
  _570 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `WakeSliceBuilder` be split into smaller, more focused modules?**
  _Cohesion score 0.07868852459016394 - nodes in this community are weakly interconnected._
- **Why does `WakeSliceBuilder` connect `WakeSliceBuilder` to `unityengine`, `WakeSlice five-room memory loop`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._
- **Why does `dependencies` connect `dependencies` to `packages-lock.json`, `com.unity.render-pipelines.core`, `com.unity.microsoft.gdk.discovery`, `com.unity.test-framework`, `com.unity.modules.unitywebrequest`, `com.unity.ugui`, `com.unity.modules.assetbundle`, `com.unity.2d.tilemap`, `com.unity.modules.uielements`, `com.unity.ext.nunit`, `com.unity.services.deployment.api`, `dependencies`, `com.unity.ai.navigation`, `com.unity.collections`, `com.unity.modules.jsonserialize`, `com.unity.modules.animation`, `com.unity.modules.physics2d`, `com.unity.modules.terrain`, `com.unity.modules.imageconversion`, `com.unity.modules.umbra`, `com.unity.burst`, `com.unity.nuget.mono-cecil`, `com.unity.xr.legacyinputhelpers`, `com.unity.2d.psdimporter`, `com.unity.2d.spriteshape`, `com.unity.2d.tilemap.extras`, `com.unity.modules.physics`, `com.unity.2d.tooling`, `com.unity.2d.common`, `com.unity.2d.sprite`, `com.unity.modules.unitywebrequestaudio`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Should `dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._