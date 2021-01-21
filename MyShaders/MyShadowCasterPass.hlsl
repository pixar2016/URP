#ifndef PIXAR_SHADOW_PASS_INCLUDED
#define PIXAR_SHADOW_PASS_INCLUDED

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

float3 TransformObjectToWorld1(float3 positionOS)
{
    return mul(UNITY_MATRIX_M, float4(positionOS, 1.0)).xyz;
}

float4 TransformWorldToHClip1(float3 positionWS)
{
    return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

Varyings ShadowCasterPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld1(input.positionOS);
    output.positionCS = TransformWorldToHClip1(positionWS);

#if UNITY_REVERSED_Z
		output.positionCS.z =
			min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    output.positionCS.z =
			max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    output.baseUV = TransformBaseUV(input.baseUV);
    return output;
}

float InterleavedGradientNoise1(float2 uv, uint frameCount)
{
    const float3 magic = float3(0.06711056f, 0.00583715f, 52.9829189f);
    float2 frameMagicScale = float2(2.083f, 4.867f);
    uv += frameCount * frameMagicScale;
    return frac(magic.z * frac(dot(uv, magic.xy)));
}

void ShadowCasterPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 base = GetBase(input.baseUV);
#if defined(_SHADOWS_CLIP)
			clip(base.a - GetCutoff(input.baseUV));
#elif defined(_SHADOWS_DITHER)
			float dither = InterleavedGradientNoise1(input.positionCS.xy, 0);
			clip(base.a - dither);
#endif
}

#endif