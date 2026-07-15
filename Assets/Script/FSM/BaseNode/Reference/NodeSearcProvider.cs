using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class NodeSearcProvider : ScriptableObject, ISearchWindowProvider
{
    FSMGraphSo graph;
    string excludeNodeId;
    Action<NodeData> onSelect;

    public void Init(FSMGraphSo graph, string excludeNodeId, Action<NodeData> onSelect)
    {
        this.graph = graph;
        this.excludeNodeId = excludeNodeId;
        this.onSelect = onSelect;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> list = new List<SearchTreeEntry>();
        list.Add(new SearchTreeGroupEntry(new GUIContent("Select Target Node"), 0));

        foreach(var node in graph.nodes)
        {
            if (node.id == excludeNodeId) continue;
            if (node.nodeType == NodeType.Entry) continue;
            if (node.nodeType == NodeType.Reference) continue;

            list.Add(new SearchTreeEntry(new GUIContent(node.title + "(" + node.nodeType + ")"))
            {
                level = 1,
                userData = node
            });
        }
        return list;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (entry.userData is NodeData node)
        {
            onSelect?.Invoke(node);
            return true;
        }
        return false;
    }
}

