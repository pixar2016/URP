using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


namespace Pixar
{
    public class Lighting
    {
        const string bufferName = "Lighting";
        static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
        static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

        CommandBuffer buffer = new CommandBuffer { name = bufferName };

        public void Setup(
            ScriptableRenderContext context, CullingResults cullingResults
        )
        {

        }
    }
}

