using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TypeSearchProvider : ScriptableObject, ISearchWindowProvider
{
    FSMGraphSo graph;
    string excludeNodeId;
    Action<Type> onSelect;

    public static readonly Dictionary<string, Type> BlackboardTypes = new Dictionary<string, Type>()
{
   // { "Int", typeof(int) },
    { "Float", typeof(float) },
    { "String", typeof(string) },
    { "Bool", typeof(bool) },

   // { "Vector2", typeof(Vector2) },
   // { "Vector3", typeof(Vector3) },
   // { "Color", typeof(Color) },

    { "GameObject", typeof(GameObject) },
    //{ "Transform", typeof(Transform) },

  //  { "Int List", typeof(List<int>) },
  //  { "Float List", typeof(List<float>) },
  //  { "String List", typeof(List<string>) },
};


    public void Init(Action<Type> onSelect)
    {
        this.onSelect = onSelect;
    }


    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> list = new List<SearchTreeEntry>();
        list.Add(new SearchTreeGroupEntry(new GUIContent("Select Type"), 0));

        foreach (var pair in BlackboardTypes)
        {
            list.Add(new SearchTreeEntry(new GUIContent(pair.Key.ToString()))
            {
                level = 1,
                userData = pair.Value
            });

        }
        return list;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (entry.userData is Type type)
        {
            onSelect?.Invoke(type);
            return true;
        }
        return false;
    }
}
