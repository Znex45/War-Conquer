Shader "WarConquer/TerrainTransparent"
{
    Properties { _BaseColor("Color", Color)=(1,1,1,1) _SoftParticle("Soft particle",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 position:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            float4 _BaseColor; float _SoftParticle;
            v2f vert(appdata v) { v2f o; o.position=UnityObjectToClipPos(v.vertex); o.color=v.color*_BaseColor; o.uv=v.uv; return o; }
            half4 frag(v2f i):SV_Target { half4 color=i.color; float2 d=(i.uv-.5)*2; color.a*=lerp(1,saturate(1-dot(d,d)),_SoftParticle); return color; }
            ENDHLSL
        }
    }
}
