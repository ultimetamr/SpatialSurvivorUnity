Shader "Survivor/RoomTheme" {
 SubShader {
  // Render before actors; no room decoration writes depth over a creature/UI.
  Tags {"Queue"="Geometry-20" "RenderType"="Transparent"}
  Cull Back ZWrite Off Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;fixed4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
   v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float3 n=abs(normalize(i.normal));float2 uv=n.y>.7?i.world.xz:n.z>n.x?i.world.xy:i.world.zy;
    float2 cell=abs(frac(uv*1.5)-.5);float grout=smoothstep(.46,.49,max(cell.x,cell.y));
    float light=.78+.22*abs(dot(normalize(i.normal),normalize(float3(-.3,.8,.5))));
    float3 paint=i.color.rgb;
    #ifndef UNITY_COLORSPACE_GAMMA
    paint=GammaToLinearSpace(paint);
    #endif
    return fixed4(paint*light*(1-grout*.24),.94);
   }
  ENDCG }
 }
}
