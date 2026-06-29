using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class TestGraphView : GraphView
{
    FSMGraphSo graph;

    public TestGraphView(FSMGraphSo graph)
    {
        this.graph = graph;
        this.StretchToParentSize();
        //ContentZoomer.DefaultMinScale => 0.25f
        //Max는 1f
        SetupZoom(ContentZoomer.DefaultMinScale, 2f);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ClickSelector());


        LoadGraph();
    }


    public void LoadGraph()
    {
        DeleteElements(graphElements.ToList());

        EnsureEntryNode();

        //var nodeViews = new Dictionary<string, FsmNodeView>();
        //foreach (var node in asset.nodes)
        //{
        //    var view = new FsmNodeView(node, asset);
        //    BindNodeHover(view);
        //    AddElement(view);
        //    nodeViews[node.id] = view;
        //}

        //foreach (var edgeData in asset.edges)
        //{
        //    if (!nodeViews.TryGetValue(edgeData.outputNodeId, out var outputNode)) continue;
        //    if (!nodeViews.TryGetValue(edgeData.inputNodeId, out var inputNode)) continue;

        //    var outPort = outputNode.GetPortForEdge(edgeData.port);
        //    if (outPort == null || inputNode.InputPort == null) continue;

        //    var edge = outPort.ConnectTo(inputNode.InputPort);
        //    edge.userData = edgeData;
        //    AddElement(edge);
        //}

    }

    public void EnsureEntryNode()
    {
        var entry = new NodeData
        {
            title = "Entry",
            position = new Vector2(80, 200)
        };
        //nodes.Add(entry);
        GraphElement view = new NodeView(entry);
        AddElement(view);
        //entryNodeId = entry.id;
    }

    public void CreateNode(Vector2 pos)
    {
        var entry = new NodeData
        {
            title = "New_Node",
            position = pos
        };
        GraphElement node = new NodeView(entry);
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
