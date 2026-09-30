Shader "PowderFlow/Snow Particle"
{
 Properties{[MainTexture]_BaseMap("Shape",2D)="white"{} [MainColor]_BaseColor("Tint",Color)=(1,1,1,1) _SoftDistance("Contact fade",Float)=.3 _NearFade("Camera fade",Float)=.7}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
  Pass
  {
   Name "SnowParticles" Tags{"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float4 _BaseMap_ST;float _SoftDistance,_NearFade;
   CBUFFER_END
   struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
   struct V{float4 vertex:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;half4 color:COLOR;float depth:TEXCOORD2;float fog:TEXCOORD3;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.vertex.xyz);o.vertex=TransformWorldToHClip(o.world);o.depth=-TransformWorldToView(o.world).z;o.uv=TRANSFORM_TEX(a.uv,_BaseMap);o.color=a.color*_BaseColor;o.fog=ComputeFogFactor(o.vertex.z);return o;}
   half4 frag(V i):SV_Target
   {
    half4 shape=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
    float scene=LinearEyeDepth(SampleSceneDepth(GetNormalizedScreenSpaceUV(i.vertex)),_ZBufferParams);
    float soft=saturate((scene-i.depth)/max(.01,_SoftDistance));float nearFade=smoothstep(_NearFade,_NearFade+1,i.depth);
    Light light=GetMainLight(TransformWorldToShadowCoord(i.world));half3 illumination=max(half3(.42,.46,.55),SampleSH(half3(0,1,0))*.8+light.color*.65*light.shadowAttenuation);
    return half4(MixFog(shape.rgb*i.color.rgb*illumination,i.fog),shape.a*i.color.a*soft*nearFade);
   }
   ENDHLSL
  }
 }
}
