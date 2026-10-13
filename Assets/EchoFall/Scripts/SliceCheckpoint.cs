using System;
using System.Collections.Generic;

namespace EchoFall.Movement
{
    // Stable IDs only. Combat timers, projectiles and Unity object references never cross a reload.
    [Serializable] public sealed class SliceEnemySnapshot
    {
        public string id;
        public float hp;
    }

    [Serializable] public sealed class SliceCheckpoint
    {
        public int version = 1, loop;
        public float resonance;
        public float motherMax;
        public int kingStage, kingBreaks, childStage, motherStage, motherRecall, motherAttack;
        public string kingStyle;
        public string archive, room, bench, decision;
        public List<string> defeated = new List<string>(), consumed = new List<string>(),
            flags = new List<string>(), visited = new List<string>();
        public List<string> decisions = new List<string>(), sacrificeUsed = new List<string>();
        public List<SliceEnemySnapshot> enemies = new List<SliceEnemySnapshot>();

        public static string Signature(SliceArchive owner)
        {
            var ids = new List<string>(owner.memories);
            ids.Sort(StringComparer.Ordinal);
            return string.Join("|", ids);
        }
        public static bool RoomId(string id) => WorldCatalog.RoomId(id);
        public static float EnemyMax(string id) => id == "archive/betrayal-guard" ? 12 : WorldCatalog.EnemyMax(id);
        static bool Unique(List<string> values, Predicate<string> valid, int limit) =>
            values != null && values.Count <= limit && values.TrueForAll(valid) &&
            new HashSet<string>(values).Count == values.Count;
        public bool Valid(SliceArchive owner)
        {
            if (owner == null || version != 1 || loop != owner.loop || archive != Signature(owner) ||
                float.IsNaN(resonance) || resonance < 1 || resonance > 3 ||
                float.IsNaN(motherMax) || motherMax<0 || motherMax>48 || motherStage<0 || motherStage>2 || motherRecall<0 || motherRecall>100000 || motherAttack<0 || motherAttack>100000 ||
                childStage<0 || childStage>3 || kingStage<0 || kingStage>2 || kingBreaks<0 || kingBreaks>10000 ||
                !(string.IsNullOrEmpty(kingStyle) || kingStyle=="air" || kingStyle=="dash" || kingStyle=="charge" || kingStyle=="ground") ||
                !WorldCatalog.Bench(room, bench) ||
                !(string.IsNullOrEmpty(decision) || decision == "mercy" || decision == "fire") ||
                !Unique(visited, RoomId, WorldCatalog.Data.rooms.Length) || !visited.Contains(room) ||
                !Unique(decisions, SliceArchive.ValidId, 9) || !Unique(sacrificeUsed, RoomId, WorldCatalog.Data.rooms.Length) ||
                !Unique(defeated, id => EnemyMax(id) > 0, WorldCatalog.EnemyCount + 1) ||
                !Unique(consumed, id => WorldCatalog.Collectible(id) || MemoryRules.NpcId(id), WorldCatalog.CollectibleCount + 5) ||
                !Unique(flags, WorldCatalog.Flag, WorldCatalog.Data.flags.Length + 1) || enemies == null || enemies.Count > WorldCatalog.EnemyCount + 1) return false;
            var ids = new HashSet<string>();
            foreach (var enemy in enemies)
                if (enemy == null || !ids.Add(enemy.id) || defeated.Contains(enemy.id) ||
                    !(enemy.hp > 0 && enemy.hp <= (enemy.id == "king/boss" && owner.Remembers("obedience") ? 48 : EnemyMax(enemy.id)))) return false;
            return true;
        }
    }
}
