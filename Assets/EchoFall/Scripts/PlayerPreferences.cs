using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    [Serializable] public sealed class PlayerPreferences
    {
        public float master=.7f,music=.45f,sfx=.8f,shake=1,flash=1;
        public bool largeText;
        public System.Collections.Generic.List<string> learned=new System.Collections.Generic.List<string>();
        [Serializable] public sealed class Binding{public string action,key;}
        public System.Collections.Generic.List<Binding> bindings=new System.Collections.Generic.List<Binding>();
        public static readonly string[] Rebindable={"Jump","Dash","Attack","Guard","Imprint","Mend","Memory","Interact"};
        public static readonly string[] DefaultKeys={"space","k","j","f","q","h","c","e"};
        public string EffectiveKey(string action)=>Key(action,DefaultKeys[Array.IndexOf(Rebindable,action)]).ToLowerInvariant();
        public bool Valid()=>Unit(master)&&Unit(music)&&Unit(sfx)&&Unit(shake)&&Unit(flash)&&bindings!=null&&bindings.Count<=8&&bindings.TrueForAll(b=>b!=null&&Array.IndexOf(Rebindable,b.action)>=0&&!string.IsNullOrEmpty(b.key)&&b.key.Length<30&&System.Text.RegularExpressions.Regex.IsMatch(b.key,"^[a-zA-Z0-9]+$"));
        public void Apply(InputAction action){var b=bindings.Find(x=>x.action==action.name);if(b!=null)action.ApplyBindingOverride(0,"<Keyboard>/"+b.key);}
        public string Key(string action,string fallback){var b=bindings.Find(x=>x.action==action);return b==null?fallback:b.key.ToUpperInvariant();}
        static bool Unit(float v)=>!float.IsNaN(v)&&v>=0&&v<=1;
        static PlayerPreferences current;
        public static PlayerPreferences Current=>current??(current=Load(Path.Combine(Application.persistentDataPath,"settings-v1.json")));
        public static PlayerPreferences Load(string path)
        {
            foreach(string p in new[]{path,path+".bak"})try{if(File.Exists(p)){var data=JsonUtility.FromJson<PlayerPreferences>(File.ReadAllText(p));if(data!=null&&data.Valid())return data;}}catch(Exception e)when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){}
            return new PlayerPreferences();
        }
        public bool Save(string path=null)
        {
            if(!Valid())return false;
            try
            {
                path=path??Path.Combine(Application.persistentDataPath,"settings-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                File.WriteAllText(path+".tmp",JsonUtility.ToJson(this,true));
                if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                return true;
            }
            catch(Exception e)when(e is IOException||e is ArgumentException||e is UnauthorizedAccessException){return false;}
        }
        public static float Next(float value)=>value>=.99f?0:Mathf.Min(1,value+.25f);
    }
}
