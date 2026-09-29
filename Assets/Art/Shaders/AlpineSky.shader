Shader "PowderFlow/Alpine Sky"
{
 Properties { _Top("Zenith",Color)=(.19,.19,.39,1) _Horizon("Horizon",Color)=(.88,.56,.65,1) _SunDirection("Sun direction",Vector)=(0,.2,1,0) _SunColor("Sun",Color)=(1,.8,.65,1) _Clouds("Cloud amount",Float)=.45 }
 SubShader
 {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"} Cull Off ZWrite Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Top,_Horizon,_SunColor;float4 _SunDirection;float _Clouds;
   CBUFFER_END
   struct V{float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
   V vert(float4 vertex:POSITION){V o;o.vertex=TransformObjectToHClip(vertex.xyz);o.direction=vertex.xyz;return o;}
   float noise(float2 p){return sin(p.x)*sin(p.y*.73)+.5*sin(p.x*2.1+p.y)*sin(p.y*1.5);}
   half4 frag(V i):SV_Target
   {
    float3 dir=normalize(i.direction);float t=pow(saturate(dir.y),.55);half3 color=lerp(_Horizon.rgb,_Top.rgb,t);
    float2 uv=dir.xz/max(.08,dir.y)*1.4;float cloud=smoothstep(.35,.9,noise(uv))*.35*_Clouds*smoothstep(0,.15,dir.y);
    color=lerp(color,half3(.86,.88,.94),cloud);
    float sun=dot(dir,normalize(_SunDirection.xyz));color+=_SunColor.rgb*(pow(saturate(sun),6500)*8+pow(saturate(sun),32)*.13);
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
