#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Built-in → URP 전환 부트스트랩.
/// 패키지 임포트 후 자동 또는 메뉴로 Pipeline Asset 생성/할당.
/// </summary>
public static class UrpProjectSetup
{
    const string SettingsFolder = "Assets/Settings";
    const string RendererPath = SettingsFolder + "/URP_Renderer.asset";
    const string PipelinePath = SettingsFolder + "/URP_Pipeline.asset";

    [InitializeOnLoadMethod]
    static void AutoSetupWhenNeeded()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (GraphicsSettings.defaultRenderPipeline != null) return;
            if (!PackageReady()) return;
            SetupUrp(silent: true);
        };
    }

    [MenuItem("Tools/FSM/Convert Project To URP")]
    public static void SetupUrpMenu()
    {
        if (!PackageReady())
        {
            EditorUtility.DisplayDialog(
                "URP",
                "com.unity.render-pipelines.universal 패키지가 아직 준비되지 않았습니다.\nUnity가 패키지 임포트를 끝낸 뒤 다시 실행하세요.",
                "OK");
            return;
        }

        SetupUrp(silent: false);
    }

    static bool PackageReady()
    {
        return System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime") != null
               || typeof(UniversalRenderPipelineAsset) != null;
    }

    static void SetupUrp(bool silent)
    {
        EnsureFolder(SettingsFolder);

        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
        }
        else
        {
            // 기존 파이프라인에 렌더러 연결 보장
            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            if (list != null && list.arraySize > 0)
            {
                list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;

        // 모든 Quality Level에 동일 파이프라인 지정
        int qualityCount = QualitySettings.names.Length;
        for (int i = 0; i < qualityCount; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
        }
        QualitySettings.SetQualityLevel(Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, qualityCount - 1), true);

        EditorUtility.SetDirty(pipeline);
        EditorUtility.SetDirty(renderer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 간단 머티리얼 업그레이드 시도 (Built-in Standard → URP)
        try
        {
            EditorApplication.ExecuteMenuItem("Edit/Rendering/Materials/Convert Selected Built-in Materials to URP");
        }
        catch
        {
            // 메뉴 경로/선택 없을 수 있음 — 무시
        }

        if (!silent)
        {
            EditorUtility.DisplayDialog(
                "URP 설정 완료",
                "Universal Render Pipeline Asset 생성 및 Graphics/Quality 할당 완료.\n\n" +
                $"Pipeline: {PipelinePath}\nRenderer: {RendererPath}\n\n" +
                "핑크 머티리얼이 있으면:\nEdit > Rendering > Materials > Convert All Built-in Materials to URP",
                "OK");
        }
        else
        {
            Debug.Log($"[URP] Pipeline assigned: {PipelinePath}");
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent ?? "Assets", name);
    }
}
#endif
