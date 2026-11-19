Shader "Survivor/MagicRibbon" {
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off
  Blend SrcAlpha One, One OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
   v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float edge=abs(i.uv.y*2-1);float soft=1-smoothstep(.15,1,edge);return fixed4(i.color.rgb*(1+soft*.35),i.color.a*soft);}
  ENDCG }
 }
}
