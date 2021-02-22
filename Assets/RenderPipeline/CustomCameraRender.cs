using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Pixar
{
    public partial class CustomCameraRender
    {
        const string bufferName = "Render Camera";

        static ShaderTagId 
            unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
            litShaderTagId = new ShaderTagId("CustomLit");

        static int frameBufferId = Shader.PropertyToID("_CameraFrameBuffer");

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };

        ScriptableRenderContext context;

        Camera camera;

        CullingResults cullingResults;

        Lighting lighting = new Lighting();

        PostFxStack postFxStack = new PostFxStack();

        string SampleName { get; set; }

        public void Render(
            ScriptableRenderContext context, Camera camera,
            bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject,
            ShadowSettings shadowSettings, PostFxSettings postFxSettings
        )
        {
            this.context = context;
            this.camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull(shadowSettings.maxDistance))
            {
                return;
            }

            buffer.BeginSample(SampleName);
            ExecuteBuffer();
            lighting.Setup(
                context, cullingResults, shadowSettings, useLightsPerObject);
            postFxStack.Setup(context, camera, postFxSettings);
            buffer.EndSample(SampleName);
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstancing, useLightsPerObject);
            DrawUnsupportedShaders();
            DrawGizmos();
            if (postFxStack.IsActive)
            {
                postFxStack.Render(frameBufferId);
            }
            Cleanup();
            Submit();
        }

        bool Cull(float maxShadowDistance)
        {
            if(camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }

        void Setup()
        {
            context.SetupCameraProperties(camera);
            CameraClearFlags flags = camera.clearFlags;

            if (postFxStack.IsActive)
            {
                if(flags > CameraClearFlags.Color)
                {
                    flags = CameraClearFlags.Color;
                }

                buffer.GetTemporaryRT(frameBufferId, camera.pixelWidth, camera.pixelHeight,
                    32, FilterMode.Bilinear, RenderTextureFormat.Default);

                buffer.SetRenderTarget(frameBufferId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            }
            buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ?
                    camera.backgroundColor.linear : Color.clear
            );
            buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        void Cleanup()
        {
            lighting.Cleanup();
            if (postFxStack.IsActive)
            {
                buffer.ReleaseTemporaryRT(frameBufferId);
            }
        }

        void Submit()
        {
            buffer.EndSample(SampleName);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void DrawVisibleGeometry(
            bool useDynamicBatching, bool useGPUInstancing, bool useLightsPerObject
        )
        {
            PerObjectData lightsPerObjectFlags = useLightsPerObject ?
                PerObjectData.LightData | PerObjectData.LightIndices :
                PerObjectData.None;
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawSettings = new DrawingSettings(
                unlitShaderTagId, sortingSettings
            )
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing,
                perObjectData = PerObjectData.ReflectionProbes |
                    PerObjectData.Lightmaps | PerObjectData.ShadowMask |
                    PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
                    PerObjectData.LightProbeProxyVolume |
                    PerObjectData.OcclusionProbeProxyVolume
                //perObjectData =
                //    PerObjectData.ReflectionProbes |
                //    PerObjectData.Lightmaps | PerObjectData.ShadowMask |
                //    PerObjectData.LightProbe | PerObjectData.OcclusionProbe |
                //    PerObjectData.LightProbeProxyVolume |
                //    PerObjectData.OcclusionProbeProxyVolume |
                //    lightsPerObjectFlags
            };
            drawSettings.SetShaderPassName(1, litShaderTagId);

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(
                cullingResults, ref drawSettings, ref filterSettings
            );

            context.DrawSkybox(camera);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;

            context.DrawRenderers(
                cullingResults, ref drawSettings, ref filterSettings);
        }
    }
}

