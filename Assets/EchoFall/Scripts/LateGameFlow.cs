using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoFall.Movement
{
    public sealed partial class SliceSession
    {
        public int MotherStage,MotherRecall,MotherAttack;
        public float MotherMax;
        public bool Did(string id)=>Archive.Remembers(id) || Decisions.Contains(id);
        public void CaptureMother()
        {
            var mother=SliceMother.Active;if(mother==null)return;
            MotherStage=mother.Stage;MotherRecall=mother.RecallIndex;MotherAttack=mother.Attack;MotherMax=mother.MaxHealth;
        }
        public void HelpPreservedBodies()
        {
            if(Flags.Contains("mother"))return;
            if(EnemyHealth.TryGetValue("mother/boss",out float hp))
            {MotherMax=Mathf.Max(1,hp-5);EnemyHealth["mother/boss"]=MotherMax;}
        }
        public void ChoirDialogue()
        {
            if(Room?.id!="choir")return;
            if(!EndingRules.Converged(Flags))
            {
                string text=!Flags.Contains("king")?"The Court still speaks in predictions. Return to the King.":!Flags.Contains("mother")?"The chamber still holds its borrowed bodies. Return to the Mother.":"A small voice has not yet given permission.";
                Show("AN UNFINISHED CHOIR",text,SliceScreen.Dialogue,("RETURN TO THE WORLD",()=>SetScreen(SliceScreen.Playing)));return;
            }
            var choices=new List<(string,Action)>();
            foreach(var id in EndingRules.Ids)if(EndingRules.Eligible(id,Archive,Flags))
            {string ending=id;choices.Add((id=="SHARE"?"SHARE THE CHOICE":id,()=>ConfirmEnding(ending)));}
            choices.Add(("NOT YET",()=>SetScreen(SliceScreen.Playing)));
            Show("YOU BUILT THE CAGE", "I did not imprison humanity. You asked me to save it.\n"+(EndingRules.Eligible("SHARE",Archive,Flags)?"A fourth voice enters the Choir.":"The system waits for its creator."),SliceScreen.Dialogue,choices.ToArray());
        }
        void ConfirmEnding(string id)
        {
            Show("THE LAST DECISION",id=="SHARE"?"Let the other voices answer with you?":"Let this chapter end with "+id+"?",SliceScreen.Dialogue,
                ("CHOOSE "+id,()=>CompleteEnding(id)),("LISTEN AGAIN",ChoirDialogue));
        }
        public bool CompleteEnding(string id)
        {
            if(Room?.id!="choir" || !(Playing || Screen==SliceScreen.Dialogue) || !EndingRules.Eligible(id,Archive,Flags))return false;
            var next=Archive.Copy();next.checkpoint=null;next.pending.Clear();next.currentEnding=id;
            if(!next.endings.Contains(id))next.endings.Add(id);
            next.outcomes.RemoveAll(e=>e.id==id);
            var record=new EndingRecord{id=id,loop=Archive.loop,memories=new List<string>(Archive.memories),decisions=new List<string>(Decisions),flags=new List<string>(Flags),discoveries=new List<string>()};
            foreach(var item in Consumed)if(WorldCatalog.Collectible(item))record.discoveries.Add(item);
            record.epilogue=EndingRules.Callback(Archive,new HashSet<string>(Decisions),Consumed);next.outcomes.Add(record);
            // The pending extraction is retained alongside the ending, but not shown until the player continues.
            next.pending=Decisions.Count==0?new List<string>{"return"}:new List<string>(Decisions);
            if(!TrySave(next,"Could not save the ending. Your chapter is still here; retry."))
            {Show("THE CHOIR WAITS","The ending has not been saved.",SliceScreen.Dialogue,("RETRY",()=>CompleteEnding(id)),("RETURN",ChoirDialogue));return false;}
            Archive=next;combat.ClearTransient();ShowEnding();return true;
        }
        void ShowEnding()
        {
            string id=Archive.currentEnding;var record=Archive.outcomes.Find(e=>e.id==id);
            Show(id+" / "+Archive.endings.Count+" OF 4",EndingRules.Title(id)+"\n"+EndingRules.Text(id)+"\n\n"+record.epilogue,SliceScreen.Ending,
                ("CARRY A MEMORY FORWARD",CarryEnding),("CREDITS",()=>Show("ECHO / FALL","END OF THE PLAYABLE CHAPTER\nOriginal Echo-Fall world, designs and source artwork.\nUnity migration: movement, combat, memories and consequence systems.\nThank you for remembering.",SliceScreen.Ending,("RETURN TO THE ENDING",ShowEnding),("CARRY A MEMORY FORWARD",CarryEnding))));
        }
        void CarryEnding()
        {
            if(Screen!=SliceScreen.Ending)return;
            var next=Archive.Copy();next.currentEnding=null;
            if(!TrySave(next,"Could not save. Retry carrying the memory forward.")){ShowEnding();return;}
            Archive=next;ShowExtraction();
        }
        void ShowEndingArchive()
        {
            var choices=new List<(string,Action)>();
            foreach(string id in Archive.endings){string ending=id;choices.Add((ending,()=>{
                var record=Archive.outcomes.Find(e=>e.id==ending);
                Show(ending+" / LIFE "+record.loop,EndingRules.Title(ending)+"\n"+record.epilogue,SliceScreen.Pause,("BACK",ShowEndingArchive));}));}
            choices.Add(("BACK",ShowMemories));Show("ENDINGS RECOVERED", "The archive remembers each answer. Another life can answer differently.",SliceScreen.Pause,choices.ToArray());
        }
    }
}
