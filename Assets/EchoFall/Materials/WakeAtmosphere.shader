Shader "EchoFall/Atmospheric Veil"
{
    Properties { _Color("Tint",Color)=(1,1,1,1) _MainTex("Sprite",2D)="white" {} _Motion("Motion",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float _Motion;
            CBUFFER_END
            Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; o.color=v.color*_Color; return o; }
            half4 frag(Varyings i):SV_Target
            {
                float2 p=i.uv*2-1;
                float falloff=pow(saturate(1-dot(p,p)),2);
                float mist=.84+.16*sin(i.uv.x*11+_Time.y*.12)*cos(i.uv.y*7-_Time.y*.09);
                return half4(i.color.rgb,i.color.a*falloff*lerp(1,mist,saturate(_Motion)));
            }
            ENDHLSL
        }
    }
}
