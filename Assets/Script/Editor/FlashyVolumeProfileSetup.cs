#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Camera Profile — 네온이되 눈 안 아픈 톤.
/// </summary>
public static class FlashyVolumeProfileSetup
{
    const string ProfilePath = "Assets/Scenes/SampleScene/Main Camera Profile.asset";
    const string TonePrefKey = "FSM_VolumeSoftTone_v2";

    [MenuItem("Tools/FSM/Make Flashy Volume Profile")]
    public static void RunMenu()
    {
        Apply(true);
        EditorPrefs.SetInt(TonePrefKey, 2);
    }

    [MenuItem("Tools/FSM/Repair Volume Profile")]
    public static void RepairMenu()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            EditorUtility.DisplayDialog("Volume", $"프로필 없음:\n{ProfilePath}", "OK");
            return;
        }

        RemoveBrokenComponents(profile);
        Apply(true);
        EditorPrefs.SetInt(TonePrefKey, 2);
    }

    [InitializeOnLoadMethod]
    static void AutoOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null) return;

            if (HasBrokenComponents(profile))
            {
                RemoveBrokenComponents(profile);
                Apply(false);
                EditorPrefs.SetInt(TonePrefKey, 2);
                Debug.Log("[Volume] Repaired broken null components in Main Camera Profile.");
                return;
            }

            bool empty = profile.components == null || profile.components.Count == 0;
            if (!empty && EditorPrefs.GetInt(TonePrefKey, 0) >= 2) return;
            Apply(false);
            EditorPrefs.SetInt(TonePrefKey, 2);
        };
    }

    static bool HasBrokenComponents(VolumeProfile profile)
    {
        if (profile.components == null) return false;
        foreach (var c in profile.components)
        {
            if (c == null) return true;
        }
        return false;
    }

    /// <summary>fileID:0 null 참조 제거 — 인스펙터 NRE 원인.</summary>
    static void RemoveBrokenComponents(VolumeProfile profile)
    {
        var so = new SerializedObject(profile);
        var list = so.FindProperty("components");
        if (list == null || !list.isArray) return;

        for (int i = list.arraySize - 1; i >= 0; i--)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                list.DeleteArrayElementAtIndex(i);
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
    }

    static void Apply(bool dialog)
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            if (dialog)
                EditorUtility.DisplayDialog("Volume", $"프로필 없음:\n{ProfilePath}", "OK");
            return;
        }

        RemoveBrokenComponents(profile);
        // --- Bloom: 은은하게 ---
        var bloom = GetOrAdd<Bloom>(profile);
        bloom.active = true;
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(1.35f);
        bloom.scatter.Override(0.55f);
        bloom.clamp.Override(12f);
        bloom.tint.Override(new Color(1f, 0.75f, 0.95f));
        bloom.highQualityFiltering.Override(true);

        // --- Chromatic Aberration ---
        var chroma = GetOrAdd<ChromaticAberration>(profile);
        chroma.active = true;
        chroma.intensity.Override(0.12f);

        // --- Vignette ---
        var vig = GetOrAdd<Vignette>(profile);
        vig.active = true;
        vig.color.Override(new Color(0.02f, 0f, 0.05f));
        vig.intensity.Override(0.28f);
        vig.smoothness.Override(0.45f);
        vig.rounded.Override(true);

        // --- Color Adjustments ---
        var color = GetOrAdd<ColorAdjustments>(profile);
        color.active = true;
        color.postExposure.Override(0.05f);
        color.contrast.Override(12f);
        color.colorFilter.Override(new Color(1f, 0.98f, 1.02f));
        color.hueShift.Override(0f);
        color.saturation.Override(12f);

        // --- White Balance ---
        var wb = GetOrAdd<WhiteBalance>(profile);
        wb.active = true;
        wb.temperature.Override(-2f);
        wb.tint.Override(4f);

        // --- Film Grain ---
        var grain = GetOrAdd<FilmGrain>(profile);
        grain.active = true;
        grain.type.Override(FilmGrainLookup.Medium1);
        grain.intensity.Override(0.12f);
        grain.response.Override(0.8f);

        // --- Lens Distortion ---
        var lens = GetOrAdd<LensDistortion>(profile);
        lens.active = true;
        lens.intensity.Override(-0.06f);
        lens.xMultiplier.Override(1f);
        lens.yMultiplier.Override(1f);
        lens.scale.Override(1.01f);

        // --- Tonemapping ---
        var tone = GetOrAdd<Tonemapping>(profile);
        tone.active = true;
        tone.mode.Override(TonemappingMode.ACES);

        // --- Lift Gamma Gain: 거의 중립 ---
        var lgg = GetOrAdd<LiftGammaGain>(profile);
        lgg.active = true;
        lgg.lift.Override(new Vector4(1f, 1f, 1f, -0.02f));
        lgg.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
        lgg.gain.Override(new Vector4(1.02f, 1f, 1.04f, 0.02f));

        // --- Shadows Midtones Highlights ---
        var smh = GetOrAdd<ShadowsMidtonesHighlights>(profile);
        smh.active = true;
        smh.shadows.Override(new Vector4(1f, 1f, 1.02f, -0.05f));
        smh.midtones.Override(new Vector4(1f, 1f, 1.02f, 0f));
        smh.highlights.Override(new Vector4(1.05f, 1.02f, 1.08f, 0.05f));

        // --- Motion Blur ---
        var motion = GetOrAdd<MotionBlur>(profile);
        motion.active = true;
        motion.quality.Override(MotionBlurQuality.Medium);
        motion.intensity.Override(0.15f);
        motion.clamp.Override(0.05f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        EnsureRendererPostProcess();
        EnsureSceneCameraVolume(profile);

        if (dialog)
        {
            EditorUtility.DisplayDialog(
                "Volume Profile",
                "소프트 네온 톤으로 낮춰 적용했습니다.\nBloom/노출/채도 완화.",
                "OK");
        }
        else
        {
            Debug.Log("[Volume] Soft neon Main Camera Profile applied.");
        }
    }

    static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (!profile.TryGet(out T comp))
            comp = profile.Add<T>(true);
        return comp;
    }

    static void EnsureRendererPostProcess()
    {
        var guids = AssetDatabase.FindAssets("t:UniversalRendererData");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (renderer == null) continue;

            var so = new SerializedObject(renderer);
            var pp = so.FindProperty("postProcessData");
            if (pp != null && pp.objectReferenceValue == null)
            {
                var ppGuids = AssetDatabase.FindAssets("t:PostProcessData");
                if (ppGuids.Length > 0)
                {
                    var ppPath = AssetDatabase.GUIDToAssetPath(ppGuids[0]);
                    pp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<PostProcessData>(ppPath);
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        // Bloom이 HDR에서 잘 먹도록
        var pipeGuids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach (var guid in pipeGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var pipe = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (pipe == null) continue;
            var so = new SerializedObject(pipe);
            var hdr = so.FindProperty("m_SupportsHDR");
            if (hdr != null) hdr.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipe);
        }
    }

    static void EnsureSceneCameraVolume(VolumeProfile profile)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var cams = Object.FindObjectsOfType<Camera>();
            if (cams.Length > 0) cam = cams[0];
        }
        if (cam == null) return;

        cam.orthographic = true;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.allowHDR = true;

        var add = cam.GetComponent<UniversalAdditionalCameraData>();
        if (add == null) add = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        add.renderPostProcessing = true;
        add.volumeLayerMask = ~0;
        add.requiresColorOption = CameraOverrideOption.On;

        var volume = cam.GetComponent<Volume>();
        if (volume == null) volume = cam.gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        volume.weight = 1f;
        volume.sharedProfile = profile;

        EditorUtility.SetDirty(cam.gameObject);
        if (!Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
#endif
