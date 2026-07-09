using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NodeData
{
    public string id = Guid.NewGuid().ToString();
    public NodeType nodeType;
    public string title = "State";
    public Vector2 position;
    public BaseStateSoAsset stateSo;
}
