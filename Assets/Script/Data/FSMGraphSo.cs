using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Graph")]
public class FSMGraphSo : ScriptableObject
{
    public string entryNodeId;
    public List<NodeData> nodes = new();
    public List<EdgeData> edges = new();
}


