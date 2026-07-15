#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Camera Profile 세팅.
/// VolumeComponent는 반드시 AddObjectToAsset으로 심어야 fileID:0 NRE가 안 난다.
/// </summary>
public static class FlashyVolumeProfileSetup
{
    const string ProfilePath = "Assets/Scenes/SampleScene/Main Camera Profile.asset";
    const string TonePrefKey = "FSM_VolumeSoftTone_v5";

    [MenuItem("Tools/FSM/Make Flashy Volume Profile")]
    public static void RunMenu()
    {
        Selection.activeObject = null;
        Rebuild(true);
    }

    [MenuItem("Tools/FSM/Repair Volume Profile")]
    public static void RepairMenu()
    {
        Selection.activeObject = null;
        Rebuild(true);
    }

    [InitializeOnLoadMethod]
    static void AutoOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null) return;

            bool broken = HasBrokenComponents(profile);
            bool empty = profile.components == null || profile.components.Count == 0;
            bool needsTone = EditorPrefs.GetInt(TonePrefKey, 0) < 5;
            if (!broken && !empty && !needsTone) return;

            // 인스펙터가 깨진 프로필을 열어두면 CreateEditor NRE 반복 → 선택 해제
            if (Selection.activeObject == profile)
                Selection.activeObject = null;

            Rebuild(false);
        };
    }

    static void Rebuild(bool dialog)
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            if (dialog)
                EditorUtility.DisplayDialog("Volume", $"프로필 없음:\n{ProfilePath}", "OK");
            return;
        }

        ClearAllComponents(profile);

        // Bloom ~6.5, 살짝 뿌연 정도
        var bloom = AddEmbedded<Bloom>(profile);
        bloom.active = true;
        bloom.threshold.Override(0.6f);
        bloom.intensity.Override(6.5f);
        bloom.scatter.Override(0.72f);
        bloom.clamp.Override(18f);
        bloom.tint.Override(new Color(1f, 0.85f, 1f));
        bloom.highQualityFiltering.Override(true);

        var chroma = AddEmbedded<ChromaticAberration>(profile);
        chroma.active = true;
        chroma.intensity.Override(0.1f);

        var vig = AddEmbedded<Vignette>(profile);
        vig.active = true;
        vig.color.Override(new Color(0.02f, 0f, 0.05f));
        vig.intensity.Override(0.28f);
        vig.smoothness.Override(0.45f);
        vig.rounded.Override(true);

        var color = AddEmbedded<ColorAdjustments>(profile);
        color.active = true;
        color.postExposure.Override(0.02f);
        color.contrast.Override(10f);
        color.colorFilter.Override(new Color(1f, 0.98f, 1.02f));
        color.hueShift.Override(0f);
        color.saturation.Override(10f);

        var wb = AddEmbedded<WhiteBalance>(profile);
        wb.active = true;
        wb.temperature.Override(-2f);
        wb.tint.Override(4f);

        var grain = AddEmbedded<FilmGrain>(profile);
        grain.active = true;
        grain.type.Override(FilmGrainLookup.Medium1);
        grain.intensity.Override(0.1f);
        grain.response.Override(0.8f);

        var lens = AddEmbedded<LensDistortion>(profile);
        lens.active = true;
        lens.intensity.Override(-0.05f);
        lens.xMultiplier.Override(1f);
        lens.yMultiplier.Override(1f);
        lens.scale.Override(1.01f);

        var tone = AddEmbedded<Tonemapping>(profile);
        tone.active = true;
        tone.mode.Override(TonemappingMode.ACES);

        var lgg = AddEmbedded<LiftGammaGain>(profile);
        lgg.active = true;
        lgg.lift.Override(new Vector4(1f, 1f, 1f, -0.02f));
        lgg.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
        lgg.gain.Override(new Vector4(1.02f, 1f, 1.04f, 0.02f));

        var smh = AddEmbedded<ShadowsMidtonesHighlights>(profile);
        smh.active = true;
        smh.shadows.Override(new Vector4(1f, 1f, 1.02f, -0.05f));
        smh.midtones.Override(new Vector4(1f, 1f, 1.02f, 0f));
        smh.highlights.Override(new Vector4(1.05f, 1.02f, 1.08f, 0.05f));

        var motion = AddEmbedded<MotionBlur>(profile);
        motion.active = true;
        motion.quality.Override(MotionBlurQuality.Medium);
        motion.intensity.Override(0.12f);
        motion.clamp.Override(0.05f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ProfilePath, ImportAssetOptions.ForceUpdate);

        // 저장 후에도 null이면 실패
        profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (HasBrokenComponents(profile))
        {
            Debug.LogError("[Volume] Rebuild failed — components still null. Do not select the profile until fixed.");
            if (dialog)
                EditorUtility.DisplayDialog("Volume", "프로필 복구 실패. Console 확인.", "OK");
            return;
        }

        EnsureRendererPostProcess();
        EnsureSceneCameraVolume(profile);
        EditorPrefs.SetInt(TonePrefKey, 5);

        if (dialog)
        {
            EditorUtility.DisplayDialog(
                "Volume Profile",
                "복구 완료.\nBloom Intensity 6.5 (뿌연 느낌 살짝 완화).",
                "OK");
        }
        else
        {
            Debug.Log("[Volume] Profile rebuilt with embedded components. Bloom 6.5");
        }
    }

    static T AddEmbedded<T>(VolumeProfile profile) where T : VolumeComponent
    {
        // 이미 정상 객체가 있으면 재사용
        if (profile.TryGet(out T existing) && existing != null)
            return existing;

        var comp = ScriptableObject.CreateInstance<T>();
        comp.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        comp.name = typeof(T).Name;
        comp.SetAllOverridesTo(true);

        AssetDatabase.AddObjectToAsset(comp, profile);
        profile.components.Add(comp);
        return comp;
    }

    static bool HasBrokenComponents(VolumeProfile profile)
    {
        if (profile == null) return true;
        if (profile.components == null) return false;

        var so = new SerializedObject(profile);
        var list = so.FindProperty("components");
        if (list != null && list.isArray)
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    return true;
            }
        }

        foreach (var c in profile.components)
        {
            if (c == null) return true;
        }
        return false;
    }

    static void ClearAllComponents(VolumeProfile profile)
    {
        // 런타임 리스트
        if (profile.components != null)
            profile.components.Clear();

        // 시리얼라이즈 배열
        var so = new SerializedObject(profile);
        var list = so.FindProperty("components");
        if (list != null && list.isArray)
        {
            list.ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // 서브에셋 전부 제거
        var path = AssetDatabase.GetAssetPath(profile);
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset == null || asset == profile) continue;
            if (asset is VolumeComponent)
                Object.DestroyImmediate(asset, true);
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
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
