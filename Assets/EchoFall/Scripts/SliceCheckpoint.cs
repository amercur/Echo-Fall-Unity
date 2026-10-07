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
        public int kingStage, kingBreaks;
        public string kingStyle;
        public string archive, room, bench, decision;
        public List<string> defeated = new List<string>(), consumed = new List<string>(),
            flags = new List<string>(), visited = new List<string>();
        public List<SliceEnemySnapshot> enemies = new List<SliceEnemySnapshot>();

        public static string Signature(SliceArchive owner)
        {
            var ids = new List<string>(owner.memories);
            ids.Sort(StringComparer.Ordinal);
            return string.Join("|", ids);
        }
        public static bool RoomId(string id) => id == "wake" || id == "belfry" ||
            id == "cistern" || id == "archive" || id == "procession" || id == "king";
        public static float EnemyMax(string id)
        {
            switch (id)
            {
                case "wake/enemy-0": case "procession/enemy-0": case "procession/enemy-3": return 7;
                case "procession/enemy-1": return 10;
                case "king/boss": return 40;
                case "belfry/enemy-0": case "cistern/enemy-0": case "cistern/enemy-1": case "procession/enemy-2": return 5;
                default: return 0;
            }
        }
        static bool Unique(List<string> values, Predicate<string> valid, int limit) =>
            values != null && values.Count <= limit && values.TrueForAll(valid) &&
            new HashSet<string>(values).Count == values.Count;
        public bool Valid(SliceArchive owner)
        {
            if (owner == null || version != 1 || loop != owner.loop || archive != Signature(owner) ||
                float.IsNaN(resonance) || resonance < 1 || resonance > 3 ||
                kingStage<0 || kingStage>2 || kingBreaks<0 || kingBreaks>10000 ||
                !(string.IsNullOrEmpty(kingStyle) || kingStyle=="air" || kingStyle=="dash" || kingStyle=="charge" || kingStyle=="ground") ||
                !((room == "wake" && bench == "rest-wake") || (room == "archive" && bench == "archive-rest")) ||
                !(string.IsNullOrEmpty(decision) || decision == "mercy" || decision == "fire") ||
                !Unique(visited, RoomId, 6) || !visited.Contains(room) ||
                !Unique(defeated, id => EnemyMax(id) > 0, 9) ||
                !Unique(consumed, id => id == "bell-secret" || id == "pogo-secret" || id == "belfry-cache" || id == "cistern-cache", 4) ||
                !Unique(flags, id => id == "sluice" || id == "archive-lift" || id == "king", 3) || enemies == null || enemies.Count > 9) return false;
            var ids = new HashSet<string>();
            foreach (var enemy in enemies)
                if (enemy == null || !ids.Add(enemy.id) || defeated.Contains(enemy.id) ||
                    !(enemy.hp > 0 && enemy.hp <= EnemyMax(enemy.id))) return false;
            return true;
        }
    }
}
