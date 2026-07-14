using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class TestGraphView : GraphView
{
    FSMGraphSo graphSo;
    GraphWindowInspectorView inspector;
    public Action OnGraphChanged;

    readonly Dictionary<string, NodeView> nodeViewsById = new();
    string activeNodeId;

    public TestGraphView(FSMGraphSo graphSo, GraphWindowInspectorView graphWindowInspectorView)
    {
        this.graphSo = graphSo;
        inspector = graphWindowInspectorView;
        //this.StretchToParentSize();
        //ContentZoomer.DefaultMinScale => 0.25f
        //Max는 1f
        SetupZoom(ContentZoomer.DefaultMinScale, 2f);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());

        graphViewChanged = OnGraphViewChange;

        LoadGraph();
    }


    public void LoadGraph()
    {
        graphViewChanged -= OnGraphViewChange;

        DeleteElements(graphElements.ToList());
        nodeViewsById.Clear();
        activeNodeId = null;

        LoadData(graphSo);

        graphViewChanged = OnGraphViewChange;
    }

    public void LoadData(FSMGraphSo graphSo)
    {
        this.graphSo.EnsureEntryNode();

        foreach (NodeData node in graphSo.nodes)
        {
            NodeView view = new NodeView(node, graphSo);
            nodeViewsById[node.id] = view;

            AddElement(view);
        }

        foreach (EdgeData edgeData in graphSo.edges)
        {
            if (!nodeViewsById.TryGetValue(edgeData.outputNodeId, out NodeView outputNode)) continue;
            if (!nodeViewsById.TryGetValue(edgeData.inputNodeId, out NodeView inputNode)) continue;

            PortType outputPortType = ResolveOutputPortType(edgeData);
            PortType inputPortType = ResolveInputPortType(edgeData);

            if (!outputNode.TryGetPort(outputPortType, out Port outPort)) continue;
            if (!inputNode.TryGetPort(inputPortType, out Port inPort)) continue;

            Edge edge = outPort.ConnectTo(inPort);
            edge.userData = edgeData;
            AddElement(edge);
        }

    }

    GraphViewChange OnGraphViewChange(GraphViewChange change)
    {
        
        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate)
            {
                NodeView output = edge.output.node as NodeView;
                NodeView input = edge.input.node as NodeView;
                if (output == null || input == null) continue;

                PortType outputPortType = edge.output.userData is PortType outputType
                    ? outputType
                    : NodeView.PortTypeFromName(edge.output.portName);
                PortType inputPortType = edge.input.userData is PortType inputType
                    ? inputType
                    : NodeView.PortTypeFromName(edge.input.portName);

                EdgeData edgeData = new EdgeData
                {
                    outputNodeId = output.NodeId,
                    inputNodeId = input.NodeId,
                    outputPortType = outputPortType,
                    inputPortType = inputPortType,
                    outPortName = edge.output.portName
                };
                graphSo.edges.Add(edgeData);
                edge.userData = edgeData;
            }
        }

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    NodeView output = edge.output.node as NodeView;
                    NodeView input = edge.input.node as NodeView;
                    if (output == null || input == null) continue;

                    //이거 먼지 알아야함
                    graphSo.edges.RemoveAll(e =>
                        e.outputNodeId == output.NodeId && e.inputNodeId == input.NodeId);
                }
                else if (element is NodeView nodeView)
                {
                    //엔트리 삭제 방지 만들어야함

                    graphSo.nodes.RemoveAll(n => n.id == nodeView.NodeId);
                    graphSo.edges.RemoveAll(e =>
                        e.outputNodeId == nodeView.NodeId || e.inputNodeId == nodeView.NodeId);
                }
            }
        }

        if (change.movedElements != null)
        {
            //언두인데 좀 나중에 하자 일단 필요없잖아
            //Undo.RecordObject(graphSo, "Change Node");
            foreach (GraphElement element in change.movedElements)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.SyncPositionToData();
                }
            }
        }

        // 에디터야 저장해줘
        EditorUtility.SetDirty(graphSo);
        OnGraphChanged?.Invoke();
        return change;
    }

    public void BindSelection()
    {
        RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.button == (int)UnityEngine.UIElements.MouseButton.RightMouse)
                return;
            RefreshInspectorSelection();
        });
        RegisterCallback<KeyUpEvent>(_ => RefreshInspectorSelection());
    }

    void RefreshInspectorSelection()
    {
        //UI ToolKit에서 사용하는 API 코루틴마냥 해당 UI에 종속되는듯?
        schedule.Execute(() =>
        {
            if(selection.Count > 1 || selection.Count == 0)
            {
                inspector.Clear();
            }
            else if (selection[0] is NodeView node)
                inspector.BindNode(node.data, graphSo);
            else if (selection[0] is Edge edge && edge.userData is EdgeData edgeData)
                inspector.BindEdge(edgeData, graphSo);
            else
                inspector.Clear();
        }).ExecuteLater(0); //한프레임 후 실행하기
    }

    public void CreateNode(Vector2 pos)
    {
        var entry = new NodeData
        {
            title = "New_Node",
            nodeType = NodeType.Action,
            position = pos
        };
        GraphElement node = new NodeView(entry, graphSo);
        graphSo.nodes.Add(entry);
        AddElement(node);

        if (node is NodeView nodeView)
            nodeViewsById[entry.id] = nodeView;
    }

    public void CreateCompareNode(Vector2 pos)
    {
        var entry = new NodeData
        {
            title = "New_CompareNode",
            nodeType = NodeType.Transition,
            position = pos
        };

        GraphElement node = new NodeView(entry, graphSo);
        graphSo.nodes.Add(entry);
        AddElement(node);

        if (node is NodeView nodeView)
            nodeViewsById[entry.id] = nodeView;
    }

    public void CreateMonitorNode(Vector2 pos)
    {
        var entry = new NodeData
        {
            title = "New_MonitorNode",
            nodeType = NodeType.Monitor,
            position = pos
        };

        GraphElement node = new NodeView(entry, graphSo);
        graphSo.nodes.Add(entry);
        AddElement(node);

        if (node is NodeView nodeView)
            nodeViewsById[entry.id] = nodeView;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        evt.menu.AppendAction("Add Node/Action", action =>
        {
            var pos = contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition);
            CreateNode(pos);
        });

        evt.menu.AppendAction("Add Node/Compare", action =>
        {
            var pos = contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition);
            CreateCompareNode(pos);
        });

        evt.menu.AppendAction("Add Node/Monitor", action =>
        {
            var pos = contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition);
            CreateMonitorNode(pos);
        });
    }

    //부모 메서드 오버라이드 (포트 연결 관련)
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
      
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            // 같은 포트 제외
            if (startPort == port) return;
            // 같은 노드끼리 연결 불가
            if (startPort.node == port.node) return;
            // Input ↔ Output만 연결 (같은 방향끼리는 불가)
            if (startPort.direction == port.direction) return;
            // 타입 같을 때만 (둘 다 typeof(float)면 OK)
            //if (startPort.portType != port.portType) return;
            //어차피 엣지는 흐름용으로만 쓸거임

            compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }

    public void RefreshAllNodeViews()
    {
        foreach(var element in nodes)
        {
            if(element is NodeView nodeView)
            {
                nodeView.SyncTitle();
            }
        }
    }

    public void SetActiveNode(string nodeId)
    {
        if (!string.IsNullOrEmpty(activeNodeId) &&
            nodeViewsById.TryGetValue(activeNodeId, out NodeView previousNode))
        {
            previousNode.SetRuntimeHighlight(false);
        }

        activeNodeId = nodeId;

        if (string.IsNullOrEmpty(nodeId))
            return;

        if (nodeViewsById.TryGetValue(nodeId, out NodeView nodeView))
            nodeView.SetRuntimeHighlight(true);
    }

    public void ClearActiveNode()
    {
        SetActiveNode(null);
    }

    static PortType ResolveOutputPortType(EdgeData edgeData)
    {
        if (!string.IsNullOrEmpty(edgeData.outPortName))
            return NodeView.PortTypeFromName(edgeData.outPortName);

        return edgeData.outputPortType;
    }

    static PortType ResolveInputPortType(EdgeData edgeData)
    {
        return edgeData.inputPortType;
    }
}
