using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "FSM/Graph")]
public class FSMGraphSo : ScriptableObject
{
    public string entryNodeId;
    //흠 이거 id 두개로 할 필요있나?
    public string id;
    [SerializeField]
    public List<NodeData> nodes = new List<NodeData>();
    [SerializeField]
    public List<EdgeData> edges = new List<EdgeData>();

    [SerializeField]
    public Blackboard blackboard;// = new Blackboard();


    //이건 좀 수정하든가 해야겠다
    public NodeData GetNode(string id) => nodes.FirstOrDefault(n => n.id == id);
    public NodeData EntryNode => string.IsNullOrEmpty(entryNodeId) ? null : GetNode(entryNodeId);

    public void EnsureEntryNode()
    {
        if (EntryNode != null) return;
        NodeData entry = new NodeData
        {
            id = "entryNode",
            nodeType = NodeType.Entry,
            title = "Entry",
            position = new Vector2(80, 200)
        };
        nodes.Add(entry);
        entryNodeId = entry.id;
    }
    [ContextMenu("test")]
    public void Tesst()
    {
    }
    
}


