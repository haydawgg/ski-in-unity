Shader "PowderFlow/Ski Grooves"
{
 Properties
 {
  _BaseColor("Cavity",Color)=(.28,.36,.48,.40) _RidgeColor("Snow lip",Color)=(.85,.92,1,.18)
  _Lifetime("Lifetime",Float)=100 _Softness("Edge softness",Range(.05,.4))=.22
 }
 SubShader
 {
  Tags{"Queue"="Transparent-10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Offset -1,-1
  Pass
  {
   Name "Grooves" Tags{"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_RidgeColor;float _Lifetime,_Softness;
   CBUFFER_END
   struct A{float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;float2 ageDepth:TEXCOORD1;};
   struct V{float4 vertex:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;float2 uv:TEXCOORD2;float2 ageDepth:TEXCOORD3;float fog:TEXCOORD4;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.vertex.xyz);o.vertex=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normal);o.uv=a.uv;o.ageDepth=a.ageDepth;o.fog=ComputeFogFactor(o.vertex.z);return o;}
   half4 frag(V i):SV_Target
   {
    float x=abs(i.uv.x*2-1),aa=max(fwidth(x),.015);
    float cavity=1-smoothstep(.22,.65,x),lip=exp(-pow((x-.74)/.12,2));
    float edge=1-smoothstep(1-_Softness-aa,1,x);
    float fade=1-smoothstep(_Lifetime*.55,_Lifetime,_Time.y-i.ageDepth.x);
    float tip=smoothstep(0,.22,i.uv.y);
    Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
    half3 illumination=max(half3(.42,.46,.55),SampleSH(normalize(i.normal))*.8+light.color*.55*light.shadowAttenuation);
    half3 color=lerp(_BaseColor.rgb,_RidgeColor.rgb,saturate(lip))*illumination;
    float alpha=(_BaseColor.a*cavity*i.ageDepth.y+_RidgeColor.a*lip)*edge*fade*tip;
    return half4(MixFog(color,i.fog),saturate(alpha));
   }
   ENDHLSL
  }
 }
}
