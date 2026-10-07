using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using EchoFall.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Phase3SceneAudit
{
    public static object Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
        var scenes=new List<object>();int missing=0;
        foreach(string id in new[]{"WakeSlice","MovementLab","Wake_wake","Wake_belfry","Wake_cistern","Wake_archive","Wake_procession","Wake_king"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/"+id+".unity");
            int scripts=0,sprites=0;
            foreach(var root in scene.GetRootGameObjects())
                foreach(var t in root.GetComponentsInChildren<Transform>(true))
                {scripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);var r=t.GetComponent<SpriteRenderer>();if(r!=null&&r.sprite==null)sprites++;}
            missing+=scripts+sprites;
            var room=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
            if(room!=null)
                foreach(var gate in room.GetComponentsInChildren<SliceInteraction>(true).Where(i=>i.kind=="gate"))
                    if(!EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path.EndsWith("Wake_"+gate.target+".unity")))throw new Exception("Missing gate destination "+gate.target);
            scenes.Add(new {id,missingScripts=scripts,missingSprites=sprites});
        }
        var atlas=AssetDatabase.LoadAllAssetsAtPath("Assets/EchoFall/Art/Wake/king.png").OfType<Sprite>().ToArray();
        if(atlas.Length!=8||atlas.Any(s=>Vector2.Distance(s.pivot,new Vector2(118.597f,53.281f))>.01f))throw new Exception("King atlas/pivot mismatch");
        EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/WakeSlice.unity");
        if(missing>0)throw new Exception("Missing scene references: "+missing);
        return new {scenes,kingFrames=atlas.Length,kingPixelsPerUnit=atlas[0].pixelsPerUnit,missing};
    }
}
