using System;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;

// Run with the Editor's Pipeline run_script command in Play Mode. Never writes a player save.
public static class CheckpointMenuReview
{
    public static async Task<object> Show()
    {
        SliceSession.EphemeralSave = true;
        bool background = Application.runInBackground;
        Application.runInBackground = true;
        try
        {
            int before = Time.frameCount;
            float deadline = Time.realtimeSinceStartup + 12;
            while (SliceSession.Instance == null || !SliceSession.Instance.Playing)
            {
                if (Time.realtimeSinceStartup > deadline) throw new Exception("Wake did not become ready");
                await Task.Delay(50);
            }
            var session = SliceSession.Instance;
            session.Archive.Transfer("mercy");
            session.Archive.Transfer("return");
            session.Archive.Transfer("fire");
            session.motor.EnterRoom(new Vector2(1.45f, .02f));
            session.follow.SnapToTarget();
            session.ShowMemories();
            await Task.Delay(500);
            if (Time.frameCount <= before) throw new Exception("Game frames did not advance");
            return new { before, after = Time.frameCount, screen = session.Screen.ToString(),
                choices = session.OptionLabels.ToArray(), ephemeral = SliceSession.EphemeralSave };
        }
        finally { Application.runInBackground = background; }
    }
}
