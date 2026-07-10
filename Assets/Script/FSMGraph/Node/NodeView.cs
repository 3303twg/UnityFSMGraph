using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : Node
{
    FSMGraphSo graphSo;
    public NodeData data;

    static readonly Color HighlightBorderColor = new Color32(120, 220, 100, 255);
    const float HighlightBorderWidth = 3f;

    bool isHighlighted;


    public string NodeId
    { 
        get
        {
            return data.id;
        }
    }
    //포트 데이터는 있어야지
    //포트 세팅이 없네 만들어야함
    public Dictionary<PortType, Port> portDic = new Dictionary<PortType, Port>();

    public bool TryGetPort(PortType portType, out Port port)
        => portDic.TryGetValue(portType, out port);

    public static PortType PortTypeFromName(string portName)
    {
        return portName switch
        {
            "True" => PortType.True,
            "False" => PortType.False,
            "Out" => PortType.Output,
            "Input" => PortType.Input,
            _ => PortType.Output
        };
    }

    Port AddPort(PortType portType, Direction direction, Port.Capacity capacity, string portName, VisualElement container)
    {
        var port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
        port.portName = portName;
        port.userData = portType;
        portDic[portType] = port;
        container.Add(port);
        return port;
    }


    public NodeView(NodeData data, FSMGraphSo graphSo)
    {
        this.data = data;
        this.graphSo = graphSo;
        SetPosition(new Rect(data.position, new Vector2(200, 100)));
        this.title = data.title;

        ApplyStyle();
        CreatePorts();
        //RefreshCapsule();
        //RegisterHover();
    }


    void ApplyStyle()
    {

        Color color;
        if (data.nodeType == NodeType.Entry)
        {
            color = new Color32(70, 170, 160, 255);

            titleContainer.style.backgroundColor = color;
            mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f); // 짙은 회색, 살짝 푸른기 (노드 본체)
        }

        else if(data.nodeType == NodeType.Action)
        {
            color = new Color32(185, 115, 35, 255);

            titleContainer.style.backgroundColor = color;
            mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f); // 짙은 회색, 살짝 푸른기 (노드 본체)
        }

        else if(data.nodeType == NodeType.Transition)
        {
            color = new Color32(130, 90, 175, 255);

            titleContainer.style.backgroundColor = color;
            mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f); // 짙은 회색, 살짝 푸른기 (노드 본체)
        }
        else
        {
            // 이건 이제 FSM노드마다 바꾸면 되겠고
            color = new Color(0.4f, 0.4f, 0.4f); // 중간 회색 (타이틀바)

            titleContainer.style.backgroundColor = color;
            mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f); // 짙은 회색, 살짝 푸른기 (노드 본체)

        }
    }

    void CreatePorts()
    {
        //타입마다 다르게 해줘야하고

        if(data.nodeType == NodeType.Entry)
        {
            AddPort(PortType.Output, Direction.Output, Port.Capacity.Single, "Out", outputContainer);
        }

        else if (data.nodeType == NodeType.Transition)
        {
            AddPort(PortType.Input, Direction.Input, Port.Capacity.Multi, "Input", inputContainer);
            AddPort(PortType.True, Direction.Output, Port.Capacity.Single, "True", outputContainer);
            AddPort(PortType.False, Direction.Output, Port.Capacity.Single, "False", outputContainer);
        }

        else if(data.nodeType == NodeType.Monitor)
        {
            return;
        }

        else if(data.nodeType == NodeType.Action)
        {
            AddPort(PortType.Input, Direction.Input, Port.Capacity.Multi, "Input", inputContainer);
            AddPort(PortType.Output, Direction.Output, Port.Capacity.Single, "Out", outputContainer);
        }

        else if(data.nodeType == NodeType.Reference)
        {
            AddPort(PortType.Output, Direction.Output, Port.Capacity.Single, "Out", outputContainer);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    public void SyncPositionToData()
    {
        data.position = GetPosition().position;
    }

    public void SyncTitle()
    {
        title = data.title;
    }

    public void SetRuntimeHighlight(bool on)
    {
        if (isHighlighted == on)
            return;

        isHighlighted = on;

        if (on)
        {
            mainContainer.style.borderTopWidth = HighlightBorderWidth;
            mainContainer.style.borderBottomWidth = HighlightBorderWidth;
            mainContainer.style.borderLeftWidth = HighlightBorderWidth;
            mainContainer.style.borderRightWidth = HighlightBorderWidth;
            mainContainer.style.borderTopColor = HighlightBorderColor;
            mainContainer.style.borderBottomColor = HighlightBorderColor;
            mainContainer.style.borderLeftColor = HighlightBorderColor;
            mainContainer.style.borderRightColor = HighlightBorderColor;
        }
        else
        {
            mainContainer.style.borderTopWidth = 0;
            mainContainer.style.borderBottomWidth = 0;
            mainContainer.style.borderLeftWidth = 0;
            mainContainer.style.borderRightWidth = 0;
        }
    }
}
