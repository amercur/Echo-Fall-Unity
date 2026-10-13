using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EchoFall.Movement
{
    [Serializable] public sealed class MemoryAge { public string id; public int loop; }
    [Serializable] public sealed class SliceArchive
    {
        public int version = 1, loop = 1;
        public string active = "return";
        public List<string> memories = new List<string>();
        public const int Capacity = 5;
        public List<MemoryAge> acquired = new List<MemoryAge>();
        public List<string> pending = new List<string>();
        public int kinship;
        public List<string> endings = new List<string>();
        public List<EndingRecord> outcomes = new List<EndingRecord>();
        public string currentEnding;
        public string lastEcho = "return";
        public int Age(string id) { var stamp = acquired?.Find(a => a != null && a.id == id); return Remembers(id) && stamp != null ? loop - stamp.loop : 0; }
        public bool MatureMercy => RootsOpen && Age("mercy") >= 3;
        public bool Forget(string id)
        {
            if (!memories.Remove(id)) return false;
            acquired?.RemoveAll(a => a.id == id);
            if (active == id) active = memories.Count == 0 ? "return" : memories[0];
            if (lastEcho == id) lastEcho = active;
            checkpoint = null; return true;
        }
        // Unity's inline JSON serialization materializes null class fields as empty objects.
        // A presence marker distinguishes old/archive-only saves from corrupt checkpoints.
        public bool hasCheckpoint;
        public SliceCheckpoint benchSave;
        public SliceCheckpoint checkpoint
        {
            get => hasCheckpoint ? benchSave : null;
            set { benchSave = value; hasCheckpoint = value != null; }
        }
        public SliceArchive Copy() => JsonUtility.FromJson<SliceArchive>(JsonUtility.ToJson(this));
        public bool CanEquip(string id) => ValidId(id) && (memories.Contains(id) || (memories.Count == 0 && id == "return"));
        public void Equip(string id)
        {
            if (!CanEquip(id)) throw new ArgumentException("Memory has not been retained");
            active = id;
        }
        public bool Remembers(string id) => memories.Contains(id);
        public bool RootsOpen => Remembers("mercy") && !Remembers("fire");
        public void Transfer(string memory)
        {
            if (!ValidId(memory)) throw new ArgumentException("Unknown memory");
            if (!memories.Contains(memory))
            {
                if (memories.Count >= Capacity) throw new InvalidOperationException("Forget a memory first");
                memories.Add(memory); acquired.Add(new MemoryAge { id = memory, loop = loop });
            }
            if (memory == "respect") kinship++;
            lastEcho = memory; pending.Clear();currentEnding=null;
            active = memory;
            loop++;
            checkpoint = null;
        }
        public static bool ValidId(string id) => MemoryRules.Ids.Contains(id);
        public bool Valid() => endings!=null && endings.Count<=4 && endings.TrueForAll(EndingRules.ValidId) && new HashSet<string>(endings).Count==endings.Count &&
            outcomes!=null && outcomes.Count==endings.Count && outcomes.TrueForAll(e=>e!=null && e.Valid() && endings.Contains(e.id)) && new HashSet<string>(outcomes.ConvertAll(e=>e.id)).Count==outcomes.Count &&
            (string.IsNullOrEmpty(currentEnding) || endings.Contains(currentEnding)) && version == 1 && loop >= 1 && kinship >= 0 && memories != null && memories.Count <= Capacity &&
            acquired != null && acquired.Count <= Capacity && acquired.TrueForAll(a => a != null && memories.Contains(a.id) && a.loop >= 1 && a.loop <= loop) &&
            new HashSet<string>(acquired.ConvertAll(a => a.id)).Count == acquired.Count &&
            pending != null && pending.Count <= 9 && pending.TrueForAll(ValidId) && new HashSet<string>(pending).Count == pending.Count && ValidId(lastEcho) &&
            memories.TrueForAll(ValidId) && new HashSet<string>(memories).Count == memories.Count && ValidId(active) &&
            (memories.Count == 0 ? active == "return" : memories.Contains(active));
    }

    public static class SliceMemoryStore
    {
        public static string DefaultPath => Path.Combine(Application.persistentDataPath, "wake-slice-v1.json");
        public static SliceArchive Load(string path, out string warning)
        {
            warning = null;
            foreach (var candidate in new[] { path, path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    var data = JsonUtility.FromJson<SliceArchive>(File.ReadAllText(candidate));
                    if (data == null || !data.Valid()) throw new InvalidDataException("Invalid archive");
                    // Legacy saves lack acquisition dates. Start their delay now, never invent old age.
                    foreach (var id in data.memories)
                        if (!data.acquired.Exists(a => a.id == id)) data.acquired.Add(new MemoryAge { id = id, loop = data.loop });
                    if (candidate != path)
                    {
                        // Backups may precede death/transfer. Recover identity, never resurrect that run.
                        data.checkpoint = null;
                        warning = "Recovered the previous archive backup. Starting a fresh Wake.";
                    }
                    // An invalid checkpoint must not discard a valid persistent archive.
                    if (data.hasCheckpoint && (data.checkpoint == null || !data.checkpoint.Valid(data)))
                    { data.checkpoint = null; warning = "The bench save is incompatible. Your archive is intact; returning to the Wake."; }
                    return data;
                }
                catch (Exception e) when (e is IOException || e is ArgumentException || e is UnauthorizedAccessException)
                { warning = "Archive could not be read. A fresh in-memory loop is available."; }
            }
            return new SliceArchive();
        }
        public static void Save(string path, SliceArchive archive)
        {
            if (archive == null || !archive.Valid() || (archive.hasCheckpoint && (archive.checkpoint == null || !archive.checkpoint.Valid(archive))))
                throw new InvalidDataException("Invalid archive or checkpoint");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path + ".tmp", JsonUtility.ToJson(archive, true));
            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);
        }
    }
}
