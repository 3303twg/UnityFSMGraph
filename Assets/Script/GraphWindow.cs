using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
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

        

        #region 툴바
        //testGraphView.style.flexGrow = 1;
        //rootVisualElement.Add(testGraphView);

        /*
        var toolbar = new Toolbar();
        var addButton = new ToolbarButton(() =>
        {
            testGraphView.CreateNode(new Vector2(300, 200));
        });
        addButton.text = "Add Node";
        toolbar.Add(addButton);
        rootVisualElement.Insert(0, toolbar);
        */
        #endregion

        ///////////
        var split = new TwoPaneSplitView(1, 320, TwoPaneSplitViewOrientation.Horizontal);


        graphInspectorView = new GraphWindowInspectorView();
        testGraphView = new TestGraphView(graph, graphInspectorView);

        graphInspectorView.OnNodeDataChanged = () => testGraphView.RefreshAllNodeViews();

        split.Add(testGraphView);
        split.Add(graphInspectorView);

        root.Add(split);

        testGraphView.BindSelection();

        /*
        inspectorView = new FsmGraphInspectorView();
        graphView = new FsmGraphView(asset, inspectorView);
        inspectorView.OnNodeDataChanged = () => graphView.RefreshAllNodeViews();
        inspectorView.OnFocusNodeRequested = FocusNodeInGraph;
        graphView.BindSelection();
        graphView.OnGraphChanged += () =>
        {
            SaveGraph(silent: true);
            RefreshNodeViews();
        };

        split.Add(graphView);
        split.Add(inspectorView);*/
    }

    private void OnEnable()
    {
        if (currentGraph != null)
            LoadGraph(currentGraph);
    }
}
