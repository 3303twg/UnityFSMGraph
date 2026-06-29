using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeView : Node
{
    NodeData data;
    public NodeView(NodeData data)
    {
        this.data = data;
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
        Port input = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Single,
            typeof(float)
        );
        input.portName = "Input";
        inputContainer.Add(input);

        Port output = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Multi,
            typeof(float)
        );
        output.portName = "Output";
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();
    }
}
