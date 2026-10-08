# Graph Report - Echo-Fall  (2026-10-07)

## Corpus Check
- 117 files · ~644,867 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 461 file(s) not represented in the graph (top: .meta 260, .asset 169, .unity 17)

## Summary
- 1649 nodes · 3002 edges · 83 communities (56 shown, 27 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 113 edges (avg confidence: 0.87)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `bb50ea4d`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- WakeSliceBuilder
- manifest.json
- dependencies
- SliceCombat
- PlayerMotor
- MovementLabHUD
- unityengine
- MovementTests
- packages-lock.json
- WakeVisualPass
- .Create
- SlicePlayTests
- author_movement_assets.py
- SliceEnemy
- WakeSlice five-room memory loop
- com.unity.render-pipelines.core
- .MatchesOriginalPerTickTrajectory
- WorldCatalog
- SliceArchive
- SliceHUD
- com.unity.microsoft.gdk.discovery
- com.unity.collections
- com.unity.modules.unitywebrequest
- PlayerCameraImpulse
- com.unity.modules.jsonserialize
- com.unity.modules.assetbundle
- com.unity.2d.tilemap
- com.unity.modules.uielements
- SliceEffects
- com.unity.modules.imgui
- Phase3CombatPlayTests
- capture_movement_reference.cjs
- com.unity.burst
- WandererPose
- com.unity.ai.navigation
- SliceSession
- com.unity.modules.terrain
- com.unity.modules.audio
- com.unity.modules.physics2d
- .LoadRoom
- com.unity.modules.subsystems
- SliceKing
- dependencies
- PlayerVisual
- .CaptureAll
- RoomCamera
- com.unity.nuget.mono-cecil
- com.unity.xr.legacyinputhelpers
- Q: Phase 2 checkpoint/loadout continuation status and dependencies
- SlicePlayerPresentation
- com.unity.2d.common
- SliceProjectile
- WorldExpansionPlayTests
- com.unity.modules.physics
- SliceRoom
- Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work
- WorldMap
- SliceSound
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
- SliceScreen
- WakeAtmosphere
- MonoBehaviour
- .Awake
- SliceParallax

## God Nodes (most connected - your core abstractions)
1. `SliceSession` - 66 edges
2. `PlayerMotor` - 49 edges
3. `SliceCombat` - 48 edges
4. `SliceEnemy` - 45 edges
5. `WakeSliceBuilder` - 36 edges
6. `PlayerVisual` - 36 edges
7. `EchoFall.Movement` - 35 edges
8. `SlicePlayTests` - 29 edges
9. `MovementTests` - 28 edges
10. `WakeVisualPass` - 26 edges

## Surprising Connections (you probably didn't know these)
- `Verification` --references--> `PlayerMotor`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerMotor.cs
- `Verification` --references--> `PlayerVisual`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerVisual.cs
- `Wanderer player presentation` --references--> `PlayerVisual`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerVisual.cs
- `Verification` --references--> `RoomCamera`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/RoomCamera.cs
- `Verification` --references--> `SliceCombat`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/SliceCombat.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Checkpoint continuation dependencies** — docs_migrationplan_checkpointsave, docs_migrationplan_rest, docs_migrationplan_lifetimes, docs_migrationplan_loadout [EXTRACTED 1.00]

## Communities (83 total, 27 thin omitted)

### Community 0 - "WakeSliceBuilder"
Cohesion: 0.05
Nodes (9): Decor, Enemy, Item, RectData, Room, Source, SpawnData, WakeSliceBuilder (+1 more)

### Community 1 - "manifest.json"
Cohesion: 0.03
Nodes (59): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+51 more)

### Community 2 - "dependencies"
Cohesion: 0.03
Nodes (60): dependencies, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape, com.unity.2d.tilemap, com.unity.2d.tilemap.extras (+52 more)

### Community 3 - "SliceCombat"
Cohesion: 0.10
Nodes (16): SliceCombat, AttackVisual, Busy, ChargingGuard, Combo, DashFollowup, Fracture, Guarding (+8 more)

