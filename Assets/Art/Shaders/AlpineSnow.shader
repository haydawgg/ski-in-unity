Shader "PowderFlow/Alpine Snow"
{
 Properties { _BaseColor("Snow tint",Color)=(.83,.89,.97,1) _RockColor("Rock",Color)=(.13,.15,.23,1) _RippleStrength("Wind ripple",Float)=.045 _Glitter("Sparkle",Float)=.25 _Cull("Cull",Float)=2 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
  Pass
  {
   Name "SnowForward" Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_RockColor;float _RippleStrength,_Glitter,_Cull;
   CBUFFER_END
   struct A {float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 vertex:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float fog:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
   V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_TRANSFER_INSTANCE_ID(a,o);o.world=TransformObjectToWorld(a.vertex.xyz);o.vertex=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(o.vertex.z);return o;}
   half4 frag(V i):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    float3 n=normalize(i.normal);float3 view=normalize(_WorldSpaceCameraPos-i.world);
    float distance=length(_WorldSpaceCameraPos-i.world);float near=1-saturate(distance/90);
    float ripple=sin(i.world.x*.8+i.world.z*1.7)+.22*sin(i.world.z*5-i.world.x*.7);
    n=normalize(n+float3(cos(i.world.x*.8+i.world.z*1.7)*_RippleStrength,0,cos(i.world.z*5-i.world.x*.7)*_RippleStrength)*near);
    float snow=smoothstep(.48,.77,i.normal.y);
    half3 albedo=lerp(_RockColor.rgb,_BaseColor.rgb*(.965+.025*ripple),snow);
    Light light=GetMainLight(TransformWorldToShadowCoord(i.world));float diffuse=saturate(dot(n,light.direction));
    half3 ambient=SampleSH(n);half3 color=albedo*(ambient*.85+light.color*(diffuse*.88+.12)*light.shadowAttenuation);
    float random=frac(sin(dot(floor(i.world.xz*28),float2(12.9898,78.233)))*43758.5453);
    float sparkle=step(.989,random)*pow(saturate(dot(reflect(-light.direction,n),view)),45)*_Glitter*near*snow;
    color+=sparkle*light.color*light.shadowAttenuation;
    color+=_BaseColor.rgb*pow(1-saturate(dot(n,view)),4)*.05*snow;
    return half4(MixFog(color,i.fog),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
  UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
