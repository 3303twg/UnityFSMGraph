using System;
using Unity.VisualScripting;
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
        if (currentGraph.blackboard == null)
        {
            currentGraph.blackboard = new Blackboard();
            currentGraph.blackboard.blackboardDic["Test"] = (object)5.4f;
            currentGraph.blackboard.blackboardDic["Test2"] = (object)23;
        }
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

        root.Add(BuildBlackboardPanel());

        


        ReBuildBlackboard();
        testGraphView.BindSelection();

        if (EditorApplication.isPlaying && !string.IsNullOrEmpty(FSMGraphRuntimeDebugger.ActiveNodeId))
            testGraphView.SetActiveNode(FSMGraphRuntimeDebugger.ActiveNodeId);

        
    }
    VisualElement blackboardContentRoot;

    VisualElement BuildBlackboardPanel()
    {
        var panel = new VisualElement();
        panel.style.position = Position.Absolute;
        panel.style.top = 8;
        panel.style.left = 8;
        panel.style.width = 220;
        panel.style.maxHeight = 300;
        panel.style.backgroundColor = new Color(0.15f, 0.15f, 0.17f, 0.95f);
        panel.style.paddingTop = 8;
        panel.style.paddingBottom = 8;
        panel.style.paddingLeft = 10;
        panel.style.paddingRight = 10;
        panel.style.borderTopWidth = 1;
        panel.style.borderBottomWidth = 1;
        panel.style.borderLeftWidth = 1;
        panel.style.borderRightWidth = 1;
        panel.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
        panel.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
        panel.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
        panel.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        panel.style.borderTopLeftRadius = 6;
        panel.style.borderTopRightRadius = 6;
        panel.style.borderBottomLeftRadius = 6;
        panel.style.borderBottomRightRadius = 6;
        var title = new Label("Blackboard");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 6;
        panel.Add(title);


        

        blackboardContentRoot = new VisualElement();
        var scroll = new ScrollView();
        scroll.Add(blackboardContentRoot);
        panel.Add(scroll);

        //아 만들고보니 object로 하니까 좀 어중간해졌네
        var button = new Button(() =>
        {
            string tempName = Guid.NewGuid().ToString();
            currentGraph.blackboard.blackboardDic[tempName] = 5.2f;
            ReBuildBlackboard();

            blackboardContentRoot.schedule.Execute(() =>
            {
                foreach (var textfield in blackboardContentRoot.Query<TextField>().ToList())
                {
                    if(textfield.text == tempName)
                        textfield?.Focus();
                }
                
            });
            
            /*
            var provider = ScriptableObject.CreateInstance<TypeSearchProvider>();
            provider.Init(key =>
            {

                //Guid.NewGuid().ToString();
                //currentGraph.blackboard.blackboardDic[key] = null;
                //BuildBlackboardPanel(); 레거시임 리빌드 해야함 애초에 쓰지도 않아 이부분

                

            });*/
        });
        button.text = "Add";
        panel.Add(button);
        return panel;
    }

    void ReBuildBlackboard()
    {
        blackboardContentRoot.Clear();
        blackboardContentRoot.Add(BuildBlackboardFields());
    }
    VisualElement BuildBlackboardFields()
    {
        var section = new VisualElement();
        section.style.flexDirection = FlexDirection.Column;

        foreach (var key in currentGraph.blackboard.blackboardDic.Keys)
        {
            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            if (!currentGraph.blackboard.blackboardDic.TryGetValue(key, out object value))
                continue;
            
            if (value is float floatValue)
            {
                //var field = new FloatField("Tesxxt") { value = floatValue };
                var textfield = new TextField()
                {
                    value = key
                };
                textfield.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return)
                    {
                        
                        if (key == textfield.value) return;
                        currentGraph.blackboard.blackboardDic[textfield.value] = currentGraph.blackboard.blackboardDic[key];
                        currentGraph.blackboard.blackboardDic.Remove(key);
                        EditorUtility.SetDirty(currentGraph);
                        ReBuildBlackboard();
                        return;
                    }
                });
                textfield.RegisterCallback<FocusOutEvent>(evt =>
                { 

                    if (key == textfield.value) return;
                    currentGraph.blackboard.blackboardDic[textfield.value] = currentGraph.blackboard.blackboardDic[key];
                    currentGraph.blackboard.blackboardDic.Remove(key);
                    EditorUtility.SetDirty(currentGraph);
                    ReBuildBlackboard();
                    return;
                });

                element.Add(textfield);
                var field = new FloatField() { value = floatValue };
                field.RegisterValueChangedCallback(evt => currentGraph.blackboard.Set<float>(key, evt.newValue));
                element.Add(field);
                

            }

            else if (value is int intValue)
            {
                
                element.style.flexDirection = FlexDirection.Row;
                var field = new IntegerField("TExxxtt") { value = intValue };
                field.RegisterValueChangedCallback(evt => currentGraph.blackboard.Set<int>(key, evt.newValue));
                element.Add(field);
            }
            //스트링 있을수 예외
            /* else
             {
                 var field = new TextValueField<string>(key.ToString()) { value = intValue };
                 field.RegisterValueChangedCallback(evt => currentGraph.blackboard.Set<string>(key, evt.newValue));
                 section.Add(field);
                 continue;
             }*/

            //section.Add(new Label($"{key}: {value}"));

            section.Add(element);

            var button = new Button(() =>
            { 

                currentGraph.blackboard.blackboardDic.Remove(key);
                ReBuildBlackboard();
            });
            button.text = "X";
            element.Add(button);



        }
        return (section);
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

            foreach (var controller in FindObjectsOfType<EnemyController>())
                controller.RuntimeDebugState = null;
        }

        if (state == PlayModeStateChange.EnteredPlayMode ||
            state == PlayModeStateChange.EnteredEditMode)
        {
            graphInspectorView?.RebuildNodeInspector();
        }
    }
}
