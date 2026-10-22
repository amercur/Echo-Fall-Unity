# Graph Report - Echo-Fall  (2026-10-22)

## Corpus Check
- 244 files · ~2,589,079 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 632 file(s) not represented in the graph (top: .meta 422, .asset 171, .unity 18)

## Summary
- 2245 nodes · 4398 edges · 115 communities (82 shown, 33 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 235 edges (avg confidence: 0.86)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f0e1c97a`
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
- .LoadRoom
- author_movement_assets.py
- SliceEnemy
- WakeSlice five-room memory loop
- com.unity.render-pipelines.core
- StandaloneQA
- OpeningPlayTests
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
- WorldCatalog
- com.unity.modules.subsystems
- SliceKing
- dependencies
- PlayerVisual
- Advanced presentation pass
- SliceDeathEcho
- com.unity.nuget.mono-cecil
- com.unity.xr.legacyinputhelpers
- Q: Phase 2 checkpoint/loadout continuation status and dependencies
- SliceCheckpoint
- com.unity.2d.common
- .CompleteEnding
- LateGamePlayTests
- com.unity.modules.physics
- Phase 3: combat and the Court
- SliceProjectile
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
- .AllAuthoredTransitionsMatchCatalogAndNamedSafeEntries
- .Run
- MonoBehaviour
- MovementPlayTests
- MemoryConsequencePlayTests
- SliceMother
- Phase6Review
- PlayerPreferences
- MovementInput
- ECHO//FALL — the hand and the record
- .BuildRoom
- Phase5Review
- Phase 4: the connected world
- WakeSliceBuilder
- .Geometry
- SliceRoom
- .Cue
- .CaptureAll
- .Interact
- OpeningExperience
- SliceRuleTests
- Phase 8: the listening glass
- .Build
- finished/package.json
- Phase 6: the voices converge
- ECHO//FALL Unity
- Echo-Fall Unity migration
- .Remembers
- .Run
- package.json
- .AuthoredAudioPaletteAndDissolveAreComplete
- README.md
- presentation/package.json

## God Nodes (most connected - your core abstractions)
1. `SliceSession` - 113 edges
2. `EchoFall.Movement` - 57 edges
3. `SliceEnemy` - 57 edges
4. `PlayerMotor` - 52 edges
5. `SliceCombat` - 51 edges
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

## Communities (115 total, 33 thin omitted)

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
Cohesion: 0.10
Nodes (17): PlayerMotor, AirDashUsed, DashCooldown, Dashing, DoubleJumpUsed, Facing, Grounded, HasAirJump (+9 more)

### Community 6 - "unityengine"
Cohesion: 0.06
Nodes (6): PresentationTests, EchoFall.Movement.Tests, EchoFall.Movement, EchoFall.Movement.Editor, CheckpointMenuReview, Phase7PackagingAudit

### Community 8 - "packages-lock.json"
Cohesion: 0.05
Nodes (43): com.unity.2d.sprite, com.unity.2d.tilemap, com.unity.modules.ai, com.unity.modules.androidjni, com.unity.modules.animation, com.unity.modules.assetbundle, com.unity.modules.audio, com.unity.modules.director (+35 more)

### Community 11 - ".LoadRoom"
Cohesion: 0.15
Nodes (3): MovementCommand, SlicePlayTests, S

### Community 12 - "author_movement_assets.py"
Cohesion: 0.06
Nodes (18): binding(), box(), collider(), gameobject(), guid(), meta(), ref(), renderer() (+10 more)

### Community 13 - "SliceEnemy"
Cohesion: 0.09
Nodes (15): SliceEnemy, Alive, EncounterStarted, Guarding, HitRect, IsBoss, MaxHealth, Missed (+7 more)

### Community 14 - "WakeSlice five-room memory loop"
Cohesion: 0.14
Nodes (19): CheckpointSave, Original combat parity, Effective room geometry, Memory and style loadout, Completed bench rest, Versioned Wake archive, WakeSliceBuilder and import_wake_slice.py, Remaining checkpoint/loadout persistence (+11 more)

### Community 15 - "com.unity.render-pipelines.core"
Cohesion: 0.09
Nodes (25): depth, source, version, dependencies, depth, source, version, dependencies (+17 more)

### Community 16 - "StandaloneQA"
Cohesion: 0.07
Nodes (17): Report, Sample, StandaloneQA, S, Box, Rect, Command, ReferenceData (+9 more)

### Community 18 - "SliceArchive"
Cohesion: 0.17
Nodes (8): MemoryAge, SliceArchive, checkpoint, MatureMercy, RootsOpen, SliceMemoryStore, DefaultPath, MemoryConsequenceTests

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
Nodes (25): SliceSession, MemoryCursor, RunPower, ControlsText, Archive, CanChangeMemory, CreatureChoiceKnown, CurrentMessage (+17 more)

### Community 36 - "com.unity.modules.jsonserialize"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, url, version, dependencies, depth, source (+9 more)

### Community 37 - "com.unity.modules.audio"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 38 - "com.unity.modules.physics2d"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, dependencies, depth, source (+8 more)

### Community 39 - "WorldCatalog"
Cohesion: 0.16
Nodes (7): WorldCatalog, CollectibleCount, Data, EnemyCount, WorldDefinition, WorldLink, WorldPlace

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
Cohesion: 0.29
Nodes (7): Advanced presentation pass, Before / after, Dependencies and baseline, Implemented changes, Prioritized checklist, Remaining presentation work, Validation

### Community 45 - "SliceDeathEcho"
Cohesion: 0.11
Nodes (3): SliceDeathEcho, FinishedPresentationPlayTests, S

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
Cohesion: 0.19
Nodes (3): SliceCheckpoint, SliceEnemySnapshot, Phase3CombatRuleTests

### Community 50 - "com.unity.2d.common"
Cohesion: 0.06
Nodes (39): dependencies, depth, source, url, version, dependencies, depth, source (+31 more)

### Community 51 - ".CompleteEnding"
Cohesion: 0.20
Nodes (3): EndingRecord, EndingRules, LateGameRuleTests

### Community 52 - "LateGamePlayTests"
Cohesion: 0.10
Nodes (4): LateGamePlayTests, S, WorldExpansionPlayTests, S

### Community 53 - "com.unity.modules.physics"
Cohesion: 0.07
Nodes (32): dependencies, depth, source, version, dependencies, depth, source, version (+24 more)

### Community 54 - "Phase 3: combat and the Court"
Cohesion: 0.22
Nodes (9): Deliberate limits and remaining parity gaps, Dependency review before implementation, Entry and progression, King, Persistence and presentation, Phase 3: combat and the Court, Player combat, Recommended Phase 4 (+1 more)

### Community 55 - "SliceProjectile"
Cohesion: 0.22
Nodes (6): SliceProjectile, Damage, Friendly, Heavy, Reflected, Velocity

### Community 56 - "Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work, Source Nodes

### Community 57 - "WorldMap"
Cohesion: 0.07
Nodes (6): WakeCameraPresentation, WorldMap, FinishedInterfaceReview, Phase3Review, Phase4Review, DirectoryPath

### Community 58 - "SliceSound"
Cohesion: 0.06
Nodes (28): AudioPalette, MovementTuning, FootMaterial, Metal, Moss, Stone, Wood, FootSurface (+20 more)

### Community 59 - ".Strike"
Cohesion: 0.16
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
Cohesion: 0.18
Nodes (3): LabMarker, SliceArenaSeal, SlicePogo

### Community 83 - "SliceMother"
Cohesion: 0.08
Nodes (12): SliceMother, Active, AidStrikes, Attack, EncounterStarted, HitRect, IsBoss, RecallIndex (+4 more)

### Community 85 - "PlayerPreferences"
Cohesion: 0.08
Nodes (7): Binding, PlayerPreferences, Current, OpeningKnowledgeTests, PolishTests, PolishPlayTests, S

### Community 86 - "MovementInput"
Cohesion: 0.15
Nodes (4): MovementInput, HasMovementIntent, MovementInput, PlayerMotor

### Community 87 - "ECHO//FALL — the hand and the record"
Cohesion: 0.33
Nodes (6): Characters, ECHO//FALL — the hand and the record, Memory and interface, Opening rhythm, Places and palette, Sound and pacing

### Community 89 - ".BuildRoom"
Cohesion: 0.16
Nodes (3): Item, RectData, Source

### Community 91 - "Phase 4: the connected world"
Cohesion: 0.29
Nodes (7): Map and persistence, New rooms and regions, Phase 4: the connected world, Remaining gaps and Phase 5, Routes, shortcuts and secrets, Source and dependency review, Validation

### Community 93 - "WakeSliceBuilder"
Cohesion: 0.29
Nodes (4): WakeSliceBuilder, Decor, Enemy, SpawnData

### Community 95 - "SliceRoom"
Cohesion: 0.05
Nodes (8): OpeningScenery, PresentationArt, SliceParallax, SliceRoom, SliceSpawn, WakeAtmosphere, Phase3SceneAudit, PresentationReview

### Community 96 - ".Cue"
Cohesion: 0.11
Nodes (5): MemoryEcho, Persistent, Recognized, Refused, SliceEffects

### Community 99 - "OpeningExperience"
Cohesion: 0.09
Nodes (5): OpeningExperience, Guidance, Isolated, Lesson, WaitingAtTitle

### Community 102 - "Phase 8: the listening glass"
Cohesion: 0.33
Nodes (6): Phase 8: the listening glass, Playable sequence, Presentation and teaching, Required custom artwork and human review, Staged rollout, Validation

### Community 103 - ".Build"
Cohesion: 0.17
Nodes (5): ReleaseBuild, Result, FinishedBuild, Phase7Build, PresentationBuild

### Community 104 - "finished/package.json"
Cohesion: 0.50
Nodes (3): bytes, file, sha256

### Community 105 - "Phase 6: the voices converge"
Cohesion: 0.20
Nodes (10): Child, Encounters, Ending conditions — internal reference, King, Mother, Persistence and replay, Phase 6: the voices converge, Progression and source adaptations (+2 more)

### Community 106 - "ECHO//FALL Unity"
Cohesion: 0.25
Nodes (8): Development status, ECHO//FALL Unity, Implemented gameplay, Known limitations, Open and run, Original browser version, Unity and technologies, Windows build

### Community 107 - "Echo-Fall Unity migration"
Cohesion: 0.38
Nodes (7): Echo-Fall Unity migration, MovementLab, Phase 1 validation, Phase 2 verification, Movement asset structural validation, Movement compiler validation, Unity 6000.5.8f1

### Community 108 - ".Remembers"
Cohesion: 0.17
Nodes (8): MemoryWorld, Decisions and consequences, Dependencies and source comparison, Phase 5: remembered consequences, Remaining gaps and Phase 6, Transfer, forgetting and persistence, Validation, World and Echo implementation

### Community 113 - "package.json"
Cohesion: 0.50
Nodes (3): Algorithm, Hash, Path

### Community 120 - "README.md"
Cohesion: 0.24
Nodes (5): Acceptance boundary, Evidence and verified baseline, Findings and priorities, Fragile systems, Phase 8 experience audit

### Community 121 - "presentation/package.json"
Cohesion: 0.50
Nodes (3): Algorithm, Hash, Path

## Knowledge Gaps
- **720 isolated node(s):** `SpawnData`, `Enemy`, `Decor`, `Focused`, `Accent` (+715 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1087 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **33 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SliceSession` connect `SliceSession` to `SliceCombat`, `PlayerMotor`, `unityengine`, `.LoadRoom`, `SliceEnemy`, `StandaloneQA`, `SliceArchive`, `SliceHUD`, `RoomCamera`, `.Apply`, `SliceCheckpoint`, `.CompleteEnding`, `WorldMap`, `SliceSound`, `MonoBehaviour`, `MemoryConsequencePlayTests`, `Phase6Review`, `PlayerPreferences`, `MovementInput`, `Phase5Review`, `SliceRoom`, `.Interact`, `OpeningExperience`, `.Remembers`?**
  _High betweenness centrality (0.109) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `SliceSession` (e.g. with `Dependencies and source comparison` and `Dependencies and scope`) actually correct?**
  _`SliceSession` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `SpawnData`, `Enemy`, `Decor` to the rest of the system?**
  _720 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `.BuildWorld` be split into smaller, more focused modules?**
  _Cohesion score 0.11578947368421053 - nodes in this community are weakly interconnected._
- **Why does `PlayerMotor` connect `PlayerMotor` to `.CaptureAll`, `SliceSession`, `SliceCombat`, `MovementLabHUD`, `unityengine`, `PlayerVisual`, `.Simulate`, `MonoBehaviour`, `MovementInput`, `RoomCamera`, `SliceSound`, `.Strike`?**
  _High betweenness centrality (0.080) - this node is a cross-community bridge._
- **Should `manifest.json` be split into smaller, more focused modules?**
  _Cohesion score 0.03333333333333333 - nodes in this community are weakly interconnected._
- **Why does `SliceEnemy` connect `SliceEnemy` to `SliceCombat`, `unityengine`, `.LoadRoom`, `SliceSession`, `SliceKing`, `SliceCheckpoint`, `SliceProjectile`, `WorldMap`, `.Strike`, `.Run`, `MonoBehaviour`, `SliceMother`, `Phase6Review`, `Phase5Review`, `SliceRoom`, `.Cue`, `.CaptureAll`, `Phase 6: the voices converge`, `.Remembers`?**
  _High betweenness centrality (0.063) - this node is a cross-community bridge._