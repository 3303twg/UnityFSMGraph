using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FSMGraphSo))]
public class FSMGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //왜 주석했지?
        base.OnInspectorGUI();
        if(GUILayout.Button("Open Graph"))
        {
            GraphWindow.Open((FSMGraphSo)target);
        }
    }
}
