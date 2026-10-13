using UnityEngine;
namespace EchoFall.Movement
{
    // A small bounded synth, following the source game's procedural-audio approach.
    // No downloaded soundtrack and no dependency on Unity Services.
    public sealed class SliceSound : MonoBehaviour
    {
        static SliceSound instance;
        AudioSource ambience;
        AudioSource[] music;
        AudioClip[] tracks;
        AudioClip step,jump,land,dash,ui,deflect;
        readonly float[] gains=new float[6];
        AudioPalette palette;
        readonly System.Collections.Generic.List<AudioClip> generated=new System.Collections.Generic.List<AudioClip>();
        int track;
        float combatHold,stepClock;
        bool grounded,dashing;
        public int MusicContext {get;private set;}=-1;
        public int MusicChanges {get;private set;}
        void Awake(){instance=this;}
        AudioSource[] voices;
        AudioClip drone, bell, cut, impact;
        int voice, lastLife, lastHp, lastCombo;
        float lastResonance;
        public static void Cue(string cue)
        {
            var sound=instance;if(sound==null || sound.voices==null)return;
            AudioClip chosen=cue=="slash"||cue=="charged"?sound.cut:cue=="hit"||cue=="heavy-hit"||cue=="armor"?sound.impact:sound.bell;
            chosen=cue=="step"?sound.step:cue=="jump"?sound.jump:cue=="land"?sound.land:cue=="dash"?sound.dash:cue=="ui"?sound.ui:cue=="deflect"||cue=="counter"?sound.deflect:chosen;
            var p=sound.palette;
            if(p!=null){var replacement=cue=="step"?p.step:cue=="jump"?p.jump:cue=="land"?p.land:cue=="dash"?p.dash:cue=="ui"?p.ui:cue=="deflect"||cue=="counter"?p.deflect:cue=="charged"?p.charged:null;if(replacement!=null)chosen=replacement;}
            sound.Play(chosen,cue=="step"?.12f:cue=="jump"||cue=="land"?.4f:1);
            if(cue=="hit"||cue=="heavy-hit"||cue=="tell"||cue=="slash")sound.combatHold=5;
        }
        string lastRoom;
        void Start()
        {
            palette=Resources.Load<AudioPalette>("EchoAudio");
            drone=Clip(palette?.ambience,"Cathedral air",8,27,true);
            bell=Clip(palette?.signal,"Signal bell",.6f,440,false);
            cut=Clip(palette?.slash,"Blade breath",.14f,130,false);
            impact=Clip(palette?.impact,"Fracture",.3f,75,false);
            step=Clip(palette?.step,"Footfall",.06f,90,false);jump=Clip(palette?.jump,"Jump",.12f,220,false);land=Clip(palette?.land,"Landing",.13f,65,false);
            dash=Clip(palette?.dash,"Dash",.15f,180,false);ui=Clip(palette?.ui,"Selection",.08f,330,false);deflect=Clip(palette?.deflect,"Deflect",.28f,660,false);
            tracks=new[]{Clip(palette?.exploration,"Exploration",8,55,true),Clip(palette?.combat,"Unrest",8,82,true),Clip(palette?.boss,"Sovereign",8,65,true),Clip(palette?.memory,"Remembering",8,110,true),Clip(palette?.ending,"Another morning",8,88,true)};
            music=new AudioSource[2];for(int i=0;i<2;i++){music[i]=gameObject.AddComponent<AudioSource>();music[i].loop=true;music[i].volume=0;}
            ambience=gameObject.AddComponent<AudioSource>(); ambience.clip=drone; ambience.loop=true; ambience.volume=.12f; ambience.Play();
            voices=new AudioSource[6]; for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].volume=.18f; }
            var s=SliceSession.Instance; lastHp=s.combat.Integrity; lastLife=s.Archive.loop;
        }
        AudioClip Clip(AudioClip authored,string name,float seconds,float frequency,bool ambient)
        {if(authored!=null)return authored;var clip=Tone(name,seconds,frequency,ambient);generated.Add(clip);return clip;}
        static AudioClip Tone(string name,float seconds,float frequency,bool ambient)
        {
            const int rate=22050; var data=new float[(int)(rate*seconds)];
            for(int i=0;i<data.Length;i++)
            {
                float t=(float)i/rate;
                float envelope=ambient?(.55f+.15f*Mathf.Sin(2*Mathf.PI*t/8)):Mathf.Min(1,t*120)*Mathf.Pow(1-t/seconds,3);
                data[i]=envelope*(Mathf.Sin(2*Mathf.PI*frequency*t)*.45f+Mathf.Sin(2*Mathf.PI*frequency*2*t)*.16f+Mathf.Sin(2*Mathf.PI*frequency*3*t)*.08f);
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false); clip.SetData(data,0); return clip;
        }
        void Play(AudioClip clip,float gain=1) {int i=voice++%voices.Length;gains[i]=gain; var source=voices[i]; source.Stop(); source.clip=clip;source.volume=.18f*gain*PlayerPreferences.Current.master*PlayerPreferences.Current.sfx; source.Play(); }
        void Update()
        {
            var s=SliceSession.Instance; if(s==null || voices==null)return;
            var p=PlayerPreferences.Current;float dt=Time.unscaledDeltaTime;
            ambience.volume=(s.Playing?.07f:.025f)*p.master*p.sfx;
            for(int i=0;i<voices.Length;i++)voices[i].volume=.18f*gains[i]*p.master*p.sfx;
            if(s.Playing)
            {
                combatHold=Mathf.Max(0,combatHold-dt);
                if(s.motor.Grounded&&!grounded)Cue("land");
                if(!s.motor.Grounded&&grounded&&s.motor.Velocity.y>0)Cue("jump");
                if(s.motor.Dashing&&!dashing)Cue("dash");
                stepClock-=dt;if(s.motor.Grounded&&Mathf.Abs(s.motor.Velocity.x)>1&&!s.motor.Dashing&&stepClock<=0){Cue("step");stepClock=.28f;}
            }
            grounded=s.motor.Grounded;dashing=s.motor.Dashing;
            var king=SliceKing.Active;var mother=SliceMother.Active;
            int next=s.Screen==SliceScreen.Ending?4:s.Screen==SliceScreen.Transfer?3:(king!=null&&king.Started&&king.Alive||mother!=null&&mother.Started&&mother.Alive)?2:combatHold>0?1:0;
            if(s.Screen==SliceScreen.Pause||s.Screen==SliceScreen.Dialogue||s.Screen==SliceScreen.Map||s.Screen==SliceScreen.Loading)next=Mathf.Max(0,MusicContext);
            if(next!=MusicContext){MusicContext=next;MusicChanges++;track=1-track;music[track].Stop();music[track].clip=tracks[next];music[track].volume=0;music[track].Play();}
            for(int i=0;i<2;i++)music[i].volume=Mathf.MoveTowards(music[i].volume,i==track?.2f*p.master*p.music:0,dt*.12f);
            if(p.master==0||p.music==0)foreach(var source in music)source.volume=0;
            if(s.combat.Integrity<lastHp)Play(impact);
            if(s.combat.Resonance>lastResonance || s.Archive.loop!=lastLife)Play(bell);

            lastHp=s.combat.Integrity; lastResonance=s.combat.Resonance; lastLife=s.Archive.loop; lastCombo=s.combat.Combo;
        }
        void OnDestroy() {if(instance==this)instance=null; foreach(var clip in generated)if(clip!=null)Destroy(clip); }
    }
}
