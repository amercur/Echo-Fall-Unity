using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoFall.Movement.Editor
{
    public static partial class WakeSliceBuilder
    {
        public static void BuildLateGame()
        {
            SliceEnemyArt("mother");EditorSceneManager.OpenScene(Root+"/Scenes/Wake_mother.unity");
            var room=Object.FindAnyObjectByType<SliceRoom>();
            var boss=room.GetComponentInChildren<SliceMother>();
            if(boss==null){var go=new GameObject("The Mother");go.transform.SetParent(room.transform,false);boss=go.AddComponent<SliceMother>();}
            boss.transform.position=new Vector2(10.12f,.02f);boss.id="mother/boss";boss.kind="mother";boss.hp=48;boss.left=3.7f;boss.right=11.6f;
            boss.frames=AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Wake/mother.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(boss.visual==null){var go=new GameObject("Original Mother atlas");go.transform.SetParent(boss.transform,false);boss.visual=go.AddComponent<SpriteRenderer>();}
            boss.visual.sprite=boss.frames[0];boss.visual.sortingOrder=9;boss.visual.transform.localScale=Vector3.one*.65f;boss.visual.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeUnlit.mat");
            // Preserve the Phase 4 pair in the eastern vestibule; the arena has room for recalls.
            foreach(var enemy in room.GetComponentsInChildren<SliceEnemy>().Where(e=>!e.IsBoss))
            {enemy.transform.position=new Vector2(enemy.kind=="drone"?13.05f:12.55f,enemy.kind=="drone"?1.5f:0);enemy.left=12.3f;enemy.right=13.3f;}
            room.title="THE MOTHER";room.subtitle="A FUTURE WITHOUT END";
            EditorSceneManager.SaveScene(room.gameObject.scene);AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root+"/Scenes/WakeSlice.unity");
        }
    }
}
