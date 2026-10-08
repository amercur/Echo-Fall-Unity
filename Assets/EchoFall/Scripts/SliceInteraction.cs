using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceInteraction : MonoBehaviour
    {
        public string id, kind, label, story, target, entry, requires, memory, blockedBy, flag;
        public float radius = .65f;
        public bool Available(SliceSession session) => !session.Consumed.Contains(id);
        public string LockReason(SliceSession session)
        {
            var king=SliceKing.Active;
            if(kind=="gate" && king!=null && king.Started && king.Alive)return "The King holds this arena. Defeat him or transfer this life.";
            if(requires=="king" && !session.Flags.Contains("king"))return "The King still holds this threshold.";
            if(requires=="child" && !session.Flags.Contains("child"))return "The Child has not chosen to let you pass. Listen in the garden.";
            if (!string.IsNullOrEmpty(blockedBy) && session.Archive.Remembers(blockedBy)) return "EMBER has burned this route closed.";
            if (!string.IsNullOrEmpty(memory) && !session.Archive.Remembers(memory)) return "A remembered MERCY gives this root a shape.";
            if (!string.IsNullOrEmpty(requires) && !session.Flags.Contains(requires)) return "Open this shortcut from the far side.";
            return null;
        }
    }
}
