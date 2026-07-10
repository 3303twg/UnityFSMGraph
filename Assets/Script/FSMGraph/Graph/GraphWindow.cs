using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphWindow : EditorWindow
{
    private TestGraphView testGraphView;
    private FSMGraphSo currentGraph;
    private GraphWindowInspectorView graphInspectorView;

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

        VisualElement root = new VisualElement { style = { flexGrow = 1 } };
        rootVisualElement.Add(root);

        var toolbar = new Toolbar();
        var addButton = new ToolbarButton(() =>
        {
            testGraphView.CreateNode(new Vector2(300, 200));
        });
        addButton.text = "Add Node";
        toolbar.Add(addButton);
        rootVisualElement.Insert(0, toolbar);

        var split = new TwoPaneSplitView(1, 320, TwoPaneSplitViewOrientation.Horizontal);

        graphInspectorView = new GraphWindowInspectorView();
        testGraphView = new TestGraphView(graph, graphInspectorView);

        graphInspectorView.OnNodeDataChanged = () => testGraphView.RefreshAllNodeViews();

        split.Add(testGraphView);
        split.Add(graphInspectorView);

        root.Add(split);

        testGraphView.BindSelection();

        if (EditorApplication.isPlaying && !string.IsNullOrEmpty(FSMGraphRuntimeDebugger.ActiveNodeId))
            testGraphView.SetActiveNode(FSMGraphRuntimeDebugger.ActiveNodeId);
    }

    private void OnEnable()
    {
        FSMGraphRuntimeDebugger.ActiveNodeChanged += OnActiveNodeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        if (currentGraph != null)
            LoadGraph(currentGraph);
    }

    private void OnDisable()
    {
        FSMGraphRuntimeDebugger.ActiveNodeChanged -= OnActiveNodeChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnActiveNodeChanged(string nodeId)
    {
        testGraphView?.SetActiveNode(nodeId);
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            testGraphView?.ClearActiveNode();
            FSMGraphRuntimeDebugger.ClearActiveNode();
        }
    }
}
