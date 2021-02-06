
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class LitShaderGUI : ShaderGUI
{
    MaterialEditor editor;
    Object[] materials;
    MaterialProperty[] properties;

    bool showPresets;

    enum ClipMode
    {
        Off, On, Shadows
    }

    ClipMode Clipping
    {
        set
        {
            FindProperty("_Clipping", properties).floatValue = (float)value;
            SetKeywordEnabled("_CLIPPING_OFF", value == ClipMode.Off);
            SetKeywordEnabled("_CLIPPING_ON", value == ClipMode.On);
            SetKeywordEnabled("_CLIPPING_SHADOWS", value == ClipMode.Shadows);
        }
    }

    bool ReceiveShadows
    {
        set
        {
            FindProperty("_ReceiveShadows", properties).floatValue = value ? 1 : 0;
            SetKeywordEnabled("_RECEIVE_SHADOWS", value);
        }
    }

    RenderQueue RenderQueue
    {
        set
        {
            foreach(Material m in materials)
            {
                m.renderQueue = (int)value;
            }
        }
    }

    //CullMode Cull
    //{
    //    set
    //    {
    //        FindProperty("_Cull", properties).floatValue = (float)value;
    //    }
    //}

    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

    bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    BlendMode SrcBlend
    {
        set
        {
            FindProperty("_SrcBlend", properties).floatValue = (float)value;
        }
    }

    BlendMode DstBlend
    {
        set
        {
            FindProperty("_DstBlend", properties).floatValue = (float)value;
        }
    }

    bool ZWrite
    {
        set
        {
            FindProperty("_ZWrite", properties).floatValue = value ? 1 : 0;
        }
    }

    enum ShadowMode
    {
        On, Clip, Dither, Off
    }

    ShadowMode Shadows
    {
        set
        {
            if(SetProperty("_Shadows", (float)value))
            {
                SetKeywordEnabled("_SHADOWS_CLIP", value == ShadowMode.Clip);
                SetKeywordEnabled("_SHADOWS_DITHER", value == ShadowMode.Dither);
            }
        }
    }

    bool HasProperty(string name) =>
        FindProperty(name, properties, false) != null;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUI.BeginChangeCheck();
        base.OnGUI(materialEditor, properties);

        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;

        BakedEmission();

        EditorGUILayout.Space();
        showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
        if (showPresets)
        {
            OpaquePreset();
            ClipPreset();
            ClipDoubleSidedPreset();
            FadePreset();
            FadeWithShadowsPreset();
            TransparentPreset();
        }

        if (EditorGUI.EndChangeCheck())
        {
            SetShadowCasterPass();
            CopyLightMappingProperties();
        }
    }

    void CopyLightMappingProperties()
    {
        MaterialProperty mainTex = FindProperty("_MainTex", properties, false);
        MaterialProperty baseMap = FindProperty("_BaseMap", properties, false);
        if(mainTex != null && baseMap != null)
        {
            mainTex.textureValue = baseMap.textureValue;
            mainTex.textureScaleAndOffset = baseMap.textureScaleAndOffset;
        }
        MaterialProperty color = FindProperty("_Color", properties, false);
        MaterialProperty baseColor = FindProperty("_BaseColor", properties, false);
        if(color != null && baseColor != null)
        {
            color.colorValue = baseColor.colorValue;
        }
    }

    void BakedEmission()
    {
        EditorGUI.BeginChangeCheck();
        editor.LightmapEmissionProperty();
        if (EditorGUI.EndChangeCheck())
        {
            foreach(Material m in editor.targets)
            {
                m.globalIlluminationFlags &=
                    ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    }

    void CastShadowsToggle()
    {
        bool? enabled = IsPassEnabled("ShadowCaster");
        if (!enabled.HasValue)
        {
            EditorGUI.showMixedValue = true;
            enabled = false;
        }
        EditorGUI.BeginChangeCheck();
        enabled = EditorGUILayout.Toggle("Cast Shadows", enabled.Value);
        if (EditorGUI.EndChangeCheck())
        {
            editor.RegisterPropertyChangeUndo("Cast Shadows");
            SetPassEnabled("ShadowCaster", enabled.Value);
        }
        EditorGUI.showMixedValue = false;
    }

    void OpaquePreset()
    {
        if (!GUILayout.Button("Opaque"))
        {
            return;
        }
        editor.RegisterPropertyChangeUndo("Opaque Preset");
        Clipping = ClipMode.Off;
        //Cull = CullMode.Off;
        SrcBlend = BlendMode.One;
        DstBlend = BlendMode.Zero;
        ZWrite = true;
        ReceiveShadows = true;
        SetPassEnabled("ShadowCaster", true);
        RenderQueue = RenderQueue.Geometry;
    }

    void ClipPreset()
    {
        if (!GUILayout.Button("Clip"))
        {
            return;
        }
        editor.RegisterPropertyChangeUndo("Clip Preset");
        Clipping = ClipMode.On;
        //Cull = CullMode.Back;
        SrcBlend = BlendMode.One;
        DstBlend = BlendMode.Zero;
        ZWrite = true;
        ReceiveShadows = true;
        SetPassEnabled("ShadowCaster", true);
        RenderQueue = RenderQueue.AlphaTest;
    }

    void ClipDoubleSidedPreset()
    {
        if (!GUILayout.Button("Clip Double-Sided"))
        {
            return;
        }
        editor.RegisterPropertyChangeUndo("Clip Double-Sided Preset");
        Clipping = ClipMode.On;
        //Cull = CullMode.Off;
        SrcBlend = BlendMode.One;
        DstBlend = BlendMode.Zero;
        ZWrite = true;
        ReceiveShadows = true;
        SetPassEnabled("ShadowCaster", true);
        RenderQueue = RenderQueue.AlphaTest;
    }

    void FadePreset()
    {
        if (!GUILayout.Button("Fade"))
        {
            return;
        }
        editor.RegisterPropertyChangeUndo("Fade Preset");
        Clipping = ClipMode.Off;
        //Cull = CullMode.Back;
        SrcBlend = BlendMode.SrcAlpha;
        DstBlend = BlendMode.OneMinusSrcAlpha;
        ZWrite = false;
        ReceiveShadows = false;
        SetPassEnabled("ShadowCaster", false);
        RenderQueue = RenderQueue.Transparent;
    }

    void FadeWithShadowsPreset()
    {
        if (!GUILayout.Button("Fade with Shadows"))
        {
            return;
        }
        editor.RegisterPropertyChangeUndo("Fade with Shadows Preset");
        Clipping = ClipMode.Shadows;
        //Cull = CullMode.Back;
        SrcBlend = BlendMode.SrcAlpha;
        DstBlend = BlendMode.OneMinusSrcAlpha;
        ZWrite = false;
        ReceiveShadows = true;
        SetPassEnabled("ShadowCaster", true);
        RenderQueue = RenderQueue.Transparent;
    }

    void TransparentPreset()
    {
        if(HasPremultiplyAlpha && GUILayout.Button("Transparent"))
        {
            Clipping = ClipMode.Shadows;
            //Cull = CullMode.Back;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    void SetPassEnabled(string pass, bool enabled)
    {
        foreach (Material m in materials)
        {
            m.SetShaderPassEnabled(pass, enabled);
        }
    }

    bool? IsPassEnabled(string pass)
    {
        bool enabled = ((Material)materials[0]).GetShaderPassEnabled(pass);
        for (int i = 1; i < materials.Length; i++)
        {
            if (enabled != ((Material)materials[i]).GetShaderPassEnabled(pass))
            {
                return null;
            }
        }
        return enabled;
    }

    void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeywordEnabled(keyword, value);
        }
    }

    bool SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties, false);
        if(property != null)
        {
            property.floatValue = value;
            return true;
        }
        return false;
    }

    void SetKeywordEnabled(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach(Material m in materials)
            {
                m.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach(Material m in materials)
            {
                m.DisableKeyword(keyword);
            }
        }
    }

    void SetShadowCasterPass()
    {
        MaterialProperty shadows = FindProperty("_Shadows", properties, false);
        if(shadows == null || shadows.hasMixedValue)
        {
            return;
        }
        bool enabled = shadows.floatValue < (float)ShadowMode.Off;
        foreach (Material m in materials)
        {
            m.SetShaderPassEnabled("ShadowCaster", enabled);
        }
    }
}
