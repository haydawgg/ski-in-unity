Shader "PowderFlow/Alpine Sky"
{
 Properties
 {
  _Top("Zenith",Color)=(.16,.20,.36,1) _Horizon("Horizon",Color)=(.76,.49,.56,1)
  _SunDirection("Sun direction",Vector)=(0,.2,1,0) _SunColor("Sun",Color)=(1,.8,.65,1)
  _CloudColor("Cloud tint",Color)=(.94,.96,.99,1) _Clouds("Cloud opacity",Range(0,1))=.45
  _CloudScale("Cloud scale",Float)=2.8 _CloudCoverage("Cloud coverage",Range(0,1))=.48
  _CloudSoftness("Cloud softness",Range(.01,.5))=.22 _SunRadius("Sun radius degrees",Float)=.55
 }
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
   half4 _Top,_Horizon,_SunColor,_CloudColor;float4 _SunDirection;float _Clouds,_CloudScale,_CloudCoverage,_CloudSoftness,_SunRadius;
   CBUFFER_END
   struct V{float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
   V vert(float4 vertex:POSITION){V o;o.vertex=TransformObjectToHClip(vertex.xyz);o.direction=vertex.xyz;return o;}
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p)
   {
    float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(hash21(q),hash21(q+float2(1,0)),f.x),lerp(hash21(q+float2(0,1)),hash21(q+1),f.x),f.y);
   }
   float cloudNoise(float2 p)
   {
    return noise21(p)*.55+noise21(p*2.03+17)*.28+noise21(p*4.1-9)*.12+noise21(p*8.2+3)*.05;
   }
   half4 frag(V i):SV_Target
   {
    float3 dir=normalize(i.direction);float t=pow(saturate(dir.y),.5);half3 color=lerp(_Horizon.rgb,_Top.rgb,t);
    float2 uv=dir.xz/max(.12,dir.y+.18)*_CloudScale;
    float cloud=smoothstep(1-_CloudCoverage-_CloudSoftness,1-_CloudCoverage+_CloudSoftness,cloudNoise(uv))*_Clouds*smoothstep(.015,.16,dir.y);
    half3 cloudTint=_CloudColor.rgb*lerp(.82,1,cloudNoise(uv+float2(.18,.1)));
    color=lerp(color,cloudTint,cloud);
    float sun=dot(dir,normalize(_SunDirection.xyz));float disk=smoothstep(cos(radians(_SunRadius+.12)),cos(radians(_SunRadius)),sun);
    color+=_SunColor.rgb*(disk*6+pow(saturate(sun),60)*.18)*(1-cloud*.55);
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