### Community 4 - "PlayerMotor"
Cohesion: 0.06
Nodes (20): MovementInput, HasMovementIntent, MovementTuning, PlayerMotor, AirDashUsed, DashCooldown, Dashing, Facing (+12 more)

### Community 6 - "unityengine"
Cohesion: 0.07
Nodes (7): WorldDefinition, WorldLink, WorldPlace, EchoFall.Movement.Tests, EchoFall.Movement, EchoFall.Movement.Editor, CheckpointMenuReview

### Community 8 - "packages-lock.json"
Cohesion: 0.05
Nodes (43): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+35 more)

### Community 12 - "author_movement_assets.py"
Cohesion: 0.10
Nodes (13): binding(), box(), collider(), gameobject(), guid(), meta(), ref(), renderer() (+5 more)

### Community 13 - "SliceEnemy"
Cohesion: 0.11
Nodes (11): SliceEnemy, Alive, Guarding, HitRect, MaxHealth, Missed, RecoveryRemaining, Red (+3 more)

### Community 14 - "WakeSlice five-room memory loop"
Cohesion: 0.06
Nodes (41): CheckpointSave, Original combat parity, Effective room geometry, Memory and style loadout, Echo-Fall Unity migration, Completed bench rest, MovementLab, Phase 1 validation (+33 more)

### Community 15 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 16 - ".MatchesOriginalPerTickTrajectory"
Cohesion: 0.10
Nodes (7): Box, Rect, Command, ReferenceData, ReferenceTrajectoryTests, Sample, Trajectory

### Community 17 - "WorldCatalog"
Cohesion: 0.06
Nodes (15): SliceCheckpoint, SliceEnemySnapshot, WorldCatalog, CollectibleCount, Data, EnemyCount, Phase3CombatRuleTests, WorldExpansionTests (+7 more)

### Community 18 - "SliceArchive"
Cohesion: 0.13
Nodes (6): SliceArchive, checkpoint, RootsOpen, SliceMemoryStore, DefaultPath, SliceRuleTests

### Community 20 - "com.unity.microsoft.gdk.discovery"
Cohesion: 0.11
Nodes (18): dependencies, depth, dependencies, depth, source, url, version, source (+10 more)

### Community 21 - "com.unity.collections"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, url, version, dependencies, depth, source (+19 more)

### Community 22 - "com.unity.modules.unitywebrequest"
Cohesion: 0.06
Nodes (34): dependencies, depth, source, version, dependencies, depth, source, version (+26 more)

### Community 24 - "com.unity.modules.jsonserialize"
Cohesion: 0.07
Nodes (32): dependencies, depth, source, url, version, dependencies, depth, source (+24 more)

### Community 25 - "com.unity.modules.assetbundle"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 26 - "com.unity.2d.tilemap"
Cohesion: 0.13
Nodes (16): dependencies, depth, dependencies, depth, source, url, version, source (+8 more)

### Community 27 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 29 - "com.unity.modules.imgui"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, version, dependencies, depth, source, url (+9 more)

### Community 30 - "Phase3CombatPlayTests"
Cohesion: 0.07
Nodes (5): MovementCommand, MovementAcceptanceDriver, MovementPlayTests, Phase3CombatPlayTests, S

### Community 31 - "capture_movement_reference.cjs"
Cohesion: 0.17
Nodes (8): {boot}, cases, crypto, definitions, fs, out, output, path

### Community 32 - "com.unity.burst"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, dependencies, depth (+9 more)

### Community 33 - "WandererPose"
Cohesion: 0.12
Nodes (16): WandererPose, Attack, Dash, Death, Deflect, Fall, Guard, Hurt (+8 more)

### Community 34 - "com.unity.ai.navigation"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 35 - "SliceSession"
Cohesion: 0.07
Nodes (21): SliceSession, Archive, CanChangeMemory, CreatureChoiceKnown, CurrentMessage, Decision, EncounterCleared, Instance (+13 more)

### Community 36 - "com.unity.modules.terrain"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 37 - "com.unity.modules.audio"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 38 - "com.unity.modules.physics2d"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, url, version, dependencies, depth, source (+9 more)

