using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceEffects : MonoBehaviour
    {
        static Material material;
        LineRenderer line;
        float age;
        Color color;
        public static LineRenderer Line(GameObject go, Color color, float width)
        {
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = width; line.sortingOrder = 30; line.numCapVertices = 3;
            return line;
        }
        public static void Ring(Vector2 center, Color color, float radius)
        {
            var go = new GameObject("Signal pulse"); var line = Line(go, color, .018f); line.positionCount = 33;
            for (int i = 0; i <= 32; i++) { float a = i * Mathf.PI / 16; line.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
            var fx = go.AddComponent<SliceEffects>(); fx.line = line; fx.color = color;
        }
        public static void Slash(Vector2 center, Vector2 direction, Color color)
        {
            var go = new GameObject("Blade arc"); var line = Line(go, color, .045f); line.positionCount = 17;
            line.widthMultiplier=.14f;
            line.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.3f,.75f),new Keyframe(.6f,1),new Keyframe(1,0));
            float start = Mathf.Atan2(direction.y, direction.x) - 1.1f;
            for (int i = 0; i < 17; i++) { float a = start + i * 2.2f / 16; line.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .75f); }
            var fx = go.AddComponent<SliceEffects>(); fx.line = line; fx.color = color;
        }
        public static void Landing(Vector2 feet)
        {
            Dust(feet,Vector2.up*.2f,6);
            var go=new GameObject("Landing dust"); var color=new Color(.51f,.61f,.62f,.4f);
            var line=Line(go,color,.018f); line.positionCount=17;
            for(int i=0;i<17;i++) {float a=i*Mathf.PI/16;line.SetPosition(i,feet+new Vector2(Mathf.Cos(a)*.24f,Mathf.Sin(a)*.045f));}
            var fx=go.AddComponent<SliceEffects>();fx.line=line;fx.color=color;
        }
        void Update()
        {
            age += Time.unscaledDeltaTime; var c = color; c.a *= Mathf.Max(0,1 - age / .25f);
            line.startColor = line.endColor = c;
            if (age >= .25f) Destroy(gameObject);
        }
        public static void Dust(Vector2 position,Vector2 drift,int count)
        {
            var go=new GameObject("Wanderer motes");go.transform.position=position;
            var particles=go.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=particles.main;main.loop=false;main.duration=.35f;main.startLifetime=.28f;
            main.startSpeed=0;main.startSize=.025f;main.maxParticles=12;main.gravityModifier=.12f;
            main.simulationSpace=ParticleSystemSimulationSpace.World;main.useUnscaledTime=true;main.stopAction=ParticleSystemStopAction.Destroy;
            var emission=particles.emission;emission.enabled=false;
            var shape=particles.shape;shape.enabled=false;
            var fade=particles.colorOverLifetime;fade.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(.58f,.74f,.73f),0),new GradientColorKey(new Color(.58f,.74f,.73f),1)},
                new[]{new GradientAlphaKey(.45f,0),new GradientAlphaKey(0,1)});fade.color=gradient;
            var size=particles.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
            if(material==null)material=new Material(Shader.Find("Sprites/Default"));
            var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=12;
            particles.Play();
            for(int i=0;i<Mathf.Min(count,12);i++)
            {
                float t=count<=1?.5f:(float)i/(count-1);
                var p=new ParticleSystem.EmitParams {position=(Vector3)position+new Vector3((t-.5f)*.12f,.02f,0),velocity=drift+new Vector2((t-.5f)*.9f,.2f+Mathf.Sin(t*Mathf.PI)*.35f),startSize=.018f+(i%3)*.006f,startLifetime=.2f+(i%3)*.045f};
                particles.Emit(p,1);
            }
        }
    }
}
