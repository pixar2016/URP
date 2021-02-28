
using UnityEngine;
using UnityEngine.Rendering;
namespace Pixar
{
    public class PostFxStack
    {
        enum Pass
        {
            BloomCombine,
            BloomHorizontal,
            BloomPrefilter,
            BloomVertical,
            Copy
        };
        const string bufferName = "Post Fx";

        const int maxBloomPyramidLevels = 16;

        int
            bloomPrefilterId = Shader.PropertyToID("_BloomPrefilter"),
            bloomIntensityId = Shader.PropertyToID("_BloomIntensity"),
            bloomThresholdId = Shader.PropertyToID("_BloomThreshold"),
            fxSourceId = Shader.PropertyToID("_PostFXSource"),
            fxSource2Id = Shader.PropertyToID("_PostFXSource2");

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };
        ScriptableRenderContext context;
        Camera camera;
        PostFxSettings settings;

        public bool IsActive => settings != null;

        int bloomPyramidId;

        public PostFxStack()
        {
            bloomPyramidId = Shader.PropertyToID("_BloomPyramid0");
            for(int i = 1; i < maxBloomPyramidLevels * 2; i++)
            {
                Shader.PropertyToID("_BloomPyramid" + i);
            }
        }

        public void Setup(ScriptableRenderContext context, Camera camera, PostFxSettings settings)
        {
            this.context = context;
            this.camera = camera;
            this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
        }

        public void Render(int sourceId)
        {
            DoBloom(sourceId);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void DoBloom(int sourceId)
        {
            buffer.BeginSample("Bloom");
            PostFxSettings.BloomSettings bloom = settings.Bloom;
            int width = camera.pixelWidth / 2;
            int height = camera.pixelHeight / 2;

            if(bloom.maxInterations == 0 || bloom.intensity <= 0f ||
                height < bloom.downscaleLimit * 2 || width < bloom.downscaleLimit * 2)
            {
                Draw(sourceId, BuiltinRenderTextureType.CameraTarget, Pass.Copy);
                buffer.EndSample("Bloom");
                return;
            }

            Vector4 threshold;
            threshold.x = Mathf.GammaToLinearSpace(bloom.threshold);
            threshold.y = threshold.x * bloom.thresholdKnee;
            threshold.z = 2f * threshold.y;
            threshold.w = 0.25f / (threshold.y + 0.00001f);
            buffer.SetGlobalVector(bloomThresholdId, threshold);

            RenderTextureFormat format = RenderTextureFormat.Default;
            buffer.GetTemporaryRT(
                bloomPrefilterId, width, height, 0, FilterMode.Bilinear, format
            );
            Draw(sourceId, bloomPrefilterId, Pass.BloomPrefilter);

            int fromId = bloomPrefilterId;
            int midId = bloomPyramidId;
            int toId = midId + 1;
            buffer.GetTemporaryRT(midId, width, height, 0, FilterMode.Bilinear, format);
            buffer.GetTemporaryRT(toId, width, height, 0, FilterMode.Bilinear, format);
            Draw(fromId, midId, Pass.BloomHorizontal);
            Draw(midId, toId, Pass.BloomVertical);

            buffer.ReleaseTemporaryRT(bloomPrefilterId);
            buffer.ReleaseTemporaryRT(bloomPyramidId);

            buffer.SetGlobalFloat(bloomIntensityId, bloom.intensity);
            buffer.SetGlobalTexture(fxSource2Id, sourceId);
            Draw(toId, BuiltinRenderTextureType.CameraTarget, Pass.BloomCombine);
            buffer.ReleaseTemporaryRT(toId);
            buffer.EndSample("Bloom");

            //int fromId = bloomPrefilterId;
            //int toId = bloomPyramidId + 1;
            //int i;
            //for (i = 0; i < bloom.maxInterations; i++)
            //{
            //    if(height < bloom.downscaleLimit || width < bloom.downscaleLimit)
            //    {
            //        break;
            //    }
            //    int midId = toId - 1;
            //    buffer.GetTemporaryRT(midId, width, height, 0, FilterMode.Bilinear, format);
            //    buffer.GetTemporaryRT(toId, width, height, 0, FilterMode.Bilinear, format);
            //    Draw(fromId, midId, Pass.BloomHorizontal);
            //    Draw(midId, toId, Pass.BloomVertical);
            //    fromId = toId;
            //    toId += 2;
            //    width /= 2;
            //    height /= 2;
            //}

            //buffer.ReleaseTemporaryRT(bloomPrefilterId);

            //if(i > 1)
            //{
            //    buffer.ReleaseTemporaryRT(fromId - 1);
            //    toId -= 5;

            //}
        }

        void Draw(RenderTargetIdentifier from, RenderTargetIdentifier to, Pass pass)
        {
            buffer.SetGlobalTexture(fxSourceId, from);
            buffer.SetRenderTarget(to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.DrawProcedural(
                Matrix4x4.identity, settings.Material, (int)pass,
                MeshTopology.Triangles, 3
            );
        }
    }
}

