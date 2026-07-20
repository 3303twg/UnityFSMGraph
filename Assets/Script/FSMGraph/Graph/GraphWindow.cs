using System;
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
    VisualElement blackboardContentRoot;
    bool blackboardExpanded = true;

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
            currentGraph.blackboard = new Blackboard();

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

    VisualElement BuildBlackboardPanel()
    {
        var panel = new VisualElement();
        panel.style.position = Position.Absolute;
        panel.style.top = 8;
        panel.style.left = 8;
        panel.style.width = 320;
        panel.style.maxHeight = 540;
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

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.alignItems = Align.Center;
        header.style.justifyContent = Justify.SpaceBetween;
        header.style.marginBottom = 6;
        header.style.minHeight = 22;

        var title = new Label("Blackboard");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.flexGrow = 1;
        header.Add(title);

        blackboardContentRoot = new VisualElement();
        blackboardContentRoot.style.flexGrow = 1;

        var scroll = new ScrollView();
        scroll.style.flexGrow = 1;
        scroll.style.minHeight = 120;
        scroll.style.maxHeight = 540;
        scroll.Add(blackboardContentRoot);

        var addButton = new Button(() =>
        {
            var provider = ScriptableObject.CreateInstance<TypeSearchProvider>();
            provider.Init(type =>
            {
                string tempName = Guid.NewGuid().ToString();
                currentGraph.blackboard.Add(Blackboard.CreateVariable(type, tempName));
                EditorUtility.SetDirty(currentGraph);
                ReBuildBlackboard();

                blackboardContentRoot.schedule.Execute(() =>
                {
                    foreach (var textfield in blackboardContentRoot.Query<TextField>().ToList())
                    {
                        if (textfield.value == tempName)
                            textfield.Focus();
                    }
                });
            });

            SearchWindow.Open(
                new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                provider);
        });
        addButton.text = "Add";
        addButton.style.marginTop = 6;
        addButton.style.height = 22;

        var foldButton = new Button();
        foldButton.style.width = 22;
        foldButton.style.height = 20;
        foldButton.style.flexShrink = 0;
        foldButton.style.marginLeft = 4;
        foldButton.style.unityTextAlign = TextAnchor.MiddleCenter;
        foldButton.clicked += () =>
        {
            blackboardExpanded = !blackboardExpanded;
            ApplyBlackboardFold(panel, foldButton, scroll, addButton);
        };
        header.Add(foldButton);

        panel.Add(header);
        panel.Add(scroll);
        panel.Add(addButton);

        ApplyBlackboardFold(panel, foldButton, scroll, addButton);
        return panel;
    }

    void ApplyBlackboardFold(VisualElement panel, Button foldButton, VisualElement scroll, VisualElement addButton)
    {
        foldButton.text = blackboardExpanded ? "▾" : "▸";
        scroll.style.display = blackboardExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        addButton.style.display = blackboardExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        panel.style.maxHeight = blackboardExpanded ? 540 : 44;
        panel.style.height = blackboardExpanded ? StyleKeyword.Auto : 44;
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
        section.style.width = Length.Percent(100);

        foreach (var pair in currentGraph.blackboard.lookUp)
        {
            string key = pair.Key;
            BlackboardVariable variable = pair.Value;
            if (variable == null)
                continue;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;
            row.style.minHeight = 22;
            row.style.width = Length.Percent(100);

            row.Add(CreateKeyField(key));

            VisualElement valueField;
            switch (variable)
            {
                case FloatVariable floatVariable:
                {
                    var field = new FloatField { value = floatVariable.value };
                    field.RegisterValueChangedCallback(evt =>
                    {
                        currentGraph.blackboard.Set(key, evt.newValue);
                        EditorUtility.SetDirty(currentGraph);
                    });
                    valueField = field;
                    break;
                }
                case BoolVariable boolVariable:
                {
                    var toggle = new Toggle { value = boolVariable.value };
                    toggle.style.marginLeft = 2;
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        currentGraph.blackboard.Set(key, evt.newValue);
                        EditorUtility.SetDirty(currentGraph);
                    });
                    valueField = toggle;
                    break;
                }
                case StringVariable stringVariable:
                {
                    var field = new TextField { value = stringVariable.value ?? string.Empty };
                    field.RegisterValueChangedCallback(evt =>
                    {
                        currentGraph.blackboard.Set(key, evt.newValue);
                        EditorUtility.SetDirty(currentGraph);
                    });
                    valueField = field;
                    break;
                }
                case GameObjectVariable gameObjectVariable:
                {
                    var objectField = new ObjectField
                    {
                        objectType = typeof(GameObject),
                        allowSceneObjects = false,
                        value = gameObjectVariable.value
                    };
                    objectField.label = string.Empty;
                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        currentGraph.blackboard.Set(key, evt.newValue);
                        EditorUtility.SetDirty(currentGraph);
                    });
                    valueField = objectField;
                    break;
                }
                default:
                    valueField = new Label(variable.GetType().Name);
                    break;
            }

            StyleValueField(valueField);
            row.Add(valueField);

            var removeButton = new Button(() =>
            {
                currentGraph.blackboard.Remove(key);
                EditorUtility.SetDirty(currentGraph);
                ReBuildBlackboard();
            })
            {
                text = "X"
            };
            removeButton.style.width = 22;
            removeButton.style.height = 20;
            removeButton.style.marginLeft = 4;
            removeButton.style.flexShrink = 0;
            row.Add(removeButton);

            section.Add(row);
        }

        return section;
    }

    static void StyleValueField(VisualElement field)
    {
        field.style.flexGrow = 1;
        field.style.flexShrink = 1;
        field.style.minWidth = 120;
        field.style.height = 20;
        field.style.marginLeft = 4;
        field.style.marginRight = 0;
    }

    TextField CreateKeyField(string key)
    {
        var keyField = new TextField { value = key };
        keyField.style.width = 88;
        keyField.style.minWidth = 72;
        keyField.style.maxWidth = 100;
        keyField.style.height = 20;
        keyField.style.flexShrink = 0;
        keyField.style.marginRight = 0;

        void TryRename()
        {
            string newKey = keyField.value?.Trim();
            if (string.IsNullOrEmpty(newKey) || newKey == key)
                return;

            if (currentGraph.blackboard.lookUp.ContainsKey(newKey))
            {
                Debug.LogWarning($"이미 존재하는 키: {newKey}");
                keyField.SetValueWithoutNotify(key);
                return;
            }

            currentGraph.blackboard.Rename(key, newKey);
            EditorUtility.SetDirty(currentGraph);
            ReBuildBlackboard();
        }

        keyField.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                return;

            TryRename();
            evt.StopPropagation();
        });

        keyField.RegisterCallback<FocusOutEvent>(_ => TryRename());
        return keyField;
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

            foreach (var agent in FindObjectsOfType<FSMAgent>())
                agent.RuntimeDebugState = null;
        }

        if (state == PlayModeStateChange.EnteredPlayMode ||
            state == PlayModeStateChange.EnteredEditMode)
        {
            graphInspectorView?.RebuildNodeInspector();
        }
    }
}
