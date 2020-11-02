#ifndef PIXAR_LIGHTING_INCLUDED
#define PIXAR_LIGHTING_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_PixarLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light{
    float3 color;
    float3 direction;
};

DirectionalShadowData GetDirectionalShadowData(int lightIndex, ShadowData shadowData){
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowdata[lightIndex].y + shadow.cascadeIndex;
    data.normalBias = _DirectionalLightShadowData[lightIndex].z;
	data.shadowMaskChannel = _DirectionalLightShadowData[lightIndex].w;
	return data;
}

int GetDirectionalLight(int index, Surface surface, ShadowData shadowData){
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
    return light;
}

int GetDirectionalLightCount(){
    return _DirectionalLightCount;
}

float3 IncomingLight(Surface surface, Light light){
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light){
    return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf, GI gi){
    ShadowData shadowData = GetShadowData(surface);
    shadowData.shadowMask = gi.shadowMask;

    float3 color = IndirectBRDF(surface, brdf, gi.diffuse, gi.specular);
    for(int i = 0; i < GetDirectionalLightCount(); i++){
        Light light = GetDirectionalLight(i, surface, shadowData);
        color += GetLighting(surface, brdf, light);
    }
    return color;
}

#endif