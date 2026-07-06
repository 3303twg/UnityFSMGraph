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
    public Action OnGraphChanged;

    public TestGraphView(FSMGraphSo graphSo)
    {
        this.graphSo = graphSo;
        this.StretchToParentSize();
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

        graphSo.EnsureEntryNode();

        LoadData(graphSo);

        graphViewChanged = OnGraphViewChange;

    }

    public void LoadData(FSMGraphSo graphSo)
    {
        //임시 딕셔너리 필요가있나? 흠..
        Dictionary<string, NodeView> nodeViews = new Dictionary<string, NodeView>();


        foreach (NodeData node in graphSo.nodes)
        {
            NodeView view = new NodeView(node, graphSo);
            nodeViews[node.id] = view;

            AddElement(view);
        }

        foreach (EdgeData edgeData in graphSo.edges)
        {
            if (!nodeViews.TryGetValue(edgeData.outputNodeId, out NodeView outputNode)) continue;
            if (!nodeViews.TryGetValue(edgeData.inputNodeId, out NodeView inputNode)) continue;

            //true false 받아오던데 필요한가?
            //var outPort = outputNode.GetPortForEdge();

            //하씨 이거좀 잘따져야겠다
            Port outPort = outputNode.OutputPort;
            //if (outPort == null || inputNode.InputPort == null) continue;

            Edge edge = outPort.ConnectTo(inputNode.InputPort);
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

                EdgeData edgeData = new EdgeData
                {
                    outputNodeId = output.NodeId,
                    inputNodeId = input.NodeId,
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



    public void CreateNode(Vector2 pos)
    {
        var entry = new NodeData
        {
            title = "New_Node",
            position = pos
        };
        GraphElement node = new NodeView(entry, graphSo);
        graphSo.nodes.Add(entry);
        AddElement(node);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        evt.menu.AppendAction("Add Node", action =>
        {
            var pos = contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition);
            CreateNode(pos);
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
}
