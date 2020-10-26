Shader "PixarRenderPipeline/MyLit"{
    Properties{
        _BaseMap("Texture", 2D) = "white"{}
        _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
    }

    SubShader{
        Tags { "RenderType" = "Opaque" }
        Pass{
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.5
            //#pragma multi_compile_instancing
            #pragma enable_d3d11_debug_symbols
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "MyLitPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "LitShaderGUI"
}