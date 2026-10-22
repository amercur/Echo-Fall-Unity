using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class Phase8Audit
{
    public static async Task<string> Run()
    {
        bool ephemeral=SliceSession.EphemeralSave,bg=Application.runInBackground;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;
        try {
            Directory.CreateDirectory("Docs/Validation/phase8");
            await SceneManager.LoadSceneAsync("MovementLab");await Task.Delay(100);
            await SceneManager.LoadSceneAsync("WakeSlice");float limit=Time.realtimeSinceStartup+20;
            while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Bootstrap timeout");await Task.Delay(20);}
            var s=SliceSession.Instance;await Task.Delay(300);ScreenCapture.CaptureScreenshot("Docs/Validation/phase8/before-wake.png");await Task.Delay(100);
            var info=s.Room.GetComponentsInChildren<SliceInteraction>().Select(i=>i.id+" | "+i.kind+" | "+i.transform.position+" | "+i.story).ToArray();
            File.WriteAllLines("Docs/Validation/phase8/opening-interactions.txt",info);
            var warden=s.Room.GetComponentsInChildren<SliceEnemy>().Single(e=>e.training);warden.enabled=false;
            s.combat.PayIntegrity(5);s.combat.ReceiveHit(6,s.motor.Position+Vector2.right,true,warden,true);
            bool safe=s.combat.Integrity==1;s.combat.Rest();warden.Deflected(false);
            var creature=s.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="creature");s.Interact(creature);await Task.Delay(200);
            ScreenCapture.CaptureScreenshot("Docs/Validation/phase8/before-choice.png");await Task.Delay(100);
            return "Screen starts directly in play; training nonfatal="+safe+"; deflect completes="+s.Defeated.Contains(warden.id)+"; creature choices="+string.Join(" / ",s.OptionLabels)+"; frame="+Time.frameCount;
        }finally{SliceSession.EphemeralSave=ephemeral;Application.runInBackground=bg;}
    }
}
