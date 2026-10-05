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
        public bool Remembers(string id) => memories.Contains(id);
        public bool RootsOpen => Remembers("mercy") && !Remembers("fire");
        public void Transfer(string memory)
        {
            if (!ValidId(memory)) throw new ArgumentException("Unknown memory");
            if (!memories.Contains(memory)) memories.Add(memory);
            active = memory;
            loop++;
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
                    if (candidate != path) warning = "Recovered the previous archive backup.";
                    return data;
                }
                catch (Exception e) when (e is IOException || e is ArgumentException || e is UnauthorizedAccessException)
                { warning = "Archive could not be read. A fresh in-memory loop is available."; }
            }
            return new SliceArchive();
        }
        public static void Save(string path, SliceArchive archive)
        {
            if (!archive.Valid()) throw new InvalidDataException("Invalid archive");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path + ".tmp", JsonUtility.ToJson(archive, true));
            if (File.Exists(path)) File.Replace(path + ".tmp", path, path + ".bak");
            else File.Move(path + ".tmp", path);
        }
    }
}
