using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphWindow : EditorWindow
{
    private TestGraphView testGraphView;
    private FSMGraphSo currentGraph;

    [MenuItem("Tool/GraphView")]
    static void OpenFromMenu()
    {
        var graph = Selection.activeObject as FSMGraphSo;
        if (graph != null)
            Open(graph);
        else
            GetWindow<GraphWindow>();
    }

    public static void Open(FSMGraphSo graph)
    {
        if (graph == null) return;

        var wnd = GetWindow<GraphWindow>();
        wnd.LoadGraph(graph);
    }

    void LoadGraph(FSMGraphSo graph)
    {
        currentGraph = graph;
        titleContent = new GUIContent(graph.name);

        rootVisualElement.Clear();

        testGraphView = new TestGraphView(graph);
        testGraphView.style.flexGrow = 1;
        rootVisualElement.Add(testGraphView);

        var toolbar = new Toolbar();
        var addButton = new ToolbarButton(() =>
        {
            testGraphView.CreateNode(new Vector2(300, 200));
        });
        addButton.text = "Add Node";
        toolbar.Add(addButton);
        rootVisualElement.Insert(0, toolbar);
    }

    private void OnEnable()
    {
        if (currentGraph != null)
            LoadGraph(currentGraph);
    }
}
