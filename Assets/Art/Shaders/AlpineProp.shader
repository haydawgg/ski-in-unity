Shader "PowderFlow/Alpine Prop"
{
 Properties
 {
  [MainColor] _BaseColor("Color",Color)=(.16,.38,.4,1)
  [MainTexture] _BaseMap("Surface",2D)="white"{}
  _Smoothness("Smoothness",Range(0,1))=.2 _Metallic("Metallic",Range(0,1))=0
  _FillColor("Ambient floor",Color)=(.65,.68,.74,1)
  _EmissionColor("Emission",Color)=(0,0,0,1) _Cutoff("Cutoff",Float)=0
 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
  half4 _BaseColor,_FillColor,_EmissionColor;float4 _BaseMap_ST;float _Smoothness,_Metallic,_Cutoff;
  CBUFFER_END
  ENDHLSL
  Pass
  {
   Name "AlpinePropForward" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A{float4 position:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V{float4 position:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float fog:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
   V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_TRANSFER_INSTANCE_ID(a,o);o.world=TransformObjectToWorld(a.position.xyz);o.position=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(o.position.z);return o;}
   half4 frag(V i):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);
    float3 n=normalize(i.normal),view=normalize(_WorldSpaceCameraPos-i.world);
    SurfaceData surface=(SurfaceData)0;surface.albedo=_BaseColor.rgb;surface.alpha=1;surface.metallic=_Metallic;surface.smoothness=_Smoothness;surface.occlusion=1;surface.normalTS=half3(0,0,1);surface.emission=_EmissionColor.rgb;
    InputData input=(InputData)0;input.positionWS=i.world;input.normalWS=n;input.viewDirectionWS=view;input.shadowCoord=TransformWorldToShadowCoord(i.world);input.bakedGI=max(SampleSH(n),_FillColor.rgb);input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.position);input.shadowMask=half4(1,1,1,1);
    half4 color=UniversalFragmentPBR(input,surface);
    // Retain shaded pigment/letter contrast even when screen-space AO is strong.
    color.rgb=max(color.rgb,_BaseColor.rgb*_FillColor.rgb*.75);
    color.rgb=MixFog(color.rgb,i.fog);return color;
   }
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual ColorMask 0
   HLSLPROGRAM
   #pragma vertex ShadowPassVertex
   #pragma fragment ShadowPassFragment
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"}
   ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma vertex DepthOnlyVertex
   #pragma fragment DepthOnlyFragment
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
   ENDHLSL
  }
  Pass
  {
   Name "DepthNormals" Tags {"LightMode"="DepthNormals"}
   ZWrite On
   HLSLPROGRAM
   #pragma vertex DepthNormalsVertex
   #pragma fragment DepthNormalsFragment
   #pragma multi_compile_instancing
   #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
   #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
   ENDHLSL
  }
 }
}
