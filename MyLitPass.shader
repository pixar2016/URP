Shader "PixarRenderPipeline/MyLit"{
    Properties{
        _BaseMap("Texture", 2D) = "white"{}
        _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [Toggle(_Clipping)] _Clippingg("Alpha Clipping", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1
    }

    SubShader{
        Tags { "RenderType" = "Opaque" }
        Pass{
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma target 3.5
            //#pragma multi_compile_instancing
            #pragma enable_d3d11_debug_symbols
            #pragma shader_feature _CLIPPING
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "MyLitPass.hlsl"
            ENDHLSL
        }
    }
}