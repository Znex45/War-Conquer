Shader "WarConquer/NeonGlow"
{
 Properties { _BaseColor("Tint",Color)=(1,1,1,1) _SoftParticle("Radial glow",Float)=1 }
 SubShader
 {
  Tags { "Queue"="Transparent+5" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Pass
  {
   Tags { "LightMode"="SRPDefaultUnlit" }
   Blend SrcAlpha One
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0; };
   struct v2f { float4 position:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0; };
   float4 _BaseColor;float _SoftParticle;
   v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.color=v.color*_BaseColor;o.uv=v.uv;return o;}
   half4 frag(v2f i):SV_Target
   {
    float d=length((i.uv-.5)*2);float radial=exp(-d*d*4)*saturate(1-d);
    float edge=saturate(1-abs(i.uv.y*2-1));
    float glow=lerp(edge,radial,_SoftParticle);
    return half4(i.color.rgb*(1+glow*.8),i.color.a*glow);
   }
   ENDHLSL
  }
 }
}
