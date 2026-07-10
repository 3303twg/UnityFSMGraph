using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
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
                
                break;
            case NodeType.Transition:
                
                break;
        }
        ObjectField objectField = new ObjectField("StateSo")
        {
            objectType = typeof(BaseStateSoAsset),
            value = node.stateSo,
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            node.stateSo = evt.newValue as BaseStateSoAsset;
            MarkDirty();
            RebuildNodeInspector();
        });
        root.Add(objectField);

        if (node.stateSo != null)
        {


            var so = new SerializedObject(node.stateSo);
            var inspector = new InspectorElement(so);
            root.Add(MakeSection("So인스펙터", inspector));
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



    //예쁜 박스만들기
    VisualElement MakeSection(string title, VisualElement content)
    {
        var section = new VisualElement();
        section.style.marginTop = 10;
        section.style.paddingTop = 8;
        section.style.paddingBottom = 8;
        section.style.paddingLeft = 6;
        section.style.paddingRight = 6;
        section.style.backgroundColor = new Color(0.14f, 0.14f, 0.16f);
        section.style.borderTopWidth = 1;
        section.style.borderBottomWidth = 1;
        section.style.borderLeftWidth = 1;
        section.style.borderRightWidth = 1;
        section.style.borderTopColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderBottomColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderLeftColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderRightColor = new Color(0.08f, 0.08f, 0.08f);
        section.style.borderTopLeftRadius = 4;
        section.style.borderTopRightRadius = 4;
        section.style.borderBottomLeftRadius = 4;
        section.style.borderBottomRightRadius = 4;

        var header = MakeHeader(title);
        header.style.marginBottom = 8;
        section.Add(header);
        section.Add(content);
        return section;
    }
}
