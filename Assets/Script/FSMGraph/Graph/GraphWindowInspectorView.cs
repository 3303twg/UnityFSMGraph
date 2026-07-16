using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class GraphWindowInspectorView : VisualElement
{
    readonly VisualElement root;
    NodeData boundNode;
    EdgeData boundEdge;
    FSMGraphSo graphDataSo;

    EnemyController boundController;
    SerializedObject boundControllerSo;
    VisualElement runtimeStateInspector;

    public Action OnNodeDataChanged;

    public GraphWindowInspectorView()
    {
        style.minWidth = 280;
        style.paddingLeft = 8;
        style.paddingRight = 8;
        style.paddingTop = 8;
        style.backgroundColor = new Color(0.18f, 0.18f, 0.2f);

        root = new VisualElement();
        Add(root);
        Clear();

        RegisterCallback<DetachFromPanelEvent>(_ => UnbindRuntimeInspector());
    }

    public void Clear()
    {
        boundNode = null;
        boundEdge = null;
        UnbindRuntimeInspector();
        root.Clear();
        root.Add(new Label("인스펙터 리셋 완료"));
    }

    public void BindNode(NodeData nodeData, FSMGraphSo graph)
    {
        boundNode = nodeData;
        boundEdge = null;
        graphDataSo = graph;
        RebuildNodeInspector();
    }

    public void BindEdge(EdgeData edgeData, FSMGraphSo graph)
    {
        boundNode = null;
        boundEdge = edgeData;
        graphDataSo = graph;
        RebuildEdgeInspector();
    }

    public void RebuildNodeInspector()
    {
        UnbindRuntimeInspector();
        root.Clear();

        if (boundNode == null) return;

        bool isPlaying = EditorApplication.isPlaying;

        root.Add(MakeHeader(isPlaying ? "Play Mode" : "Edit Mode"));
        root.Add(MakeHeader("Node: " + boundNode.nodeType));

        if (!isPlaying)
        {
            BuildEditModeNodeInspector(boundNode);
            return;
        }

        BuildPlayModeNodeInspector(boundNode);
    }

    void BuildEditModeNodeInspector(NodeData node)
    {
        TextField titleField = new TextField("Title") { value = boundNode.title };
        titleField.RegisterValueChangedCallback(evt =>
        {
            boundNode.title = evt.newValue;
            MarkDirty();
        });
        root.Add(titleField);

        if (node.nodeType == NodeType.Entry)
        {
            root.Add(new Label("엔트리 노드"));
            return;
        }

        if (node.nodeType == NodeType.Reference)
        {
            //root.Add(new Label("엔트리 노드"));

            root.Add(new UnityEngine.UIElements.Button(() =>
                {
                    var provider = ScriptableObject.CreateInstance<NodeSearcProvider>();
                    provider.Init(graphDataSo, node.id, target =>
                    {
                        node.referenceTargetId = target.id;
                        node.title = "Ref => " + target.title;
                        MarkDirty();
                        //EditorUtility.SetDirty(graphDataSo);
                        RebuildNodeInspector();
                    });

                    Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                    SearchWindow.Open(new SearchWindowContext(screenPos), provider);

                })
            {
                text = "Select Target"
            });

            
            return;
        }

        ObjectField objectField = new ObjectField("StateSo")
        {
            objectType = typeof(BaseStateSoAsset),
            value = node.stateSo,
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            node.stateSo = evt.newValue as BaseStateSoAsset;
            MarkDirty();
            RebuildNodeInspector();
        });
        root.Add(objectField);

        if (node.stateSo == null) return;

        var so = new SerializedObject(node.stateSo);
        var inspector = new InspectorElement(so);
        root.Add(MakeSection("State SO (원본)", inspector));
    }

    void BuildPlayModeNodeInspector(NodeData node)
    {
        root.Add(new Label($"Title: {node.title}"));

        boundController = FindController(graphDataSo);
        if (boundController == null)
        {
            root.Add(new Label("이 그래프를 실행 중인 EnemyController가 없습니다."));
            return;
        }

        //BuildBlackboardFields(boundController);

        if (node.nodeType == NodeType.Entry || node.stateSo == null)
        {
            root.Add(new Label("런타임 State 없음"));
            return;
        }

        if (!boundController.GraphRuntime.TryGetState(node.id, out BaseState runtimeState))
        {
            root.Add(new Label("이 노드의 런타임 State가 없습니다."));
            return;
        }

        boundController.RuntimeDebugState = runtimeState;

        boundControllerSo = new SerializedObject(boundController);
        boundControllerSo.Update();

        SerializedProperty stateProp = boundControllerSo.FindProperty("runtimeDebugState");
        if (stateProp == null)
        {
            root.Add(new Label("runtimeDebugState 프로퍼티를 찾을 수 없습니다."));
            return;
        }

        var propertyField = new PropertyField(stateProp);
        propertyField.Bind(boundControllerSo);
        runtimeStateInspector = propertyField;
        root.Add(MakeSection("Runtime State (플레이 중만 반영)", propertyField));

        var reEvaluateButton = new UnityEngine.UIElements.Button(() =>
        {
            if (runtimeState is CompareState compareState)
            {
                boundController.GraphRuntime.CurrentNodeId = node.id;
                compareState.Enter();
            }
        })
        {
            text = "Compare Re-Evaluate"
        };
        reEvaluateButton.style.marginTop = 8;
        root.Add(reEvaluateButton);

        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }
    /*
    void BuildBlackboardFields(EnemyController controller)
    {
        var section = new VisualElement();

        foreach (BlackboardKey key in Enum.GetValues(typeof(BlackboardKey)))
        {
            if (!controller.blackboard.TryGetValue(key, out object value))
                continue;

            if (value is float floatValue)
            {
                var field = new FloatField(key.ToString()) { value = floatValue };
                field.RegisterValueChangedCallback(evt =>
                {
                    controller.SetBlackboardValue(key, evt.newValue);
                    if (key == BlackboardKey.CurHp)
                        controller.enemyStat.hp = evt.newValue;
                });
                section.Add(field);
                continue;
            }

            if (value is int intValue)
            {
                var field = new IntegerField(key.ToString()) { value = intValue };
                field.RegisterValueChangedCallback(evt => controller.SetBlackboardValue(key, evt.newValue));
                section.Add(field);
                continue;
            }

            section.Add(new Label($"{key}: {value}"));
        }

        root.Add(MakeSection("Blackboard (런타임 값)", section));
    }*/

    void OnEditorUpdate()
    {
        if (!EditorApplication.isPlaying || boundControllerSo == null)
            return;

        boundControllerSo.Update();

        if (boundControllerSo.hasModifiedProperties)
            boundControllerSo.ApplyModifiedProperties();
    }

    void UnbindRuntimeInspector()
    {
        EditorApplication.update -= OnEditorUpdate;
        boundController = null;
        boundControllerSo = null;
        runtimeStateInspector = null;
    }

    static EnemyController FindController(FSMGraphSo graph)
    {
        if (graph == null) return null;

        var controllers = UnityEngine.Object.FindObjectsOfType<EnemyController>();
        foreach (var controller in controllers)
        {
            if (controller.graphSo == graph)
                return controller;
        }

        return null;
    }

    public void RebuildEdgeInspector()
    {
        UnbindRuntimeInspector();
        root.Clear();

        if (boundEdge == null) return;

        root.Add(MakeHeader("Edge"));
        root.Add(new Label($"From: {boundEdge.outputNodeId}"));
        root.Add(new Label($"To: {boundEdge.inputNodeId}"));
        root.Add(new Label($"Port: {boundEdge.outputPortType}"));
    }

    Label MakeHeader(string text)
    {
        var label = new Label(text);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.marginBottom = 6;
        return label;
    }

    void MarkDirty()
    {
        if (graphDataSo != null)
            EditorUtility.SetDirty(graphDataSo);
        OnNodeDataChanged?.Invoke();
    }

    VisualElement MakeSection(string title, VisualElement content)
    {
        var section = new VisualElement();
        section.style.marginTop = 10;
        section.style.paddingTop = 8;
        section.style.paddingBottom = 8;
        section.style.paddingLeft = 6;
        section.style.paddingRight = 6;
        section.style.backgroundColor = new Color(0.14f, 0.14f, 0.16f);
        section.style.borderTopWidth = 1;
        section.style.borderBottomWidth = 1;
        section.style.borderLeftWidth = 1;
        section.style.borderRightWidth = 1;
        section.style.borderTopColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderBottomColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderLeftColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderRightColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderTopLeftRadius = 4;
        section.style.borderTopRightRadius = 4;
        section.style.borderBottomLeftRadius = 4;
        section.style.borderBottomRightRadius = 4;

        var header = MakeHeader(title);
        header.style.marginBottom = 8;
        section.Add(header);
        section.Add(content);
        return section;
    }
}
