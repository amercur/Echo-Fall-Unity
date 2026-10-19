Shader "EchoFall/Remembered Dissolve"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) _Dissolve("Forgetting",Range(0,1))=0 }
 SubShader {
 Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
 TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
 CBUFFER_START(UnityPerMaterial)
 float4 _Color;float _Dissolve;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 V vert(A v){UNITY_SETUP_INSTANCE_ID(v);SetUpSpriteInstanceProperties();V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color*_Color*unity_SpriteColor;return o;}
 half4 frag(V i):SV_Target {
 half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
 float noise=frac(sin(dot(floor(i.uv*72),float2(12.9898,78.233)))*43758.5453);
 float edge=(i.uv.y*.7+noise*.3)-_Dissolve;
 c.a*=smoothstep(0,.06,edge);return c;
 }
 ENDHLSL }
 }
}
