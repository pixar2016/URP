#ifndef PIXAR_SURFACE_INCLUDED
#define PIXAR_SURFACE_INCLUDED

struct Surface{
    float3 normal;
    float3 viewDirection;
    float3 color;
    float alpha;
    float metallic;
    float smoothness;
    float fresnelStrength;
    float occlusion;
    float depth;
}

#endif