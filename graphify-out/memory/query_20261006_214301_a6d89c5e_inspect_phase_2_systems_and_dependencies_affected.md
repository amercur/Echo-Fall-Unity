---
type: "query"
date: "2026-10-06T21:43:01.599090+00:00"
question: "Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work"
contributor: "graphify"
outcome: "useful"
source_nodes: ["SliceSession", "SliceArchive", "SliceEnemy"]
---

# Q: Inspect Phase 2 systems and dependencies affected by remaining checkpoint and combat work

## Answer

Expanded from graph vocabulary: slice session combat archive checkpoint transfer room wake visual test memory. SliceSession references SliceArchive (Assets/EchoFall/Scripts/SliceSession.cs and SliceMemory.cs). SliceEnemy is referenced by SliceCombat, SliceProjectile, SliceSession.LoadRoom and SliceRoom.Apply; enemy changes therefore warrant combat, projectile, room-transition and memory-application checks. Checkpoint/loadout persistence is documented as remaining work in Docs/Phase2-WakeSlice.md and relates to the migration loadout design. Graph is a navigation aid: raw extraction flagged 109 dangling-endpoint edges, 2 self-loops, and 131 undirected collapsed edges; Unity serialized scene and asset files are unclassified. Verify affected source before editing.

## Outcome

- Signal: useful

## Source Nodes

- SliceSession
- SliceArchive
- SliceEnemy