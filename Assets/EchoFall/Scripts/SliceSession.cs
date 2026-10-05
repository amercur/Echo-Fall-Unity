using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoFall.Movement
{
    public enum SliceScreen { Playing, Dialogue, Transfer, Pause, Loading }
    public sealed class SliceSession : MonoBehaviour
    {
        public static SliceSession Instance { get; private set; }
        // Tests may opt into an ephemeral archive before loading the bootstrap scene.
        public static bool EphemeralSave;
        public PlayerMotor motor;
        public RoomCamera follow;
        public SliceCombat combat;
        public SliceHUD hud;
        public SliceArchive Archive { get; private set; }
        public SliceRoom Room { get; private set; }
        public SliceScreen Screen { get; private set; } = SliceScreen.Loading;
        public string Decision { get; private set; }
        public string Message { get; private set; }
        public string ModalTitle { get; private set; }
        public string ModalBody { get; private set; }
        public readonly HashSet<string> Defeated = new HashSet<string>(), Consumed = new HashSet<string>(), Flags = new HashSet<string>(), Visited = new HashSet<string>();
        public readonly Dictionary<string, float> EnemyHealth = new Dictionary<string, float>();
        public readonly List<string> OptionLabels = new List<string>();
        readonly List<Action> options = new List<Action>();
        public SliceInteraction Nearest { get; private set; }
        MovementInput movement;
        InputAction interact, pause, transfer, select1, select2, select3;
        string loadedScene;
        float messageUntil;
        bool interactPressed, pausePressed, transferPressed;
        int pendingOption = -1;
        public bool Playing => Screen == SliceScreen.Playing;
        public bool EncounterCleared => Defeated.Contains("procession/enemy-0") && Defeated.Contains("procession/enemy-1") && Defeated.Contains("procession/enemy-2") && Defeated.Contains("procession/enemy-3");
        public bool SliceComplete => Decision != null && Visited.Count == 5 && EncounterCleared && Consumed.Contains("bell-secret") && Consumed.Contains("cistern-record");
        public string Objective => Decision == null ? "Listen to the wounded creature in the Wake." : !EncounterCleared ? "Clear the four sentries on the Pilgrim Causeway." : !Consumed.Contains("bell-secret") ? "Find the Bell Keeper's record above the Belfry." : !Consumed.Contains("cistern-record") ? "Recover the Root Keeper's record in the Cistern." : Visited.Count < 5 ? "Reach the Glass Archive through the Belfry." : "The five rooms are witnessed. Return to the transfer glass.";

        void Awake()
        {
            Instance = this;
            movement = motor.GetComponent<MovementInput>();
            movement.allowReset = false;
            string warning = null;
            Archive = EphemeralSave ? new SliceArchive() : SliceMemoryStore.Load(SliceMemoryStore.DefaultPath, out warning);
            if (warning != null) Notify(warning);
            interact = Action("Interact", "<Keyboard>/e", "<Gamepad>/buttonNorth");
            pause = Action("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            transfer = Action("Transfer", "<Keyboard>/r", "<Gamepad>/select");
            select1 = Action("First", "<Keyboard>/digit1", "<Gamepad>/buttonSouth");
            select2 = Action("Second", "<Keyboard>/digit2", "<Gamepad>/buttonWest");
            select3 = Action("Third", "<Keyboard>/digit3", "<Gamepad>/buttonEast");
            interact.performed += _ => interactPressed=true;
            pause.performed += _ => pausePressed=true;
            transfer.performed += _ => transferPressed=true;
            select1.performed += _ => pendingOption=0;
            select2.performed += _ => pendingOption=1;
            select3.performed += _ => pendingOption=2;
            SetScreen(SliceScreen.Loading);
        }
        static InputAction Action(string name, string key, string pad)
        { var action = new InputAction(name, InputActionType.Button); action.AddBinding(key); action.AddBinding(pad); action.Enable(); return action; }
        IEnumerator Start() { yield return LoadRoom("wake", "default"); Notify("Find the wounded creature. What you carry will change the next Wake."); }
        void OnDestroy()
        {
            foreach (var a in new[] { interact, pause, transfer, select1, select2, select3 }) a?.Dispose();
            if (Instance == this) Instance = null;
        }
        void Update()
        {
            bool use=interactPressed, menu=pausePressed, leave=transferPressed; int option=pendingOption;
            interactPressed=pausePressed=transferPressed=false; pendingOption=-1;
            if (Screen == SliceScreen.Loading) return;
            if (!Playing)
            {
                if (option >= 0) ChooseOption(option);
                else if (menu && Screen != SliceScreen.Transfer) SetScreen(SliceScreen.Playing);
                return;
            }
            Nearest = FindNearest();
            if (menu)
                Show("THE WORLD WAITS", "A / D move  ·  SPACE jump  ·  K / SHIFT dash\nJ / X strike (hold for charged cut)  ·  W / S aim\nF tap: white deflect  ·  Hold F, release: red counter\nQ imprint / detonate  ·  H hold: mend  ·  C memory\nE interact  ·  R transfer\nController: A jump, X strike, B dash, Y interact; LB guard, RB imprint, LT memory, RT mend.", SliceScreen.Pause,
                    ("RESUME", () => SetScreen(SliceScreen.Playing)));
            else if (leave)
                Show("LEAVE THIS LIFE?", "The run ends. One decision may cross. Unfinished encounters will return.", SliceScreen.Dialogue,
                    ("TRANSFER", () => EndRun(false)), ("STAY", () => SetScreen(SliceScreen.Playing)));
            else if (use && Nearest != null) Interact(Nearest);
        }
        SliceInteraction FindNearest()
        {
            SliceInteraction best = null; float distance = float.MaxValue;
            if (Room == null) return null;
            foreach (var item in Room.GetComponentsInChildren<SliceInteraction>())
            {
                float d = Vector2.Distance(motor.Position + Vector2.up * .25f, item.transform.position);
                if (item.Available(this) && d < item.radius && d < distance) { best = item; distance = d; }
            }
            return best;
        }
        public void Notify(string text) { Message = text; messageUntil = Time.unscaledTime + 7; }
        public string CurrentMessage => Time.unscaledTime < messageUntil ? Message : "";
        public void SetScreen(SliceScreen value)
        {
            Screen = value;
            if (combat != null) combat.DiscardInput();
            motor.automaticSimulation = Playing;
            if (movement != null) movement.enabled = Playing;
            if (!Playing) Nearest = null;
        }
        public void Show(string title, string body, SliceScreen screen, params (string label, Action action)[] choices)
        {
            ModalTitle = title; ModalBody = body; OptionLabels.Clear(); options.Clear();
            foreach (var choice in choices) { OptionLabels.Add(choice.label); options.Add(choice.action); }
            SetScreen(screen);
        }
        public void ChooseOption(int index)
        { if (Playing || Screen == SliceScreen.Loading || index < 0 || index >= options.Count) return; var action = options[index]; options.Clear(); action(); }
        public void Interact(SliceInteraction item)
        {
            if (!Playing || !item.Available(this)) return;
            string locked = item.LockReason(this);
            if (locked != null) { Notify(locked); return; }
            switch (item.kind)
            {
                case "gate": StartCoroutine(LoadRoom(item.target, item.entry)); break;
                case "bench": combat.Rest(); Notify("Integrity restored. The archive persists at transfer; this bench restores this life only."); break;
                case "lever": Flags.Add(item.flag); Notify("The shortcut is open at both ends for this life."); break;
                case "cache": case "relic":
                    Consumed.Add(item.id); combat.Rest(); item.gameObject.SetActive(false);
                    Show(item.label, string.IsNullOrEmpty(item.story) ? "A stored pulse. Integrity and Resonance restored." : item.story,
                        SliceScreen.Dialogue, ("CARRY ON", () => SetScreen(SliceScreen.Playing))); break;
                case "creature":
                    if (Decision != null) { Notify("Your decision is made. Carry it through the glass."); break; }
                    if (Archive.Remembers("fire")) { Notify("A warm scar. The stones remember losing a life."); break; }
                    if (Archive.Remembers("mercy")) { combat.Heal(1); Notify("It presses its head into your hand. Living roots now cross the Cistern."); break; }
                    Show("A SMALL, WARM THING", "An injured creature curls around a living ember.\nGive two integrity to keep it alive, or take its fire into your blade.\nOnly a transferred decision survives this life.", SliceScreen.Dialogue,
                        ("GIVE 2 INTEGRITY — MERCY", () => MakeDecision("mercy")),
                        ("TAKE THE EMBER", () => MakeDecision("fire")), ("LEAVE", () => SetScreen(SliceScreen.Playing))); break;
                case "mirror":
                    if (SliceComplete) EndRun(true);
                    else Show("THE GLASS IS LISTENING", Objective + "\nYou may also leave this unfinished life and carry one decision.", SliceScreen.Dialogue,
                        ("CONTINUE EXPLORING", () => SetScreen(SliceScreen.Playing)), ("TRANSFER THIS LIFE", () => EndRun(false)));
                    break;
                default: Show(item.label, item.story, SliceScreen.Dialogue, ("CONTINUE", () => SetScreen(SliceScreen.Playing))); break;
            }
        }
        public void MakeDecision(string id)
        {
            if (Decision != null || (id != "mercy" && id != "fire")) return;
            Decision = id; SetScreen(SliceScreen.Playing);
            if (id == "mercy") combat.PayIntegrity(2);
            Notify(id == "mercy" ? "It limps into the dark. Return to the glass to remember MERCY." : "The warmth stays in your blade. The creature does not. Carry EMBER to the glass.");
        }
        public void EndRun(bool completed)
        {
            if (Screen == SliceScreen.Transfer || Screen == SliceScreen.Loading) return;
            string memory = Decision ?? "return";
            Show(completed ? "THE GLASS ACCEPTS YOU" : "THIS LIFE IS OVER", "One decision can cross.\n" +
                (memory == "return" ? "You made no new decision. RETURN remains." : memory == "mercy" ? "MERCY — the creature returns. A living root opens a path." : "EMBER — fire enters your blade. The creature becomes a scar; roots close.") +
                "\nThe world will be rebuilt. Your archive endures.", SliceScreen.Transfer,
                ("REMEMBER " + (memory == "fire" ? "EMBER" : memory.ToUpperInvariant()), () => CommitTransfer(memory)));
        }
        public void CommitTransfer(string memory)
        {
            if (Screen != SliceScreen.Transfer || memory != (Decision ?? "return")) return;
            var next = JsonUtility.FromJson<SliceArchive>(JsonUtility.ToJson(Archive)); next.Transfer(memory);
            try { if (!EphemeralSave) SliceMemoryStore.Save(SliceMemoryStore.DefaultPath, next); }
            catch (Exception e) when (e is System.IO.IOException || e is UnauthorizedAccessException)
            { Notify("Could not save the memory. Free disk space and try again; this life is preserved."); Show(ModalTitle, ModalBody, SliceScreen.Transfer, ("RETRY TRANSFER", () => CommitTransfer(memory))); return; }
            Archive = next; Decision = null; Defeated.Clear(); EnemyHealth.Clear(); Consumed.Clear(); Flags.Clear(); Visited.Clear();
            combat.Rest(); StartCoroutine(LoadRoom("wake", "default", true));
        }
        public IEnumerator LoadRoom(string id, string entry, bool newLife = false)
        {
            if (Array.IndexOf(new[] { "wake", "belfry", "cistern", "archive", "procession" }, id) < 0) yield break;
            SetScreen(SliceScreen.Loading); combat.ClearTransient();
            if (!newLife && Room != null) foreach (var enemy in Room.GetComponentsInChildren<SliceEnemy>()) EnemyHealth[enemy.id] = enemy.hp;
            if (!string.IsNullOrEmpty(loadedScene)) yield return SceneManager.UnloadSceneAsync(loadedScene);
            loadedScene = "Wake_" + id;
            yield return SceneManager.LoadSceneAsync(loadedScene, LoadSceneMode.Additive);
            Room = FindAnyObjectByType<SliceRoom>();
            Room.Apply(this); Visited.Add(id);
            motor.roomBounds = follow.bounds = Room.bounds;
            motor.EnterRoom(Room.Spawn(entry));
            Physics2D.SyncTransforms(); follow.SnapToTarget();
            SetScreen(SliceScreen.Playing);
        }
    }
}
