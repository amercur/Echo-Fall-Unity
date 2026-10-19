using System;
using System.IO;
using System.Linq;
using EchoFall.Movement;
using UnityEngine;
using UnityEditor;
public static class ImportFinishedPresentation
{
    const string Root="Assets/EchoFall/Resources/Presentation";
    public static object Run()
    {
        AssetDatabase.Refresh();int count=0;
        foreach(var file in Directory.GetFiles(Root,"*.png",SearchOption.AllDirectories))
        {
            string path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            bool player=path.Contains("/Player/");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=player?100:512f/3.6f;importer.spritePivot=new Vector2(.5f,player?.15870218f:.11775547f);
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=importer.spritePivot;settings.spriteGenerateFallbackPhysicsShape=false;importer.SetTextureSettings(settings);
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();count++;
        }
        string palettePath="Assets/EchoFall/Resources/EchoAudio.asset";
        var palette=AssetDatabase.LoadAssetAtPath<AudioPalette>(palettePath);
        if(palette==null){palette=ScriptableObject.CreateInstance<AudioPalette>();AssetDatabase.CreateAsset(palette,palettePath);}
        foreach(var field in typeof(AudioPalette).GetFields())
        {
            string name=field.Name=="ambience"?"wakeAmbience":field.Name=="step"?"stepStone":field.Name;
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/"+name+".wav");if(clip!=null)field.SetValue(palette,clip);
        }
        EditorUtility.SetDirty(palette);AssetDatabase.SaveAssets();return new{sprites=count,audio=typeof(AudioPalette).GetFields().Count(f=>f.GetValue(palette)!=null)};
    }
}
