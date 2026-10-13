using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    public sealed partial class SliceSession
    {
        public readonly List<string> Decisions = new List<string>();
        public readonly HashSet<string> SacrificeUsed = new HashSet<string>();
        public int RunPower => Decisions.Contains("fire") || Decisions.Contains("betrayal") ? 1 : 0;
        List<string> extraction = new List<string>();
        readonly List<InputAction> memoryInputs = new List<InputAction>();
        public int MemoryCursor { get; private set; }
        public void RecordDecision(string id)
        { if (SliceArchive.ValidId(id) && !Decisions.Contains(id)) Decisions.Add(id); }
        void ShowExtraction(List<string> candidates = null, bool completed = false)
        {
            extraction = new List<string>(candidates ?? Archive.pending);
            var choices = new List<(string, Action)>();
            foreach (var id in extraction) { string chosen = id; choices.Add(("REMEMBER " + MemoryRules.Name(id), () => CommitTransfer(chosen))); }
            Show(completed ? "THE GLASS ACCEPTS YOU" : "ONE THING CAN CROSS", "Choose one decision from this life. The others will fade.\n" + Archive.memories.Count + " / 5 memories retained. A full archive asks what to forget.\nThe world will be rebuilt around what remains.", SliceScreen.Transfer, choices.ToArray());
        }
        void TransferSelected(string memory, string forget)
        {
            if (Screen != SliceScreen.Transfer || !extraction.Contains(memory)) return;
            if (!Archive.Remembers(memory) && Archive.memories.Count >= SliceArchive.Capacity && forget == null)
            {
                var choices = new List<(string, Action)>();
                foreach (var id in Archive.memories) { string lost = id; choices.Add(("FORGET " + MemoryRules.Name(id), () => Show("RELEASE THIS MEMORY?", "Forget " + MemoryRules.Name(lost) + " and remember " + MemoryRules.Name(memory) + "?", SliceScreen.Transfer,
                    ("REPLACE AND TRANSFER", () => TransferSelected(memory, lost)), ("BACK", () => CommitTransfer(memory))))); }
                choices.Add(("BACK", () => ShowExtraction(extraction)));
                Show("THE ARCHIVE IS FULL", "To remember " + MemoryRules.Name(memory) + ", release one memory.\nIts future influence will fade. The change is saved together with your transfer.", SliceScreen.Transfer, choices.ToArray()); return;
            }
            var next = Archive.Copy();
            if (forget != null && !next.Forget(forget)) return;
            next.Transfer(memory);
            if (!TrySave(next, "Could not save. Your archive is unchanged; retry the transfer."))
            { Show("THE GLASS WAITS", "Your decision has not been lost.", SliceScreen.Transfer, ("RETRY", () => TransferSelected(memory, forget))); return; }
            Archive = next; RebuildLife();
        }
        void RebuildLife()
        {
            Decision = null; Decisions.Clear(); SacrificeUsed.Clear(); Defeated.Clear(); EnemyHealth.Clear(); Consumed.Clear(); Flags.Clear(); Visited.Clear();
            KingStage = KingBreaks = ChildStage = MotherStage = MotherRecall = MotherAttack = 0; KingStyle = null;
            MotherMax=0;
            combat.Rest(); StartCoroutine(LoadRoom("wake", "default", true));
        }
        void ShowForgetting()
        {
            var choices = new List<(string, Action)>();
            foreach (var id in Archive.memories)
            {
                string lost = id;
                choices.Add(("FORGET " + MemoryRules.Name(id), () => Show("LET IT FADE?", "Forget " + MemoryRules.Name(lost) + " and rebuild this life at the Wake?\nThis life, its decisions, and its checkpoint will be released. Your other memories remain.", SliceScreen.Pause,
                    ("FORGET AND REBUILD", () => ForgetMemory(lost)), ("KEEP IT", ShowMemories))));
            }
            choices.Add(("BACK", ShowMemories));
            Show("THINGS LEFT BEHIND", "Forgetting removes a memory's future influence.\nRebuilding prevents old consequences lingering in visited rooms.", SliceScreen.Pause, choices.ToArray());
        }
        public bool ForgetMemory(string id)
        {
            if (!CanChangeMemory) return false;
            var next = Archive.Copy(); if (!next.Forget(id)) return false;
            next.pending.Clear();
            if (!TrySave(next, "Could not save. The memory remains.")) return false;
            Archive = next; RebuildLife(); return true;
        }
        void InitializeMemoryInput()
        {
            for(int i=3;i<9;i++)
            {
                int option=i;var action=new InputAction("Memory choice "+(i+1),InputActionType.Button,"<Keyboard>/"+(i+1));
                action.performed+=_=>pendingOption=option;action.Enable();memoryInputs.Add(action);
            }
            foreach(int direction in new[]{-1,1})
            {
                int step=direction;var action=new InputAction("Memory navigation "+direction,InputActionType.Button,"<Gamepad>/dpad/"+(direction>0?"down":"up"));
                action.performed+=_=>{if(!Playing && Screen!=SliceScreen.Loading && OptionLabels.Count>3)MemoryCursor=(MemoryCursor+OptionLabels.Count+step)%OptionLabels.Count;};
                action.Enable();memoryInputs.Add(action);
            }
        }
        bool MemoryConversation(SliceInteraction item)
        {
            if (!MemoryRules.NpcId(item.id)) return false;
            Action leave = () => SetScreen(SliceScreen.Playing);
            if(item.id=="echo" && Archive.Age("abandon")>=2 && !Archive.Remembers("respect"))
            {Show("AN EMPTY PLACE","The dust stops where someone used to stand.",SliceScreen.Dialogue,("LEAVE",leave));return true;}
            Action<string, int> decide = (memory, cost) => {
                Consumed.Add(item.id); RecordDecision(memory); SetScreen(SliceScreen.Playing);
                if(memory=="sacrifice")HelpPreservedBodies();
                if (cost > 0) combat.PayIntegrity(cost); else if(memory=="obedience")combat.GrantWard(15);else combat.Heal(memory == "betrayal" ? 6 : 1);
                if (Playing) Notify("Something about this moment stays with you.");
            };
            if(item.id=="door" && Archive.Remembers("defiance"))
            {
                Consumed.Add(item.id);combat.Heal(2);
                Show("YOUR OWN HANDWRITING","This is a transfer, not a resurrection.",SliceScreen.Dialogue,("CARRY ON",leave));return true;
            }
            if (item.id == "door")
                Show("A DOOR WITHOUT HINGES", Archive.Remembers("defiance") ? "It opens before your hand reaches the stone." : Archive.Remembers("obedience") ? "The seam is exactly where you left it." : "Beyond the seam, something waits for an answer.", SliceScreen.Dialogue,
                    ("OPEN IT / DEFIANCE", () => decide("defiance", 0)), ("SEAL IT / ORDER", () => decide("obedience", 0)), ("LEAVE", leave));
            else if (item.id == "keeper")
            {
                if (Archive.Remembers("betrayal")) { Show("THE KEEPER", "I know that hand. There is nothing here for you.", SliceScreen.Dialogue, ("LEAVE", leave)); return true; }
                if (Archive.Remembers("sacrifice"))
                    Show("THE KEEPER", "The cages are empty. You paid enough.", SliceScreen.Dialogue,
                        ("ACCEPT HIS HELP", () => { Consumed.Add(item.id); combat.Rest(); leave(); }), ("REFUSE / DISTANCE", () => decide("abandon", 0)), ("LEAVE", leave));
                else Show("THE KEEPER", "A pulse from you could keep the little ones breathing.", SliceScreen.Dialogue,
                    ("GIVE 2 INTEGRITY / SACRIFICE", () => decide("sacrifice", 2)), ("STRIKE HIM / BETRAYAL", () => decide("betrayal", 0)), ("LEAVE", leave));
            }
            else
                Show("SOMEONE YOU LEFT BEHIND", Archive.Remembers("respect") ? "You remembered my voice. That is enough." : Archive.Remembers("abandon") ? "You are good at leaving." : Archive.loop >= 3 ? "You came back. I thought you would have forgotten." : "Still me. For a little longer.", SliceScreen.Dialogue,
                    ("LISTEN / KINSHIP", () => decide("respect", 0)), ("WALK AWAY / DISTANCE", () => decide("abandon", 0)), ("STAY A MOMENT", leave));
            return true;
        }
    }
}
