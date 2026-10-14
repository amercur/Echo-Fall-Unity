# Graph Report - Echo-Fall  (2026-10-13)

## Corpus Check
- 194 files · ~1,782,491 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 485 file(s) not represented in the graph (top: .meta 281, .asset 169, .unity 17)

## Summary
- 2039 nodes · 3970 edges · 106 communities (77 shown, 29 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 221 edges (avg confidence: 0.86)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `9a47845a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- .BuildWorld
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
- StandaloneQA
- SliceArchive
- SliceHUD
- com.unity.microsoft.gdk.discovery
- com.unity.collections
- com.unity.modules.unitywebrequest
- RoomCamera
- com.unity.ugui
- com.unity.modules.assetbundle
- com.unity.2d.tilemap
- com.unity.modules.uielements
- .Line
- com.unity.ext.nunit
- Phase3CombatPlayTests
- capture_movement_reference.cjs
- com.unity.mathematics
- WandererPose
- com.unity.ai.navigation
- SliceSession
- com.unity.modules.jsonserialize
- com.unity.modules.audio
- com.unity.modules.physics2d
- .Remembers
- com.unity.modules.subsystems
- SliceKing
- dependencies
- PlayerVisual
- Advanced presentation pass
- com.unity.nuget.mono-cecil
- com.unity.xr.legacyinputhelpers
- Q: Phase 2 checkpoint/loadout continuation status and dependencies
- SliceCheckpoint
- com.unity.2d.common
- .Eligible
- WorldCatalog
- com.unity.modules.physics
- Phase 3: combat and the Court
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
- WakeAtmosphere
- MonoBehaviour
- MovementPlayTests
- MemoryConsequencePlayTests
- SliceMother
- .LoadRoom
- PlayerPreferences
- MovementInput
- .Cue
- .BuildRoom
- Phase5Review
- Phase 4: the connected world
- WakeSliceBuilder
- .Geometry
- Phase6Review
- SliceRuleTests
- ReleaseBuild
- .Interact
- Phase 6: the voices converge
- ECHO//FALL Unity
- Echo-Fall Unity migration
- Phase 5: remembered consequences
- Remaining checkpoint/loadout persistence
- MovementLab
- .Run
- package.json

## God Nodes (most connected - your core abstractions)
1. `SliceSession` - 105 edges
2. `SliceEnemy` - 54 edges
3. `PlayerMotor` - 52 edges
4. `SliceCombat` - 51 edges
5. `EchoFall.Movement` - 50 edges
6. `SliceArchive` - 43 edges
7. `PlayerVisual` - 40 edges
8. `WakeSliceBuilder` - 37 edges
9. `SlicePlayTests` - 29 edges
10. `MovementTests` - 28 edges

## Surprising Connections (you probably didn't know these)
- `Dependencies and scope` --references--> `SliceSession`  [INFERRED]
  Docs/Phase7-ReleasePolish.md → Assets/EchoFall/Scripts/LateGameFlow.cs
- `Source and dependency review` --references--> `WorldCatalog`  [INFERRED]
  Docs/Phase4-WorldExpansion.md → Assets/EchoFall/Scripts/WorldCatalog.cs
- `Dependencies and source comparison` --references--> `SliceSession`  [INFERRED]
  Docs/Phase5-MemoryConsequences.md → Assets/EchoFall/Scripts/LateGameFlow.cs
- `World and Echo implementation` --references--> `MemoryWorld`  [INFERRED]
  Docs/Phase5-MemoryConsequences.md → Assets/EchoFall/Scripts/MemoryWorld.cs
- `Verification` --references--> `PlayerMotor`  [INFERRED]
  Docs/PlayerPresentation.md → Assets/EchoFall/Scripts/PlayerMotor.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Checkpoint continuation dependencies** — docs_migrationplan_checkpointsave, docs_migrationplan_rest, docs_migrationplan_lifetimes, docs_migrationplan_loadout [EXTRACTED 1.00]

## Communities (106 total, 29 thin omitted)

### Community 0 - ".BuildWorld"
Cohesion: 0.10
Nodes (3): Item, RectData, BuildPhase4

### Community 1 - "manifest.json"
Cohesion: 0.03
Nodes (59): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+51 more)

### Community 2 - "dependencies"
Cohesion: 0.03
Nodes (60): dependencies, com.unity.2d.animation, com.unity.2d.aseprite, com.unity.2d.psdimporter, com.unity.2d.sprite, com.unity.2d.spriteshape, com.unity.2d.tilemap, com.unity.2d.tilemap.extras (+52 more)

### Community 3 - "SliceCombat"
Cohesion: 0.09
Nodes (16): SliceCombat, AttackVisual, Busy, ChargingGuard, Combo, DashFollowup, Fracture, Guarding (+8 more)

### Community 4 - "PlayerMotor"
Cohesion: 0.09
Nodes (17): PlayerMotor, AirDashUsed, DashCooldown, Dashing, DoubleJumpUsed, Facing, Grounded, HasAirJump (+9 more)

### Community 6 - "unityengine"
Cohesion: 0.06
Nodes (6): PresentationTests, EchoFall.Movement.Tests, EchoFall.Movement, EchoFall.Movement.Editor, CheckpointMenuReview, Phase7PackagingAudit

### Community 8 - "packages-lock.json"
Cohesion: 0.05
Nodes (43): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+35 more)

### Community 11 - "SlicePlayTests"
Cohesion: 0.15
Nodes (3): MovementCommand, SlicePlayTests, S

### Community 12 - "author_movement_assets.py"
Cohesion: 0.09
Nodes (13): binding(), box(), collider(), gameobject(), guid(), meta(), ref(), renderer() (+5 more)

### Community 13 - "SliceEnemy"
Cohesion: 0.08
Nodes (20): SliceEnemy, Alive, EncounterStarted, Guarding, HitRect, IsBoss, MaxHealth, Missed (+12 more)

### Community 14 - "WakeSlice five-room memory loop"
Cohesion: 0.20
Nodes (14): Original combat parity, Effective room geometry, Versioned Wake archive, WakeSliceBuilder and import_wake_slice.py, Slice combat systems, Remaining combat parity, EMBER / fire, Transfer extraction (+6 more)

### Community 15 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 16 - "StandaloneQA"
Cohesion: 0.07
Nodes (17): Report, Sample, StandaloneQA, S, Box, Rect, Command, ReferenceData (+9 more)

### Community 18 - "SliceArchive"
Cohesion: 0.17
Nodes (6): MemoryAge, SliceArchive, checkpoint, MatureMercy, RootsOpen, MemoryConsequenceTests

### Community 19 - "SliceHUD"
Cohesion: 0.09
Nodes (4): ChoicePresentation, Accent, Focused, SliceHUD

### Community 20 - "com.unity.microsoft.gdk.discovery"
Cohesion: 0.11
Nodes (18): dependencies, depth, dependencies, depth, source, url, version, source (+10 more)

### Community 21 - "com.unity.collections"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, url, version, dependencies, depth, source (+19 more)

### Community 22 - "com.unity.modules.unitywebrequest"
Cohesion: 0.06
Nodes (35): dependencies, depth, source, version, dependencies, dependencies, depth, source (+27 more)

### Community 23 - "RoomCamera"
Cohesion: 0.16
Nodes (3): PlayerCameraImpulse, Offset, RoomCamera

### Community 24 - "com.unity.ugui"
Cohesion: 0.10
Nodes (22): dependencies, depth, source, url, version, dependencies, depth, source (+14 more)

### Community 25 - "com.unity.modules.assetbundle"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 26 - "com.unity.2d.tilemap"
Cohesion: 0.13
Nodes (16): dependencies, depth, dependencies, depth, source, url, version, source (+8 more)

### Community 27 - "com.unity.modules.uielements"
Cohesion: 0.13
Nodes (15): dependencies, depth, source, url, version, depth, source, version (+7 more)

### Community 29 - "com.unity.ext.nunit"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 31 - "capture_movement_reference.cjs"
Cohesion: 0.17
Nodes (8): {boot}, cases, crypto, definitions, fs, out, output, path

### Community 32 - "com.unity.mathematics"
Cohesion: 0.12
Nodes (16): dependencies, dependencies, depth, source, version, dependencies, depth, source (+8 more)

### Community 33 - "WandererPose"
Cohesion: 0.12
Nodes (16): WandererPose, Attack, Dash, Death, Deflect, Fall, Guard, Hurt (+8 more)

### Community 34 - "com.unity.ai.navigation"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, url, version, dependencies, depth, source (+3 more)

### Community 35 - "SliceSession"
Cohesion: 0.05
Nodes (24): SliceSession, MemoryCursor, RunPower, ControlsText, Archive, CanChangeMemory, CreatureChoiceKnown, CurrentMessage (+16 more)

### Community 36 - "com.unity.modules.jsonserialize"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, url, version, dependencies, depth, source (+9 more)

### Community 37 - "com.unity.modules.audio"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 38 - "com.unity.modules.physics2d"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 39 - ".Remembers"
Cohesion: 0.33
Nodes (3): MemoryWorld, WorldLink, World and Echo implementation

### Community 40 - "com.unity.modules.subsystems"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 41 - "SliceKing"
Cohesion: 0.11
Nodes (14): SliceKing, Active, AidStrikes, Breaks, EncounterStarted, Exposed, HitRect, IsBoss (+6 more)

### Community 42 - "dependencies"
Cohesion: 0.10
Nodes (20): dependencies, depth, source, version, dependencies, depth, source, version (+12 more)

### Community 43 - "PlayerVisual"
Cohesion: 0.09
Nodes (10): PlayerVisual, FrameIndex, LiveGhosts, Pose, PoseHoldRemaining, StrongImpactCount, SlicePlayerPresentation, Asset gaps (+2 more)

### Community 44 - "Advanced presentation pass"
Cohesion: 0.22
Nodes (7): Advanced presentation pass, Before / after, Dependencies and baseline, Implemented changes, Prioritized checklist, Remaining presentation work, Validation

### Community 46 - "com.unity.nuget.mono-cecil"
Cohesion: 0.11
Nodes (18): dependencies, depth, source, url, version, dependencies, depth, source (+10 more)

### Community 47 - "com.unity.xr.legacyinputhelpers"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.modules.xr, com.unity.xr.legacyinputhelpers

### Community 48 - "Q: Phase 2 checkpoint/loadout continuation status and dependencies"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Phase 2 checkpoint/loadout continuation status and dependencies, Source Nodes

### Community 49 - "SliceCheckpoint"
Cohesion: 0.20
Nodes (3): SliceCheckpoint, SliceEnemySnapshot, Phase3CombatRuleTests

### Community 50 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (39): dependencies, depth, source, url, version, dependencies, depth, source (+31 more)

### Community 51 - ".Eligible"
Cohesion: 0.19
Nodes (3): EndingRecord, EndingRules, LateGameRuleTests

### Community 52 - "WorldCatalog"
Cohesion: 0.06
Nodes (7): WorldCatalog, CollectibleCount, Data, EnemyCount, WorldExpansionTests, WorldExpansionPlayTests, S

### Community 53 - "com.unity.modules.physics"
Cohesion: 0.07
Nodes (32): dependencies, depth, source, version, dependencies, depth, source, version (+24 more)

### Community 54 - "Phase 3: combat and the Court"
Cohesion: 0.22
Nodes (9): Deliberate limits and remaining parity gaps, Dependency review before implementation, Entry and progression, King, Persistence and presentation, Phase 3: combat and the Court, Player combat, Recommended Phase 4 (+1 more)

### Community 55 - "SliceRoom"
Cohesion: 0.09
Nodes (4): SliceRoom, SliceSpawn, Phase3SceneAudit, PresentationReview

### Community 56 - "Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work, Source Nodes

### Community 57 - "WorldMap"
Cohesion: 0.06
Nodes (8): WakeCameraPresentation, WorldDefinition, WorldPlace, WorldMap, Phase3Review, Phase4Review, DirectoryPath, PlayerPresentationReview

### Community 58 - "SliceSound"
Cohesion: 0.10
Nodes (13): AudioPalette, MovementTuning, SliceScreen, Dialogue, Ending, Loading, Map, Pause (+5 more)

### Community 59 - ".Strike"
Cohesion: 0.17
Nodes (7): CombatRules, CombatHitResult, Blocked, Damaged, Deflected, Immune, Shared enemy framework and roster

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

### Community 80 - "MonoBehaviour"
Cohesion: 0.11
Nodes (5): LabMarker, SliceArenaSeal, SliceDeathEcho, SliceParallax, SlicePogo

### Community 83 - "SliceMother"
Cohesion: 0.08
Nodes (12): SliceMother, Active, AidStrikes, Attack, EncounterStarted, HitRect, IsBoss, RecallIndex (+4 more)

### Community 85 - "PlayerPreferences"
Cohesion: 0.09
Nodes (5): PlayerPreferences, Current, PolishTests, PolishPlayTests, S

### Community 86 - "MovementInput"
Cohesion: 0.15
Nodes (4): MovementInput, HasMovementIntent, MovementInput, PlayerMotor

### Community 87 - ".Cue"
Cohesion: 0.17
Nodes (4): MemoryEcho, Persistent, Recognized, Refused

### Community 91 - "Phase 4: the connected world"
Cohesion: 0.29
Nodes (7): Map and persistence, New rooms and regions, Phase 4: the connected world, Remaining gaps and Phase 5, Routes, shortcuts and secrets, Source and dependency review, Validation

### Community 93 - "WakeSliceBuilder"
Cohesion: 0.29
Nodes (4): WakeSliceBuilder, Decor, Enemy, SpawnData

### Community 101 - "SliceRuleTests"
Cohesion: 0.18
Nodes (3): SliceMemoryStore, DefaultPath, SliceRuleTests

### Community 103 - "ReleaseBuild"
Cohesion: 0.22
Nodes (4): ReleaseBuild, Result, Phase7Build, PresentationBuild

### Community 104 - ".Interact"
Cohesion: 0.12
Nodes (3): MemoryRules, Binding, SliceInteraction

### Community 105 - "Phase 6: the voices converge"
Cohesion: 0.20
Nodes (10): Child, Encounters, Ending conditions — internal reference, King, Mother, Persistence and replay, Phase 6: the voices converge, Progression and source adaptations (+2 more)

### Community 106 - "ECHO//FALL Unity"
Cohesion: 0.25
Nodes (8): Development status, ECHO//FALL Unity, Implemented gameplay, Known limitations, Open and run, Original browser version, Unity and technologies, Windows build

### Community 108 - "Phase 5: remembered consequences"
Cohesion: 0.33
Nodes (6): Decisions and consequences, Dependencies and source comparison, Phase 5: remembered consequences, Remaining gaps and Phase 6, Transfer, forgetting and persistence, Validation

### Community 110 - "Remaining checkpoint/loadout persistence"
Cohesion: 0.40
Nodes (5): CheckpointSave, Memory and style loadout, Completed bench rest, Remaining checkpoint/loadout persistence, 45 original game tests

### Community 111 - "MovementLab"
Cohesion: 0.40
Nodes (5): MovementLab, Phase 1 validation, Movement asset structural validation, Movement compiler validation, Unity 6000.5.8f1

### Community 113 - "package.json"
Cohesion: 0.50
Nodes (3): Algorithm, Hash, Path

## Knowledge Gaps
- **683 isolated node(s):** `SpawnData`, `Enemy`, `Decor`, `Focused`, `Accent` (+678 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 995 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **29 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SliceSession` connect `SliceSession` to `SliceCombat`, `PlayerMotor`, `unityengine`, `SlicePlayTests`, `SliceEnemy`, `StandaloneQA`, `SliceArchive`, `SliceHUD`, `RoomCamera`, `.Remembers`, `SliceCheckpoint`, `SliceRoom`, `WorldMap`, `SliceSound`, `MonoBehaviour`, `MemoryConsequencePlayTests`, `.LoadRoom`, `PlayerPreferences`, `MovementInput`, `Phase5Review`, `Phase6Review`, `.Interact`, `Phase 5: remembered consequences`?**
  _High betweenness centrality (0.124) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `SliceSession` (e.g. with `Dependencies and source comparison` and `Dependencies and scope`) actually correct?**
  _`SliceSession` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `SpawnData`, `Enemy`, `Decor` to the rest of the system?**
  _683 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.BuildWorld` be split into smaller, more focused modules?**
  _Cohesion score 0.1038961038961039 - nodes in this community are weakly interconnected._
- **Why does `PlayerMotor` connect `PlayerMotor` to `SliceSession`, `SliceCombat`, `MovementLabHUD`, `unityengine`, `PlayerVisual`, `MonoBehaviour`, `.Simulate`, `MovementInput`, `RoomCamera`, `WorldMap`, `SliceSound`?**
  _High betweenness centrality (0.074) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `SliceEnemy` (e.g. with `Shared enemy framework and roster` and `Progression and source adaptations`) actually correct?**
  _`SliceEnemy` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._