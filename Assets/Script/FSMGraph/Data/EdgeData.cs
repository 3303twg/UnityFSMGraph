using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class EdgeData
{
    public string id;
    public string outputNodeId;
    public string inputNodeId;
    public PortType outputPortType;
    public PortType inputPortType = PortType.Input;
    public string outPortName;
}
