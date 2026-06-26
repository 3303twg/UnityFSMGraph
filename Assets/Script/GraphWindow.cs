using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GraphWindow : EditorWindow
{
    private TestGraphView testGraphView;

    [MenuItem("Tool/GraphView")]
    static void Open()
    {
        GetWindow<GraphWindow>();
    }

    private void OnEnable()
    {
        testGraphView = new TestGraphView();

        rootVisualElement.Add(testGraphView);
    }
}
