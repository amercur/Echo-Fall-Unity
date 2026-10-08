using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EchoFall.Movement
{
    [Serializable] public sealed class WorldLink
    {
        public string id,target,entry,requires,memory,blockedBy;
        public bool Open(SliceArchive archive, ISet<string> flags) =>
            (string.IsNullOrEmpty(requires) || flags.Contains(requires)) &&
            (string.IsNullOrEmpty(memory) || archive.Remembers(memory)) &&
            (string.IsNullOrEmpty(blockedBy) || !archive.Remembers(blockedBy));
    }
    [Serializable] public sealed class WorldPlace
    {
        public string id,name,region;
        public float x,y;
        public bool hidden;
        public string[] benches,collectibles;
        public SliceEnemySnapshot[] enemies;
        public WorldLink[] links;
    }
    [Serializable] public sealed class WorldDefinition { public WorldPlace[] rooms; public string[] flags; }
    // One authored manifest owns runtime loading, map connectivity and saved-ID validation.
    public static class WorldCatalog
    {
        static WorldDefinition data;
        public static WorldDefinition Data => data ?? (data=JsonUtility.FromJson<WorldDefinition>(Resources.Load<TextAsset>("WorldCatalog").text));
        public static WorldPlace Find(string id) => Array.Find(Data.rooms,r=>r.id==id);
        public static bool RoomId(string id) => Find(id)!=null;
        public static bool Bench(string room,string id) => Find(room)?.benches.Contains(id)==true;
        public static bool Collectible(string id) => Data.rooms.Any(r=>r.collectibles.Contains(id));
        public static bool Flag(string id) => Data.flags.Contains(id);
        public static float EnemyMax(string id) => Data.rooms.SelectMany(r=>r.enemies).FirstOrDefault(e=>e.id==id)?.hp ?? 0;
        public static int EnemyCount => Data.rooms.Sum(r=>r.enemies.Length);
        public static int CollectibleCount => Data.rooms.Sum(r=>r.collectibles.Length);
        public static HashSet<string> Visible(ISet<string> discovered)
        {
            var result=new HashSet<string>(discovered);
            foreach(var id in discovered)
                foreach(var link in Find(id).links)
                    if(Find(link.target)?.hidden==false)result.Add(link.target);
            return result;
        }
    }
}
