﻿using UnityEngine;
using UnityEngine.Rendering;

namespace Pixar
{
    public partial class CustomRenderPipeline : RenderPipeline
    {
        CustomCameraRender renderer = new CustomCameraRender();

        bool useDynamicBatching, useGPUInstancing, useLightsPerObject;

        ShadowSettings shadowSettings;

        PostFxSettings postFxSettings;

        public CustomRenderPipeline(
            bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,
            bool useLightsPerObject, ShadowSettings shadowSettings, PostFxSettings postFxSettings)
        {
            this.postFxSettings = postFxSettings;
            this.shadowSettings = shadowSettings;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.useLightsPerObject = useLightsPerObject;
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
            InitializeForEditor();
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(Camera camera in cameras)
            {
                renderer.Render(
                    context, camera,
                    useDynamicBatching, useGPUInstancing, useLightsPerObject,
                    shadowSettings, postFxSettings);
            }
        }
    }
}

