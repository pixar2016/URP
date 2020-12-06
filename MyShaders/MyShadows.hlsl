#ifndef PIXAR_SHADOWS_INCLUDED
#define PIXAR_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
TEXTURE2D_SHADOW(_OtherShadowAtlas);
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_PixarShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowAtlasSize;
    float4 _ShadowDistanceFade;
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

struct DirectionalShadowData
{
    float strength;
    int tileIndex;
    float normalBias;
    int shadowMaskChannel;
};

float SampleDirectionalShadowAtlas(float3 position)
{
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, position
    );

}

float FilterDirectionalShadow(float3 position)
{
    return SampleDirectionalShadowAtlas(position);
}

float GetBakedShadow(ShadowMask mask, int channel)
{
    float shadow = 1.0;
    if (mask.always || mask.distance)
    {
        if (channel >= 0)
        {
            shadow = mask.shadows[channel];
        }
    }
    return shadow;
}

float GetBakedShadow(ShadowMask mask, int channel, float strength)
{
    if (mask.always || mask.distance)
    {
        return lerp(1.0, GetBakedShadow(mask, channel), strength);
    }
    return 1.0;
}

float FadedShadowStrength(float distance, float scale, float fade)
{
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surface){
    ShadowData data;
    data.shadowMask.always = false;
    data.shadowMask.distance = false;
    data.shadowMask.shadows = 1.0;
    data.cascadeBlend = 1.0;
    data.strength = FadedShadowStrength(
        surface.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
    );
    int i;
    for (i = 0; i < _CascadeCount; i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surface.position, sphere.xyz);
        if (distanceSqr < sphere.w)
        {
            float fade = FadedShadowStrength(
                distanceSqr, _CascadeData[i].x, _ShadowDistanceFade.z
            );
            if (i == _CascadeCount - 1)
            {
                data.strength *= fade;
            }
            else
            {
                data.cascadeBlend = fade;
            }
            break;
        }
    }
    if (i == _CascadeCount)
    {
        data.strength = 0.0;
    }
    data.cascadeIndex = i;
    return data;
}

float GetCascadedShadow(
    DirectionalShadowData directional, ShadowData global, Surface surfaceWS
)
{
    float3 normalBias = surfaceWS.interpolatedNormal *
        (directional.normalBias * _CascadeData[global.cascadeIndex].y);
    float3 position = mul(
        _DirectionalShadowMatrices[directional.tileIndex],
        float4(surfaceWS.position + normalBias, 1.0)
    ).xyz;
    float shadow = FilterDirectionalShadow(position);
    return shadow;
}

float GetDirectionalShadowAttenuation(
    DirectionalShadowData directional, ShadowData global, Surface surfaceWS
){
    #if !defined(_RECEIVE_SHADOWS)
        return 1.0;
    #endif
    float shadow = 1.0;
    if (directional.strength * global.strength <= 0.0)
    {
        shadow = GetBakedShadow(
            global.shadowMask, directional.shadowMaskChannel,
            abs(directional.strength)
        );

    }
    else
    {
        shadow = GetCascadedShadow(directional, global, surfaceWS);
    }
    return shadow;
}

struct OtherShadowData
{
    float strength;
    int shadowMaskChannel;
};

float GetOtherShadowAttenuation(
    OtherShadowData other, ShadowData global, Surface surfaceWS
)
{
    #if !defined(_RECEIVE_SHADOWS)
        return 1.0;
    #endif
    float shadow;
    if (other.strength > 0.0)
    {
        shadow = GetBakedShadow(
            global.shadowMask, other.shadowMaskChannel, other.strength
        );
    }
    else
    {
        shadow = 1.0;
    }
    return shadow;
}
#endif