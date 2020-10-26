Shader "PixarRenderPipeline/Lit"{
    _Properties{
        _BaseMap("Texture", 2D) = "white"{}
        _BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
    }

    SubShader{
        Pass{
            Tags{
                "LightMode" = "CustomLit"
            }

            HLSLPROGRAM
            #include "MyLitPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }
    }
}