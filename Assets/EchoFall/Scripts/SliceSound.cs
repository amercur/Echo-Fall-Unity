using UnityEngine;

namespace EchoFall.Movement
{
    // A small bounded synth, following the source game's procedural-audio approach.
    // No downloaded soundtrack and no dependency on Unity Services.
    public sealed class SliceSound : MonoBehaviour
    {
        AudioSource ambience;
        AudioSource[] voices;
        AudioClip drone, bell, cut, impact;
        int voice, lastLife, lastHp, lastResonance, lastCombo;
        string lastRoom;
        void Start()
        {
            drone=Tone("Cathedral air",8,55,true);
            bell=Tone("Signal bell",1.3f,440,false);
            cut=Tone("Blade breath",.14f,130,false);
            impact=Tone("Fracture",.3f,75,false);
            ambience=gameObject.AddComponent<AudioSource>(); ambience.clip=drone; ambience.loop=true; ambience.volume=.12f; ambience.Play();
            voices=new AudioSource[4]; for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].volume=.18f; }
            var s=SliceSession.Instance; lastHp=s.combat.Integrity; lastLife=s.Archive.loop;
        }
        static AudioClip Tone(string name,float seconds,float frequency,bool ambient)
        {
            const int rate=22050; var data=new float[(int)(rate*seconds)];
            for(int i=0;i<data.Length;i++)
            {
                float t=(float)i/rate;
                float envelope=ambient?(.55f+.15f*Mathf.Sin(2*Mathf.PI*t/8)):Mathf.Min(1,t*120)*Mathf.Exp(-t*5/seconds);
                data[i]=envelope*(Mathf.Sin(2*Mathf.PI*frequency*t)*.45f+Mathf.Sin(2*Mathf.PI*frequency*2*t)*.16f+Mathf.Sin(2*Mathf.PI*frequency*3*t)*.08f);
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false); clip.SetData(data,0); return clip;
        }
        void Play(AudioClip clip) { var source=voices[voice++%voices.Length]; source.Stop(); source.clip=clip; source.Play(); }
        void Update()
        {
            var s=SliceSession.Instance; if(s==null || voices==null)return;
            ambience.volume=Mathf.MoveTowards(ambience.volume,s.Playing?.12f:.04f,Time.unscaledDeltaTime*.1f);
            if(s.Room!=null && s.Room.id!=lastRoom) { lastRoom=s.Room.id; Play(bell); }
            if(s.combat.Integrity<lastHp)Play(impact);
            if(s.combat.Resonance>lastResonance || s.Archive.loop!=lastLife)Play(bell);
            if(s.combat.Combo!=lastCombo)Play(cut);
            lastHp=s.combat.Integrity; lastResonance=s.combat.Resonance; lastLife=s.Archive.loop; lastCombo=s.combat.Combo;
        }
        void OnDestroy() { foreach(var clip in new[]{drone,bell,cut,impact})if(clip!=null)Destroy(clip); }
    }
}
