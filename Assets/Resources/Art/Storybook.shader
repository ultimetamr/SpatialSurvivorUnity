Shader "Survivor/Storybook" {
 Properties { _Tint("Tint",Color)=(1,1,1,1) }
 SubShader { Tags {"RenderType"="Opaque" "Queue"="Geometry"} Cull Back ZWrite On
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 fixed4 _Tint;
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;fixed4 color:COLOR;UNITY_VERTEX_OUTPUT_STEREO};
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color*_Tint;return o;}
 fixed4 frag(v2f i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float3 n=normalize(i.normal),v=normalize(_WorldSpaceCameraPos-i.world),l=normalize(float3(-.45,.85,.65));float nd=dot(n,l)*.5+.5;float shade=lerp(.48,1.05,smoothstep(.22,.85,nd));float rim=pow(1-saturate(dot(n,v)),3)*.14;float spec=pow(saturate(dot(n,normalize(l+v))),48)*.08;float3 paint=i.color.rgb;
 #ifndef UNITY_COLORSPACE_GAMMA
 paint=GammaToLinearSpace(paint);
 #endif
 float3 rgb=paint*shade+float3(.12,.19,.28)*rim+spec;return fixed4(rgb,1);}
 ENDCG }
 }
}
