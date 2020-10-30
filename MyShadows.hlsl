#ifndef PIXAR_SHADOWS_INCLUDED
#define PIXAR_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
TEXTURE2D_SHADOW(_OtherShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_PixarShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
CBUFFER_END

struct ShadowMask{
    bool always;
    bool distance;
    float4 shadows;
};

struct ShadowData{
    int cascadeIndex;
    float cascadeBlend;
    float strength;
    ShadowMask shadowMask;
};

struct DirectionalShadowData{
    float strength;
    int tileIndex;
    float normalBias;
    int shadowMaskChannel;
}

ShadowData GetShadowData(Surface surface){
    ShaodwData data;
    data.shadowMask.always = false;
    data.shadowMask.distance = false;
    data.shadowMask.shadows = 1.0;
    data.cascadeBlend = 1.0;
    int i;
    for(i = 0; i < _CascadeCount; i++){
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surface.position, sphere.xyz);
    }
}

#endif