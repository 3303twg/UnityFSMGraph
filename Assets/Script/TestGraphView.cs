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
    public TestGraphView()
    {
        this.StretchToParentSize();
        SetupZoom(0.25f, 2f);

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
}

public class NodeData
{
    public string title = "State";
    public Vector2 position;
}

public class NodeView : Node
{
    NodeData data;
    public NodeView(NodeData data)
    {
        this.data = data;
        SetPosition(new Rect(
        data.position,
        new Vector2(150, 80)
    ));

        ApplyStyle();
        CreatePorts();
        //RefreshCapsule();
        //RegisterHover();
    }


    void ApplyStyle()
    {

        var color = new Color(0.4f, 0.4f, 0.4f);

        titleContainer.style.backgroundColor = color;
        mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f);
    }

    void CreatePorts()
    {
        Port input = InstantiatePort(
            Orientation.Vertical,
            Direction.Input,
            Port.Capacity.Single,
            typeof(float)
        );

        input.portName = "Input";


        Port output = InstantiatePort(
            Orientation.Vertical,
            Direction.Output,
            Port.Capacity.Multi,
            typeof(float)
        );

        output.portName = "Output";


        // 위 포트 영역
        VisualElement topPort = new VisualElement();
        topPort.style.flexDirection = FlexDirection.Column;
        topPort.style.alignItems = Align.Center;

        topPort.Add(input);


        // 아래 포트 영역
        VisualElement bottomPort = new VisualElement();
        bottomPort.style.flexDirection = FlexDirection.Column;
        bottomPort.style.alignItems = Align.Center;

        bottomPort.Add(output);


        extensionContainer.Insert(0, topPort);
        extensionContainer.Add(bottomPort);


        RefreshPorts();
        RefreshExpandedState();
    }
}
