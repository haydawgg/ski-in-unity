Shader "PowderFlow/Skier Surface"
{
 Properties
 {
  [MainColor] _BaseColor("Color",Color)=(.17,.5,.34,1) [MainTexture] _BaseMap("Surface",2D)="white"{}
  _Smoothness("Smoothness",Range(0,1))=.2 _Metallic("Metallic",Range(0,1))=0
  _FillColor("Ambient floor",Color)=(.12,.15,.20,1) _Rim("Soft rim",Range(0,.3))=.14
  _Weave("Fabric variation",Range(0,.1))=.035 _Cutoff("Cutoff",Float)=0
 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Name "SkierForward" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_FillColor;float4 _BaseMap_ST;float _Smoothness,_Metallic,_Rim,_Weave,_Cutoff;
   CBUFFER_END
   struct A{float4 position:POSITION;float3 normal:NORMAL;};
   struct V{float4 position:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float3 local:TEXCOORD2;float fog:TEXCOORD3;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.position.xyz);o.position=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.local=a.position.xyz;o.fog=ComputeFogFactor(o.position.z);return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.normal),view=normalize(_WorldSpaceCameraPos-i.world);
    float detail=1-saturate(length(fwidth(i.local))*300);
    float weave=sin(i.local.x*950)*sin(i.local.y*900)*_Weave*detail;
    SurfaceData surface=(SurfaceData)0;surface.albedo=_BaseColor.rgb*(1+weave);surface.alpha=1;surface.metallic=_Metallic;surface.smoothness=_Smoothness;surface.occlusion=1;surface.normalTS=half3(0,0,1);
    surface.emission=_BaseColor.rgb*_Rim*pow(1-saturate(dot(n,view)),3);
    InputData input=(InputData)0;input.positionWS=i.world;input.normalWS=n;input.viewDirectionWS=view;input.shadowCoord=TransformWorldToShadowCoord(i.world);input.bakedGI=max(SampleSH(n),_FillColor.rgb);input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.position);input.shadowMask=half4(1,1,1,1);
    half4 color=UniversalFragmentPBR(input,surface);color.rgb=MixFog(color.rgb,i.fog);return color;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
  UsePass "Universal Render Pipeline/Lit/DepthNormals"
 }
}
