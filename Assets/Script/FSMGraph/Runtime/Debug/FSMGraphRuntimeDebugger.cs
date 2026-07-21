using System;


//이건 에디터만 하게 하지 않을까?
public static class FSMGraphRuntimeDebugger
{
    public static string ActiveNodeId { get; private set; }

    public static event Action<string> ActiveNodeChanged;

    public static void SetActiveNode(string nodeId)
    {
        if (ActiveNodeId == nodeId)
            return;

        ActiveNodeId = nodeId;
        ActiveNodeChanged?.Invoke(nodeId);
    }

    public static void ClearActiveNode()
    {
        if (string.IsNullOrEmpty(ActiveNodeId))
            return;

        ActiveNodeId = null;
        ActiveNodeChanged?.Invoke(null);
    }
}
