Shader "SpatialSurvivor/RoomOcclusion" {
 Properties { _Preview ("Show scanned surfaces",Float)=0 }
 SubShader {
 Tags { "Queue"="Geometry-1" "RenderType"="Opaque" }
 Pass {
  ZWrite Off Cull Off
  Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_instancing
  #include "UnityCG.cginc"
  float _Preview;
  struct appdata {float4 vertex:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
  struct v2f {float4 position:SV_POSITION;float3 world:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
  v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.position=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
  fixed4 frag(v2f i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);clip(_Preview-.5);float3 grid=abs(frac(i.world*4)-.5);float lines=step(.46,max(grid.x,max(grid.y,grid.z)));return fixed4(.12,.8,1,.12+lines*.38);}
  ENDCG
 }
 }
}
