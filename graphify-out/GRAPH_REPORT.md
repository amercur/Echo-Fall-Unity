# Graph Report - Echo-Fall  (2026-10-08)

## Corpus Check
- 132 files · ~724,686 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 466 file(s) not represented in the graph (top: .meta 265, .asset 169, .unity 17)

## Summary
- 1753 nodes · 3305 edges · 93 communities (61 shown, 32 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 163 edges (avg confidence: 0.87)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5b9d3cff`
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
- .Transfer
- SliceHUD
- com.unity.microsoft.gdk.discovery
- com.unity.collections
- com.unity.modules.unitywebrequest
- PlayerCameraImpulse
- com.unity.modules.jsonserialize
- com.unity.modules.assetbundle
- com.unity.2d.tilemap
- com.unity.modules.uielements
- .Ring
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
- .EndRun
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
- .ReceiveHit
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
- MovementPlayTests
- MemoryConsequencePlayTests
- SliceArchive
- SliceCheckpoint
- .AllAuthoredTransitionsMatchCatalogAndNamedSafeEntries
- Phase 5: remembered consequences
- .Awake
- Phase5Review
- Phase 4: the connected world
- SliceParallax

## God Nodes (most connected - your core abstractions)
1. `SliceSession` - 84 edges
2. `PlayerMotor` - 52 edges
3. `SliceCombat` - 49 edges
4. `SliceEnemy` - 47 edges
5. `EchoFall.Movement` - 39 edges
6. `PlayerVisual` - 37 edges
7. `WakeSliceBuilder` - 36 edges
8. `SliceArchive` - 34 edges
9. `SlicePlayTests` - 29 edges
10. `MovementTests` - 28 edges

## Surprising Connections (you probably didn't know these)
- `World and Echo implementation` --references--> `MemoryWorld`  [INFERRED]
  Docs/Phase5-MemoryConsequences.md → Assets/EchoFall/Scripts/MemoryWorld.cs
- `Source and dependency review` --references--> `WorldCatalog`  [INFERRED]
  Docs/Phase4-WorldExpansion.md → Assets/EchoFall/Scripts/WorldCatalog.cs
- `Dependencies and source comparison` --references--> `SliceSession`  [INFERRED]
  Docs/Phase5-MemoryConsequences.md → Assets/EchoFall/Scripts/MemoryFlow.cs
- `Verification` --references--> `PlayerMotor`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerMotor.cs
- `Verification` --references--> `PlayerVisual`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerVisual.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Checkpoint continuation dependencies** — docs_migrationplan_checkpointsave, docs_migrationplan_rest, docs_migrationplan_lifetimes, docs_migrationplan_loadout [EXTRACTED 1.00]

## Communities (93 total, 32 thin omitted)

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
Cohesion: 0.05
Nodes (22): MovementInput, HasMovementIntent, MovementTuning, PlayerMotor, AirDashUsed, DashCooldown, Dashing, DoubleJumpUsed (+14 more)

### Community 6 - "unityengine"
Cohesion: 0.08
Nodes (4): EchoFall.Movement.Tests, EchoFall.Movement, EchoFall.Movement.Editor, CheckpointMenuReview

### Community 8 - "packages-lock.json"
Cohesion: 0.05
Nodes (43): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+35 more)

### Community 11 - "SlicePlayTests"
Cohesion: 0.14
Nodes (3): MovementCommand, SlicePlayTests, S

### Community 12 - "author_movement_assets.py"
Cohesion: 0.10
Nodes (13): binding(), box(), collider(), gameobject(), guid(), meta(), ref(), renderer() (+5 more)

### Community 13 - "SliceEnemy"
Cohesion: 0.09
Nodes (14): MemoryEcho, Recognized, Refused, SliceEnemy, Alive, Guarding, HitRect, MaxHealth (+6 more)

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
Cohesion: 0.17
Nodes (6): WorldCatalog, CollectibleCount, Data, EnemyCount, WorldDefinition, WorldPlace

### Community 18 - ".Transfer"
Cohesion: 0.16
Nodes (4): MemoryAge, SliceMemoryStore, DefaultPath, SliceRuleTests

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
Nodes (24): SliceSession, MemoryCursor, RunPower, SliceInteraction, Archive, CanChangeMemory, CreatureChoiceKnown, CurrentMessage (+16 more)

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
Cohesion: 0.10
Nodes (13): SliceArenaSeal, SliceKing, Active, AidStrikes, Breaks, Exposed, HitRect, LastStyle (+5 more)

### Community 42 - "dependencies"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 43 - "PlayerVisual"
Cohesion: 0.12
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
Cohesion: 0.33
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

### Community 54 - ".ReceiveHit"
Cohesion: 0.25
Nodes (5): CombatHitResult, Blocked, Damaged, Deflected, Immune

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
Cohesion: 0.21
Nodes (3): CombatRules, SlicePogo, Shared enemy framework and roster

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

### Community 83 - "SliceArchive"
Cohesion: 0.17
Nodes (7): MemoryWorld, SliceArchive, checkpoint, MatureMercy, RootsOpen, WorldLink, MemoryConsequenceTests

### Community 84 - "SliceCheckpoint"
Cohesion: 0.21
Nodes (3): SliceCheckpoint, SliceEnemySnapshot, Phase3CombatRuleTests

### Community 88 - "Phase 5: remembered consequences"
Cohesion: 0.29
Nodes (7): Decisions and consequences, Dependencies and source comparison, Phase 5: remembered consequences, Remaining gaps and Phase 6, Transfer, forgetting and persistence, Validation, World and Echo implementation

### Community 91 - "Phase 4: the connected world"
Cohesion: 0.29
Nodes (7): Map and persistence, New rooms and regions, Phase 4: the connected world, Remaining gaps and Phase 5, Routes, shortcuts and secrets, Source and dependency review, Validation

## Knowledge Gaps
- **633 isolated node(s):** `SpawnData`, `Enemy`, `Decor`, `RunPower`, `MemoryCursor` (+628 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 893 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **32 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SliceSession` connect `SliceSession` to `SliceCombat`, `PlayerMotor`, `unityengine`, `SlicePlayTests`, `SliceEnemy`, `SliceHUD`, `.EndRun`, `RoomCamera`, `SliceRoom`, `WorldMap`, `SliceScreen`, `MonoBehaviour`, `MemoryConsequencePlayTests`, `SliceArchive`, `SliceCheckpoint`, `.Apply`, `.LoadRoom`, `Phase 5: remembered consequences`, `Phase5Review`?**
  _High betweenness centrality (0.087) - this node is a cross-community bridge._
- **What connects `SpawnData`, `Enemy`, `Decor` to the rest of the system?**
  _633 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `WakeSliceBuilder` be split into smaller, more focused modules?**
  _Cohesion score 0.05479059093516925 - nodes in this community are weakly interconnected._
- **Why does `WakeSliceBuilder` connect `WakeSliceBuilder` to `unityengine`, `WakeSlice five-room memory loop`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._
- **Why does `dependencies` connect `dependencies` to `packages-lock.json`, `com.unity.render-pipelines.core`, `com.unity.microsoft.gdk.discovery`, `com.unity.collections`, `com.unity.modules.unitywebrequest`, `com.unity.modules.jsonserialize`, `com.unity.modules.assetbundle`, `com.unity.2d.tilemap`, `com.unity.modules.uielements`, `com.unity.modules.imgui`, `com.unity.burst`, `com.unity.ai.navigation`, `com.unity.modules.terrain`, `com.unity.modules.audio`, `com.unity.modules.physics2d`, `com.unity.modules.subsystems`, `com.unity.nuget.mono-cecil`, `com.unity.xr.legacyinputhelpers`, `com.unity.2d.common`, `com.unity.modules.physics`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Should `dependencies` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._