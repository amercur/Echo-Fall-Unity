using System;
using UnityEngine;

namespace EchoFall.Movement
{
    [Serializable] public struct SliceSpawn { public string id; public Vector2 feet; }
    public sealed class SliceRoom : MonoBehaviour
    {
        public string id, title, subtitle;
        public Rect bounds;
        public SliceSpawn[] spawns;
        public Transform rootBridge, returnedCreature, emberScar;
        public Vector2 Spawn(string entry)
        {
            foreach (var spawn in spawns) if (spawn.id == entry) return spawn.feet;
            return spawns[0].feet;
        }
        public void Apply(SliceSession session)
        {
            if (rootBridge != null) rootBridge.gameObject.SetActive(session.Archive.RootsOpen);
            if (returnedCreature != null) returnedCreature.gameObject.SetActive(session.Archive.RootsOpen);
            if (emberScar != null) emberScar.gameObject.SetActive(session.Archive.Remembers("fire"));
            foreach (var e in GetComponentsInChildren<SliceEnemy>())
            {
                if(e is SliceKing king)king.RestorePrediction(session.KingStage,session.KingBreaks,session.KingStyle);
                if (session.Defeated.Contains(e.id)) e.gameObject.SetActive(false);
                else if (session.EnemyHealth.TryGetValue(e.id, out float hp)) e.hp = hp;
            }
            foreach (var item in GetComponentsInChildren<SliceInteraction>())
            {
                if (session.Consumed.Contains(item.id)) item.gameObject.SetActive(false);
                if (item.kind == "creature" && (session.Archive.Remembers("mercy") || session.Archive.Remembers("fire")))
                {
                    foreach (var line in item.GetComponentsInChildren<LineRenderer>()) line.enabled=false;
                    foreach (var sprite in item.GetComponentsInChildren<SpriteRenderer>()) sprite.enabled=false;
                    item.label = session.Archive.Remembers("fire") ? "THE EMBER SCAR" : "THE RETURNED CREATURE";
                }
            }
        }
    }
}