### Community 40 - "com.unity.modules.subsystems"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 41 - "SliceKing"
Cohesion: 0.08
Nodes (12): SliceArenaSeal, SliceKing, Active, Breaks, Exposed, HitRect, LastStyle, Pattern (+4 more)

### Community 42 - "dependencies"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 43 - "PlayerVisual"
Cohesion: 0.14
Nodes (6): PlayerVisual, FrameIndex, LiveGhosts, Pose, PoseHoldRemaining, StrongImpactCount

### Community 46 - "com.unity.nuget.mono-cecil"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, version, dependencies, depth, source, url (+15 more)

### Community 47 - "com.unity.xr.legacyinputhelpers"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.modules.xr, com.unity.xr.legacyinputhelpers

### Community 48 - "Q: Phase 2 checkpoint/loadout continuation status and dependencies"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Phase 2 checkpoint/loadout continuation status and dependencies, Source Nodes

### Community 49 - "SlicePlayerPresentation"
Cohesion: 0.40
Nodes (4): SlicePlayerPresentation, Asset gaps, Verification, Wanderer player presentation

### Community 50 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (39): dependencies, depth, source, url, version, dependencies, depth, source (+31 more)

### Community 51 - "SliceProjectile"
Cohesion: 0.24
Nodes (5): SliceProjectile, Damage, Friendly, Reflected, Velocity

### Community 53 - "com.unity.modules.physics"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 55 - "SliceRoom"
Cohesion: 0.20
Nodes (3): SliceRoom, SliceSpawn, Phase3SceneAudit

### Community 56 - "Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work, Source Nodes

### Community 57 - "WorldMap"
Cohesion: 0.08
Nodes (5): WakeCameraPresentation, WorldMap, Phase3Review, Phase4Review, DirectoryPath

### Community 59 - ".Strike"
Cohesion: 0.15
Nodes (8): CombatRules, CombatHitResult, Blocked, Damaged, Deflected, Immune, SlicePogo, Shared enemy framework and roster

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

### Community 78 - "SliceScreen"
Cohesion: 0.29
Nodes (7): SliceScreen, Dialogue, Loading, Map, Pause, Playing, Transfer

## Knowledge Gaps
- **619 isolated node(s):** `SpawnData`, `Enemy`, `Decor`, `HasMovementIntent`, `Offset` (+614 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 861 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SliceSession` connect `SliceSession` to `SliceCombat`, `PlayerMotor`, `unityengine`, `.LoadRoom`, `SliceKing`, `SlicePlayTests`, `SliceEnemy`, `RoomCamera`, `SliceScreen`, `MonoBehaviour`, `WorldCatalog`, `SliceArchive`, `SliceHUD`, `SliceRoom`, `WorldMap`?**
  _High betweenness centrality (0.075) - this node is a cross-community bridge._
- **What connects `SpawnData`, `Enemy`, `Decor` to the rest of the system?**
  _619 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `WakeSliceBuilder` be split into smaller, more focused modules?**
  _Cohesion score 0.05479059093516925 - nodes in this community are weakly interconnected._
- **Why does `WakeSliceBuilder` connect `WakeSliceBuilder` to `unityengine`, `WakeSlice five-room memory loop`?**
  _High betweenness centrality (0.071) - this node is a cross-community bridge._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._
- **Why does `dependencies` connect `dependencies` to `packages-lock.json`, `com.unity.render-pipelines.core`, `com.unity.microsoft.gdk.discovery`, `com.unity.collections`, `com.unity.modules.unitywebrequest`, `com.unity.modules.jsonserialize`, `com.unity.modules.assetbundle`, `com.unity.2d.tilemap`, `com.unity.modules.uielements`, `com.unity.modules.imgui`, `com.unity.burst`, `com.unity.ai.navigation`, `com.unity.modules.terrain`, `com.unity.modules.audio`, `com.unity.modules.physics2d`, `com.unity.modules.subsystems`, `com.unity.nuget.mono-cecil`, `com.unity.xr.legacyinputhelpers`, `com.unity.2d.common`, `com.unity.modules.physics`?**
  _High betweenness centrality (0.063) - this node is a cross-community bridge._
- **Should `dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._