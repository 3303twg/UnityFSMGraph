using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphWindow : EditorWindow
{
    private FSMGraphView graphView;
    private FSMGraphSo currentGraph;
    [SerializeField] FSMAgent boundAgent;
    private Blackboard activeBlackboard;
    private GraphWindowInspectorView graphInspectorView;
    VisualElement blackboardContentRoot;
    ObjectField contextAgentField;
    bool syncingSelection;
    bool blackboardExpanded = true;
    Vector2 blackboardPanelOffset = new Vector2(8f, 8f);
    Vector2 blackboardPanelSize = new Vector2(320f, 360f);
    const float BlackboardMinWidth = 200f;
    const float BlackboardMinHeight = 140f;
    Vector3 blackboardDragStartPointer;
    Vector2 blackboardDragStartOffset;

    [MenuItem("Tool/GraphView")]
    static void OpenFromMenu()
    {
        FSMAgent agent = FindAgentOnSelection();
        if (agent != null && agent.graphSo != null)
        {
            Open(agent.graphSo, agent);
            return;
        }

        var graph = Selection.activeObject as FSMGraphSo;
        if (graph != null)
            Open(graph);
        else
            GetWindow<GraphWindow>();
    }

    public static void Open(FSMGraphSo graph, FSMAgent agent = null)
    {
        if (graph == null) return;

        var wnd = GetWindow<GraphWindow>();
        wnd.boundAgent = agent;
        wnd.LoadGraph(graph);
    }

    static FSMAgent FindAgentOnSelection()
    {
        var selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
            return null;

        Transform current = selectedObject.transform;
        while (current != null)
        {
            var agent = current.GetComponent<FSMAgent>();
            if (agent != null)
                return agent;
            current = current.parent;
        }

        var components = selectedObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            FSMAgent referenced = FindDirectAgentReference(components[i]);
            if (referenced != null)
                return referenced;
        }

        return null;
    }

    internal static FSMAgent FindDirectAgentReference(Component component)
    {
        if (component == null || component is FSMAgent)
            return null;

        var serialized = new SerializedObject(component);
        SerializedProperty property = serialized.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue is FSMAgent referenced)
                return referenced;
        }

        return null;
    }

    void LoadGraph(FSMGraphSo graph)
    {
        UnbindBlackboard();

        currentGraph = graph;
        if (currentGraph.blackboard == null)
            currentGraph.blackboard = new Blackboard();

        activeBlackboard = null;
        if (EditorApplication.isPlaying)
            activeBlackboard = FindRuntimeBlackboard();

        BindBlackboard();

        UpdateWindowTitle();

        rootVisualElement.Clear();

        VisualElement root = new VisualElement { style = { flexGrow = 1 } };
        rootVisualElement.Add(root);

        rootVisualElement.Insert(0, BuildToolbar());

        var split = new TwoPaneSplitView(1, 220, TwoPaneSplitViewOrientation.Horizontal);

        graphInspectorView = new GraphWindowInspectorView();
        graphInspectorView.ContextAgent = boundAgent;
        graphView = new FSMGraphView(graph, graphInspectorView);

        graphInspectorView.OnNodeDataChanged = () => graphView.RefreshAllNodeViews();

        split.Add(graphView);
        split.Add(graphInspectorView);

        root.Add(split);
        root.Add(BuildBlackboardPanel());

        ReBuildBlackboard();
        graphView.BindSelection();

        if (EditorApplication.isPlaying && boundAgent != null)
            graphView.SetActiveNode(boundAgent.GraphRuntime?.CurrentNodeId);
    }

    Toolbar BuildToolbar()
    {
        var toolbar = new Toolbar();
        var addButton = new ToolbarButton(() =>
        {
            graphView.CreateNode(new Vector2(300, 200));
        });
        addButton.text = "Add Node";
        toolbar.Add(addButton);

        contextAgentField = new ObjectField("기준")
        {
            objectType = typeof(FSMAgent),
            allowSceneObjects = true,
            value = boundAgent
        };
        var agentField = contextAgentField;
        agentField.style.width = 280;
        agentField.style.marginLeft = 8;
        agentField.RegisterValueChangedCallback(evt =>
        {
            var next = evt.newValue as FSMAgent;
            if (next == boundAgent)
                return;

            if (next != null && next.graphSo == null)
            {
                agentField.SetValueWithoutNotify(boundAgent);
                Debug.LogWarning("이 에이전트에는 그래프가 없습니다.");
                return;
            }

            if (next != null && next.graphSo != currentGraph)
            {
                boundAgent = next;
                LoadGraph(next.graphSo);
                return;
            }

            ApplyContextAgent(next);
        });
        toolbar.Add(agentField);
        return toolbar;
    }

    void ApplyContextAgent(FSMAgent agent)
    {
        boundAgent = agent;
        UnbindBlackboard();
        activeBlackboard = EditorApplication.isPlaying ? FindRuntimeBlackboard() : null;
        BindBlackboard();
        ReBuildBlackboard();
        UpdateWindowTitle();

        contextAgentField?.SetValueWithoutNotify(boundAgent);

        if (graphInspectorView != null)
        {
            graphInspectorView.ContextAgent = boundAgent;
            graphInspectorView.RebuildNodeInspector();
        }

        if (EditorApplication.isPlaying && boundAgent != null)
            graphView?.SetActiveNode(boundAgent.GraphRuntime?.CurrentNodeId);
        else
            graphView?.ClearActiveNode();
    }

    void UpdateWindowTitle()
    {
        if (currentGraph == null)
            return;

        string agentName = boundAgent != null ? boundAgent.name : "기준 없음";
        titleContent = new GUIContent($"{currentGraph.name} ({agentName})");
    }

    Blackboard GetActiveBlackboard()
    {
        if (activeBlackboard != null)
            return activeBlackboard;
        return currentGraph?.blackboard;
    }

    void BindBlackboard()
    {
        var bb = GetActiveBlackboard();
        if (bb != null)
            bb.OnChanged += ReBuildBlackboard;
    }

    void UnbindBlackboard()
    {
        if (activeBlackboard != null)
            activeBlackboard.OnChanged -= ReBuildBlackboard;
        if (currentGraph?.blackboard != null)
            currentGraph.blackboard.OnChanged -= ReBuildBlackboard;
    }

    void MarkGraphDirtyIfEditing()
    {
        if (!EditorApplication.isPlaying && currentGraph != null)
            EditorUtility.SetDirty(currentGraph);
    }

    VisualElement BuildBlackboardPanel()
    {
        var panel = new VisualElement();
        panel.style.position = Position.Absolute;
        panel.style.top = blackboardPanelOffset.y;
        panel.style.left = blackboardPanelOffset.x;
        panel.style.width = blackboardPanelSize.x;
        panel.style.flexDirection = FlexDirection.Column;
        panel.style.overflow = Overflow.Hidden;
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

        var grip = new Label("::");
        grip.pickingMode = PickingMode.Ignore;
        grip.style.marginRight = 6;
        grip.style.color = new Color(0.65f, 0.65f, 0.65f);
        grip.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.Add(grip);

        var title = new Label("Blackboard");
        title.pickingMode = PickingMode.Ignore;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.flexGrow = 1;
        header.Add(title);

        blackboardContentRoot = new VisualElement();
        blackboardContentRoot.style.flexGrow = 1;

        var scroll = new ScrollView();
        scroll.style.flexGrow = 1;
        scroll.style.flexShrink = 1;
        scroll.style.minHeight = 0;
        scroll.Add(blackboardContentRoot);

        var addButton = new Button(() =>
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Play Mode에서는 블랙보드 키 추가를 지원하지 않습니다.");
                return;
            }

            var provider = ScriptableObject.CreateInstance<TypeSearchProvider>();
            provider.Init(type =>
            {
                string tempName = Guid.NewGuid().ToString();
                GetActiveBlackboard().Add(Blackboard.CreateVariable(type, tempName));
                MarkGraphDirtyIfEditing();
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
        header.Add(foldButton);

        var resizeRight = CreateBlackboardResizeHandle(panel, BlackboardResizeAxis.Horizontal);
        var resizeBottom = CreateBlackboardResizeHandle(panel, BlackboardResizeAxis.Vertical);
        var resizeCorner = CreateBlackboardResizeHandle(panel, BlackboardResizeAxis.Both);

        panel.Add(header);
        panel.Add(scroll);
        panel.Add(addButton);
        panel.Add(resizeRight);
        panel.Add(resizeBottom);
        panel.Add(resizeCorner);

        foldButton.clicked += () =>
        {
            blackboardExpanded = !blackboardExpanded;
            ApplyBlackboardFold(panel, foldButton, scroll, addButton, resizeRight, resizeBottom, resizeCorner);
        };

        EnableBlackboardDrag(panel, header, foldButton);
        ApplyBlackboardFold(panel, foldButton, scroll, addButton, resizeRight, resizeBottom, resizeCorner);
        return panel;
    }

    enum BlackboardResizeAxis
    {
        Horizontal,
        Vertical,
        Both
    }

    VisualElement CreateBlackboardResizeHandle(VisualElement panel, BlackboardResizeAxis axis)
    {
        var handle = new VisualElement();
        handle.style.position = Position.Absolute;
        handle.style.backgroundColor = Color.clear;

        if (axis == BlackboardResizeAxis.Horizontal)
        {
            handle.style.top = 0;
            handle.style.right = 0;
            handle.style.bottom = 14;
            handle.style.width = 6;
        }
        else if (axis == BlackboardResizeAxis.Vertical)
        {
            handle.style.left = 0;
            handle.style.right = 14;
            handle.style.bottom = 0;
            handle.style.height = 6;
        }
        else
        {
            handle.style.right = 0;
            handle.style.bottom = 0;
            handle.style.width = 14;
            handle.style.height = 14;
        }

        handle.style.cursor = CreateEditorCursor(ResizeMouseCursor(axis));
        EnableBlackboardResize(panel, handle, axis);
        return handle;
    }

    static readonly PropertyInfo CursorIdProperty =
        typeof(UnityEngine.UIElements.Cursor).GetProperty(
            "defaultCursorId",
            BindingFlags.Instance | BindingFlags.NonPublic);

    static UnityEngine.UIElements.Cursor CreateEditorCursor(MouseCursor cursor)
    {
        object boxed = new UnityEngine.UIElements.Cursor();
        if (CursorIdProperty != null)
            CursorIdProperty.SetValue(boxed, (int)cursor);
        else
        {
            FieldInfo field = typeof(UnityEngine.UIElements.Cursor).GetField(
                "<defaultCursorId>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(boxed, (int)cursor);
        }

        return (UnityEngine.UIElements.Cursor)boxed;
    }

    static MouseCursor ResizeMouseCursor(BlackboardResizeAxis axis)
    {
        switch (axis)
        {
            case BlackboardResizeAxis.Horizontal:
                return MouseCursor.ResizeHorizontal;
            case BlackboardResizeAxis.Vertical:
                return MouseCursor.ResizeVertical;
            default:
                return MouseCursor.ResizeUpLeft;
        }
    }

    void EnableBlackboardResize(VisualElement panel, VisualElement handle, BlackboardResizeAxis axis)
    {
        Vector3 startPointer = default;
        Vector2 startSize = default;

        handle.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != (int)MouseButton.LeftMouse || !blackboardExpanded)
                return;

            float width = panel.resolvedStyle.width;
            float height = panel.resolvedStyle.height;
            if (float.IsNaN(width))
                width = blackboardPanelSize.x;
            if (float.IsNaN(height))
                height = blackboardPanelSize.y;

            startPointer = evt.position;
            startSize = new Vector2(width, height);
            handle.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        });

        handle.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!handle.HasPointerCapture(evt.pointerId))
                return;

            Vector2 delta = (Vector2)(evt.position - startPointer);
            Vector2 size = startSize;
            if (axis != BlackboardResizeAxis.Vertical)
                size.x += delta.x;
            if (axis != BlackboardResizeAxis.Horizontal)
                size.y += delta.y;

            ApplyBlackboardSize(panel, size);
            evt.StopPropagation();
        });

        handle.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (!handle.HasPointerCapture(evt.pointerId))
                return;

            handle.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        });
    }

    void ApplyBlackboardSize(VisualElement panel, Vector2 size)
    {
        float maxWidth = 4000f;
        float maxHeight = 4000f;
        var parent = panel.parent;
        if (parent != null)
        {
            float parentWidth = parent.resolvedStyle.width;
            float parentHeight = parent.resolvedStyle.height;
            if (!float.IsNaN(parentWidth))
                maxWidth = parentWidth - blackboardPanelOffset.x;
            if (!float.IsNaN(parentHeight))
                maxHeight = parentHeight - blackboardPanelOffset.y;
        }

        size.x = Mathf.Clamp(size.x, BlackboardMinWidth, Mathf.Max(BlackboardMinWidth, maxWidth));
        size.y = Mathf.Clamp(size.y, BlackboardMinHeight, Mathf.Max(BlackboardMinHeight, maxHeight));

        bool widthChanged = float.IsNaN(panel.resolvedStyle.width) ||
                            !Mathf.Approximately(panel.resolvedStyle.width, size.x);
        bool heightChanged = blackboardExpanded &&
                             (float.IsNaN(panel.resolvedStyle.height) ||
                              !Mathf.Approximately(panel.resolvedStyle.height, size.y));
        if (!widthChanged && !heightChanged && blackboardPanelSize == size)
        {
            ApplyBlackboardPosition(panel, blackboardPanelOffset);
            return;
        }

        blackboardPanelSize = size;
        panel.style.width = size.x;
        if (blackboardExpanded)
            panel.style.height = size.y;

        ApplyBlackboardPosition(panel, blackboardPanelOffset);
    }

    void EnableBlackboardDrag(VisualElement panel, VisualElement dragHandle, VisualElement ignore)
    {
        dragHandle.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            var target = evt.target as VisualElement;
            if (target != null && (target == ignore || ignore.Contains(target)))
                return;

            float left = panel.resolvedStyle.left;
            float top = panel.resolvedStyle.top;
            if (float.IsNaN(left))
                left = blackboardPanelOffset.x;
            if (float.IsNaN(top))
                top = blackboardPanelOffset.y;

            blackboardDragStartPointer = evt.position;
            blackboardDragStartOffset = new Vector2(left, top);
            dragHandle.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        });

        dragHandle.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (!dragHandle.HasPointerCapture(evt.pointerId))
                return;

            Vector2 delta = (Vector2)(evt.position - blackboardDragStartPointer);
            ApplyBlackboardPosition(panel, blackboardDragStartOffset + delta);
            evt.StopPropagation();
        });

        dragHandle.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (!dragHandle.HasPointerCapture(evt.pointerId))
                return;

            dragHandle.ReleasePointer(evt.pointerId);
            evt.StopPropagation();
        });

        panel.RegisterCallback<AttachToPanelEvent>(_ =>
        {
            panel.parent?.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                ApplyBlackboardSize(panel, blackboardPanelSize);
            });
        });
    }

    void ApplyBlackboardPosition(VisualElement panel, Vector2 position)
    {
        var parent = panel.parent;
        if (parent != null)
        {
            float parentWidth = parent.resolvedStyle.width;
            float parentHeight = parent.resolvedStyle.height;
            float panelWidth = panel.resolvedStyle.width;
            float panelHeight = panel.resolvedStyle.height;

            if (!float.IsNaN(parentWidth) && !float.IsNaN(panelWidth))
                position.x = Mathf.Clamp(position.x, 0f, Mathf.Max(0f, parentWidth - panelWidth));
            if (!float.IsNaN(parentHeight) && !float.IsNaN(panelHeight))
                position.y = Mathf.Clamp(position.y, 0f, Mathf.Max(0f, parentHeight - panelHeight));
        }

        blackboardPanelOffset = position;

        float currentLeft = panel.resolvedStyle.left;
        float currentTop = panel.resolvedStyle.top;
        if (!float.IsNaN(currentLeft) && !float.IsNaN(currentTop) &&
            Mathf.Approximately(currentLeft, position.x) &&
            Mathf.Approximately(currentTop, position.y))
            return;

        panel.style.left = position.x;
        panel.style.top = position.y;
    }

    void ApplyBlackboardFold(
        VisualElement panel,
        Button foldButton,
        VisualElement scroll,
        VisualElement addButton,
        params VisualElement[] resizeHandles)
    {
        foldButton.text = blackboardExpanded ? "▾" : "▸";
        DisplayStyle contentDisplay = blackboardExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        scroll.style.display = contentDisplay;
        addButton.style.display = contentDisplay;
        for (int i = 0; i < resizeHandles.Length; i++)
            resizeHandles[i].style.display = contentDisplay;

        panel.style.maxHeight = blackboardExpanded ? StyleKeyword.None : 44;
        panel.style.height = blackboardExpanded ? blackboardPanelSize.y : 44;
        panel.style.minHeight = blackboardExpanded ? BlackboardMinHeight : 44;
        panel.schedule.Execute(() => ApplyBlackboardPosition(panel, blackboardPanelOffset));
    }

    void ReBuildBlackboard()
    {
        if (blackboardContentRoot == null)
            return;

        blackboardContentRoot.Clear();
        blackboardContentRoot.Add(BuildBlackboardFields());
    }

    VisualElement BuildBlackboardFields()
    {
        var section = new VisualElement();
        section.style.flexDirection = FlexDirection.Column;
        section.style.width = Length.Percent(100);

        var blackboard = GetActiveBlackboard();
        if (blackboard == null)
        {
            section.Add(new Label("Blackboard 없음"));
            return section;
        }

        foreach (var pair in blackboard.lookUp)
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
                        GetActiveBlackboard().Set(key, evt.newValue, false);
                        MarkGraphDirtyIfEditing();
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
                        GetActiveBlackboard().Set(key, evt.newValue, false);
                        MarkGraphDirtyIfEditing();
                    });
                    valueField = toggle;
                    break;
                }
                case StringVariable stringVariable:
                {
                    var field = new TextField { value = stringVariable.value ?? string.Empty };
                    field.RegisterValueChangedCallback(evt =>
                    {
                        GetActiveBlackboard().Set(key, evt.newValue, false);
                        MarkGraphDirtyIfEditing();
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
                        GetActiveBlackboard().Set(key, evt.newValue, false);
                        MarkGraphDirtyIfEditing();
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
                if (EditorApplication.isPlaying)
                {
                    Debug.LogWarning("Play Mode에서는 블랙보드 키 삭제를 지원하지 않습니다.");
                    return;
                }

                GetActiveBlackboard().Remove(key);
                MarkGraphDirtyIfEditing();
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
            if (EditorApplication.isPlaying)
            {
                keyField.SetValueWithoutNotify(key);
                return;
            }

            string newKey = keyField.value?.Trim();
            if (string.IsNullOrEmpty(newKey) || newKey == key)
                return;

            var blackboard = GetActiveBlackboard();
            if (blackboard.lookUp.ContainsKey(newKey))
            {
                Debug.LogWarning($"이미 존재하는 키: {newKey}");
                keyField.SetValueWithoutNotify(key);
                return;
            }

            blackboard.Rename(key, newKey);
            MarkGraphDirtyIfEditing();
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
        Selection.selectionChanged += OnEditorSelectionChanged;

        if (currentGraph != null)
            LoadGraph(currentGraph);
    }

    private void OnDisable()
    {
        UnbindBlackboard();
        FSMGraphRuntimeDebugger.ActiveNodeChanged -= OnActiveNodeChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        Selection.selectionChanged -= OnEditorSelectionChanged;
    }

    void OnEditorSelectionChanged()
    {
        if (syncingSelection)
            return;

        FSMAgent agent = FindAgentOnSelection();
        if (agent == null || agent.graphSo == null)
            return;

        if (agent == boundAgent && agent.graphSo == currentGraph)
            return;

        syncingSelection = true;
        try
        {
            if (agent.graphSo != currentGraph)
            {
                boundAgent = agent;
                LoadGraph(agent.graphSo);
            }
            else
                ApplyContextAgent(agent);
        }
        finally
        {
            syncingSelection = false;
        }
    }

    void OnActiveNodeChanged(UnityEngine.Object owner, string nodeId)
    {
        if (boundAgent == null || owner != boundAgent)
            return;

        graphView?.SetActiveNode(nodeId);
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            graphView?.ClearActiveNode();
            FSMGraphRuntimeDebugger.ClearActiveNode();

            foreach (var agent in FindObjectsOfType<FSMAgent>())
                agent.RuntimeDebugState = null;
        }

        if (state == PlayModeStateChange.EnteredPlayMode ||
            state == PlayModeStateChange.EnteredEditMode)
        {
            graphInspectorView?.RebuildNodeInspector();
        }

        if (currentGraph == null)
            return;

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            UnbindBlackboard();
            activeBlackboard = null;

            // Agent Awake/Init보다 먼저 올 수 있어서 한 프레임 뒤 재탐색
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlaying || currentGraph == null)
                    return;

                UnbindBlackboard();
                activeBlackboard = FindRuntimeBlackboard();
                BindBlackboard();
                ReBuildBlackboard();
                graphInspectorView?.RebuildNodeInspector();
                if (boundAgent != null)
                    graphView?.SetActiveNode(boundAgent.GraphRuntime?.CurrentNodeId);
            };
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            UnbindBlackboard();
            activeBlackboard = null; // SO 템플릿으로 복귀 (SO 필드는 건드리지 않음)
            BindBlackboard();
            ReBuildBlackboard();
        }
    }

    Blackboard FindRuntimeBlackboard()
    {
        if (!EditorApplication.isPlaying || currentGraph == null || boundAgent == null)
            return null;

        if (boundAgent.graphSo != currentGraph)
            return null;

        return boundAgent.GraphRuntime?.blackboard;
    }
}
