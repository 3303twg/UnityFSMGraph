using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphWindowInspectorView : VisualElement
{
    readonly VisualElement root;
    NodeData boundNode;
    EdgeData boundEdge;
    FSMGraphSo graphDataSo;

    public Action OnNodeDataChanged;
    public GraphWindowInspectorView()
    {
        //바꿔도 댈듯?
        style.minWidth = 280;
        style.paddingLeft = 8;
        style.paddingRight = 8;
        style.paddingTop = 8;
        style.backgroundColor = new Color(0.18f, 0.18f, 0.2f);
        //style.backgroundColor = new Color(255, 255, 255);

        root = new VisualElement();
        Add(root);
        Clear();
    }

    public void Clear()
    {
        boundNode = null;
        boundEdge = null;
        //base.Clear();
        root.Clear();
        root.Add(new Label("인스펙터 리셋 완료"));
    }

    public void BindNode(NodeData nodeData, FSMGraphSo graph)
    {
        boundNode = nodeData;
        boundEdge = null;
        graphDataSo = graph;
        RebuildNodeInspector();
    }
    public void BindEdge(EdgeData edgeData, FSMGraphSo graph)
    {
        boundNode = null;
        boundEdge = edgeData;
        graphDataSo = graph;
        RebuildEdgeInspector();
    }
    public void RebuildNodeInspector()
    {
        root.Clear();

        if (boundNode == null) return;

        root.Add(MakeHeader("Node:" + boundNode.nodeType.ToString()));

        TextField titleField = new TextField("Title") { value = boundNode.title };
        titleField.RegisterValueChangedCallback(evt =>
        {
            boundNode.title = evt.newValue;
            MarkDirty();
        });
        root.Add(titleField);

        BuildNodeTypeFields(boundNode);
    }

    void BuildNodeTypeFields(NodeData node)
    {
        switch (node.nodeType)
        {
            
            case NodeType.Entry:
                root.Add(new Label("엔트리 노드"));
                break;

            case NodeType.Action:
                ObjectField objectField = new ObjectField("PatternSo")
                {
                    objectType = typeof(Test),
                    value = new Test()
                };
                objectField.RegisterValueChangedCallback(_ =>
                {
                    //대충 데이터 기반으로 갱신인데 일단 뭐 나중에 ㄱㄱ
                    MarkDirty();
                });
                root.Add(objectField);
                break;

        }
    }

    public void RebuildEdgeInspector()
    {
        root.Clear();

        if (boundEdge == null) return;
    }


    Label MakeHeader(string text)
    {
        var label = new Label(text);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.marginBottom = 6;
        return label;
    }
    void MarkDirty()
    {
        if (graphDataSo != null)
            EditorUtility.SetDirty(graphDataSo);
        OnNodeDataChanged?.Invoke();
    }
}
