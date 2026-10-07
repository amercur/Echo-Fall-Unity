using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EchoFall.Movement
{
    [Serializable] public sealed class SliceArchive
    {
        public int version = 1, loop = 1;
        public string active = "return";
        public List<string> memories = new List<string>();
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
            if (!memories.Contains(memory)) memories.Add(memory);
            active = memory;
            loop++;
            checkpoint = null;
        }
        public static bool ValidId(string id) => id == "return" || id == "mercy" || id == "fire";
        public bool Valid() => version == 1 && loop >= 1 && memories != null && memories.Count <= 3 &&
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
