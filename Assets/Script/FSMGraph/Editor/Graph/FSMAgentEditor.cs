using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FSMAgent))]
public class FSMAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var agent = (FSMAgent)target;
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(agent.graphSo == null))
        {
            if (GUILayout.Button("Open Graph"))
                GraphWindow.Open(agent.graphSo, agent);
        }

        if (agent.graphSo == null)
            EditorGUILayout.HelpBox("graphSo를 넣어야 그래프를 열 수 있습니다.", MessageType.Info);
    }
}
