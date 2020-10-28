#ifndef PIXAR_LIGHTING_INCLUDED
#define PIXAR_LIGHTING_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_PixarLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light{
    float3 color;
    float3 direction;
};

int GetDirectionalLight(int index){
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    return light;
}

int GetDirectionalLightCount(){
    return _DirectionalLightCount;
}

float3 IncomingLight(Surface surface, Light light){
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light){
    return IncomingLight(surface, light) *
}

float3 GetLighting(Surface surface, BRDF brdf){
    float3 color = 0.0;
    for(int i = 0; i < GetDirectionalLightCount(); i++){
        color += GetLighting(surface, brdf, GetDirectionalLight(i))l
    }
    return color;
}

#endif