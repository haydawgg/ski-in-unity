Shader "PowderFlow/Ski Grooves"
{
 Properties{_BaseColor("Groove",Color)=(.24,.28,.4,.35)}
 SubShader
 {
  Tags{"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   struct A{float4 vertex:POSITION;half4 color:COLOR;};struct V{float4 vertex:SV_POSITION;half4 color:COLOR;};
   V vert(A i){V o;o.vertex=TransformObjectToHClip(i.vertex.xyz);o.color=i.color*_BaseColor;return o;}half4 frag(V i):SV_Target{return i.color;}
   ENDHLSL
  }
 }
}
