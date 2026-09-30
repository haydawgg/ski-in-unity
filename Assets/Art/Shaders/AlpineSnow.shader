Shader "PowderFlow/Alpine Snow"
{
 Properties
 {
  _BaseColor("Snow tint",Color)=(.82,.88,.95,1) _RockColor("Rock",Color)=(.16,.19,.25,1)
  _RippleStrength("Wind normal strength",Range(0,.3))=.022 _RippleScale("Wind scale",Float)=1.8
  _Variation("Snow variation",Range(0,.2))=.065 _Glitter("Sparkle",Range(0,.5))=.16
  _DetailDistance("Detail fade distance",Float)=80 _Wrap("Wrap light",Range(0,.5))=.16
  _RimStrength("Snow rim",Range(0,.2))=.055 _SlopeBlend("Slope rock blend",Range(0,1))=1 _Cull("Cull",Float)=2
 }
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
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #pragma multi_compile _ LOD_FADE_CROSSFADE
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_RockColor;float _RippleStrength,_RippleScale,_Variation,_Glitter,_DetailDistance,_Wrap,_RimStrength,_SlopeBlend,_Cull;
   CBUFFER_END
   struct A {float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 vertex:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float fog:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
   V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_TRANSFER_INSTANCE_ID(a,o);o.world=TransformObjectToWorld(a.vertex.xyz);o.vertex=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(o.vertex.z);return o;}
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p)
   {
    float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(hash21(q),hash21(q+float2(1,0)),f.x),lerp(hash21(q+float2(0,1)),hash21(q+1),f.x),f.y);
   }
   half4 frag(V i):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    #ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(i.vertex);
    #endif
    float3 n=normalize(i.normal);float3 view=normalize(_WorldSpaceCameraPos-i.world);
    float distance=length(_WorldSpaceCameraPos-i.world);float detail=1-smoothstep(_DetailDistance*.25,_DetailDistance,distance);
    float2 p=i.world.xz*_RippleScale;float windNoise=noise21(i.world.xz*.25);float warp=(windNoise-.5)*4;
    float a=dot(p,float2(.75,1.55))+warp,b=dot(p,float2(-1.8,2.4))-warp*.6;
    float ripple=(sin(a)+sin(b)*.32)/1.32*(.35+windNoise*.65);
    float2 slope=float2(.75,1.55)*cos(a)+float2(-1.8,2.4)*cos(b)*.12;
    float3 perturb=float3(-slope.x,0,-slope.y);perturb-=n*dot(n,perturb);
    n=normalize(n+perturb*_RippleStrength*detail*(.35+windNoise*.65));
    float snow=lerp(1,smoothstep(.48,.77,i.normal.y),_SlopeBlend);
    float broad=noise21(i.world.xz*.12)*2-1;
    half3 snowColor=_BaseColor.rgb*(1+_Variation*(broad*.85+ripple*.15*detail));
    half3 rockColor=_RockColor.rgb*lerp(.76,1.20,noise21(i.world.xz*.24+i.world.y*.07));
    half3 albedo=lerp(rockColor,snowColor,snow);
    Light light=GetMainLight(TransformWorldToShadowCoord(i.world));float diffuse=saturate((dot(n,light.direction)+_Wrap)/(1+_Wrap));
    AmbientOcclusionFactor ao=GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(i.vertex));
    half3 ambient=SampleSH(n)*ao.indirectAmbientOcclusion;
    half3 color=albedo*(ambient*.85+light.color*diffuse*light.shadowAttenuation*ao.directAmbientOcclusion);
    float random=hash21(floor(i.world.xz*28));
    float antialias=1-saturate(length(fwidth(i.world.xz))*14);
    float sparkle=step(.995,random)*pow(saturate(dot(reflect(-light.direction,n),view)),48)*_Glitter*detail*antialias*snow;
    color+=sparkle*light.color*light.shadowAttenuation;
    color+=_BaseColor.rgb*light.color*pow(1-saturate(dot(n,view)),4)*_RimStrength*snow*light.shadowAttenuation;
    return half4(MixFog(color,i.fog),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
  UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
