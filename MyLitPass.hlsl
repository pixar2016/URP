#ifndef PIXAR_LIT_PASS_INCLUDED
#define PIXAR_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "MyShadows.hlsl"
#include "MyBRDF.hlsl"
#include "MyGI.hlsl"
#include "MyLighting.hlsl"


TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    //UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings{
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float2 baseUV : VAR_BASE_UV;
    //UNITY_VERTEX_INPUT_INSTANCE_ID
};

#define INPUT_PROP(name) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, name)

float2 TransformBaseUV(float2 baseUV)
{
    float4 baseST = INPUT_PROP(_BaseMap_ST);
    return baseUV * baseST.xy + baseST.zw;
}

Varyings LitPassVertex(Attributes input){
    Varyings output;
    //UNITY_SETUP_INSTANCE_ID(input);
    //UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);

    output.baseUV = TransformBaseUV(input.baseUV);
    return output;
}

float4 LitPassFragment(Varyings input):SV_TARGET{
    //UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 color = baseMap * baseColor;
    #if defined(_CLIPPING)
        clip(base.a - INPUT_PROP(UnityPerMaterial, _Cutoff));
    #endif

    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = INPUT_PROP(UnityPerMaterial, _Metallic);
    surface.smoothness = INPUT_PROP(UnityPerMaterial, _Smoothness);

    #if defined(_PREMULTIPLY_ALPHA)
        BRDF brdf = GetBRDF(surface, true);
    #else
        BRDF brdf = GetBRDF(surface);
    #endif
    GI gi = GetGI(GI_FRAGMENT_DATA(input), surface, brdf);
    float3 color = GetLighting(surface, brdf, gi);
    return float4(color, surface.alpha);
}

#endif
