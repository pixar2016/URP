using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="Rendering/Custom Post Fx Settings")]
public class PostFxSettings : ScriptableObject
{
    [SerializeField]
    Shader shader = null;

    [System.Serializable]
    public struct BloomSettings
    {
        public int maxInterations;
        public int downscaleLimit;
        public bool bicubicUpsampling;
        public float threshold;
        public float thresholdKnee;
        public float intensity;
    }

    [SerializeField]
    BloomSettings bloom = default;

    public BloomSettings Bloom => bloom;

    [System.NonSerialized]
    Material material;

    public Material Material
    {
        get
        {
            if(material == null && shader != null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }
}
