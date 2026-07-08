using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeView : Node
{
    FSMGraphSo graphSo;
    public NodeData data;


    public string NodeId
    { 
        get
        {
            return data.id;
        }
    }
    //포트 데이터는 있어야지
    //포트 세팅이 없네 만들어야함
    public Port InputPort { get; private set; }
    public Port OutputPort { get; private set; }


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
        // 이건 이제 FSM노드마다 바꾸면 되겠고
        var color = new Color(0.4f, 0.4f, 0.4f); // 중간 회색 (타이틀바)

        titleContainer.style.backgroundColor = color;
        mainContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.18f); // 짙은 회색, 살짝 푸른기 (노드 본체)
    }

    void CreatePorts()
    {
        //타입마다 다르게 해줘야하고

        if(data.nodeType == NodeType.Entry)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);
        }

        else if (data.nodeType == NodeType.Transition)
        {

        }

        else if(data.nodeType == NodeType.Monitor)
        {
            return;
        }

        else if(data.nodeType == NodeType.Action)
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);
        }

        else if(data.nodeType == NodeType.Reference)
        {
            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);
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
}
