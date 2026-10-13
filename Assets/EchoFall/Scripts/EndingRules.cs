using System;
using System.Collections.Generic;

namespace EchoFall.Movement
{
    [Serializable] public sealed class EndingRecord
    {
        public string id,epilogue;
        public int loop;
        public List<string> memories=new List<string>(),decisions=new List<string>(),flags=new List<string>(),discoveries=new List<string>();
        public bool Valid()=>EndingRules.ValidId(id) && loop>=1 && !string.IsNullOrEmpty(epilogue) && memories!=null && memories.Count<=5 && memories.TrueForAll(SliceArchive.ValidId) && decisions!=null && decisions.Count<=9 && decisions.TrueForAll(SliceArchive.ValidId) && flags!=null && flags.TrueForAll(WorldCatalog.Flag) && discoveries!=null && discoveries.TrueForAll(WorldCatalog.Collectible);
    }
    public static class EndingRules
    {
        public static readonly string[] Ids={"RESET","BREAK","REMEMBER","SHARE"};
        public static bool ValidId(string id)=>Array.IndexOf(Ids,id)>=0;
        public static bool Converged(ISet<string> flags)=>flags.Contains("king") && flags.Contains("mother") && flags.Contains("child");
        public static bool Eligible(string id,SliceArchive archive,ISet<string> flags)=>ValidId(id) && Converged(flags) &&
            (id!="SHARE" || archive.Remembers("respect") && (archive.Remembers("mercy") || archive.Remembers("sacrifice")) && archive.kinship>=1);
        public static string Title(string id)=>id=="RESET"?"THE KINDEST PRISON":id=="BREAK"?"AN UNWRITTEN MORNING":id=="REMEMBER"?"THE WEIGHT RETURNS":"NO SINGLE VOICE";
        public static string Text(string id)=>id=="RESET"?"The world wakes without its fear. The Child returns to the garden. You close your eyes and choose, once more, to forget.":id=="BREAK"?"The Choir falls silent. No prediction catches the falling world. Somewhere, a child sees a sunrise that has never happened before.":id=="REMEMBER"?"Every ending. Every body. Every plea. The system answers to you again. This time, you refuse the comfort of forgetting.":"You invite the abandoned selves into the Choir. Then the Child. The system learns a question it was never built to ask: what do you choose?";
        public static string Callback(SliceArchive archive,ISet<string> decisions,ISet<string> consumed)
        {
            string keeper=archive.Remembers("betrayal") || decisions.Contains("betrayal")?"One hand stays closed. The Keeper does not answer.":archive.Remembers("sacrifice") || decisions.Contains("sacrifice")?"The Keeper leaves the cages open.":"The Keeper returns to his borrowed heartbeats.";
            string creature=archive.Remembers("fire")?"Warm stones keep the shape of an absence.":archive.Remembers("mercy")?"Small paws wait beyond the glass.":"Something small still breathes in the dark.";
            string secret=consumed.Contains("vault-record") || consumed.Contains("root-secret")?"A seed survives in a pocket.":"";
            return keeper+"\n"+creature+(secret.Length>0?"\n"+secret:"");
        }
    }
}
