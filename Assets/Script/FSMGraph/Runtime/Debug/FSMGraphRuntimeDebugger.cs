using System;
using UnityEngine;


//이건 에디터만 하게 하지 않을까?
public static class FSMGraphRuntimeDebugger
{
    public static string ActiveNodeId { get; private set; }

    public static event Action<UnityEngine.Object, string> ActiveNodeChanged;

    public static void SetActiveNode(UnityEngine.Object owner, string nodeId)
    {
        ActiveNodeId = nodeId;
        ActiveNodeChanged?.Invoke(owner, nodeId);
    }

    public static void ClearActiveNode()
    {
        if (string.IsNullOrEmpty(ActiveNodeId))
            return;

        ActiveNodeId = null;
        ActiveNodeChanged?.Invoke(null, null);
    }
}
