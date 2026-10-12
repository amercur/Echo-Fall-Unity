using System.Collections.Generic;

namespace EchoFall.Movement
{
    // Stable source-game identifiers; presentation names deliberately reveal little of the future.
    public static class MemoryRules
    {
        public static readonly List<string> Ids = new List<string> { "return", "mercy", "fire", "defiance", "obedience", "sacrifice", "betrayal", "respect", "abandon" };
        public static string Name(string id) => id == "fire" ? "EMBER" : id == "obedience" ? "ORDER" : id == "respect" ? "KINSHIP" : id == "abandon" ? "DISTANCE" : id.ToUpperInvariant();
        public static string Hint(string id)
        {
            switch (id)
            {
                case "return": return "A familiar rhythm. C: extended deflect.";
                case "mercy": return "Something survived you. C: spirit strike.";
                case "fire": return "Warmth leaves a scar. C: fire burst; strikes cast embers.";
                case "defiance": return "There was another way. Extra air jump; C: blink.";
                case "obedience": return "The world holds its shape. C: brief ward.";
                case "sacrifice": return "Someone remembers the cost. C: mend once per room.";
                case "betrayal": return "A hand closed around a blade. C: fury; kills mend.";
                case "respect": return "You listened. C: call a remembered voice.";
                default: return "You kept walking. Faster dash recovery; C: phase.";
            }
        }
        public static bool NpcId(string id) => id == "door" || id == "keeper" || id == "echo" || id == "archive-echo" || id == "observatory-echo";
    }
}
