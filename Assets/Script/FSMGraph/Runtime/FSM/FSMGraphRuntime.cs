using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class FSMGraphRuntime : IFSMNavigator
{
    readonly FSMGraphSo graph;
    readonly IFSMAgent agent;
    readonly StateMachine stateMachine;

    readonly Dictionary<string, BaseState> statesByNodeId = new Dictionary<string, BaseState>();
    readonly Dictionary<(string nodeId, PortType port), string> nextNodeByPort = new Dictionary<(string nodeId, PortType port), string>();
    readonly Dictionary<string, List<EdgeData>> outEdges = new Dictionary<string, List<EdgeData>>();

    [SerializeField]
    public Blackboard blackboard = new Blackboard();

    public string CurrentNodeId { get; set; }
    public FSMGraphSo Graph => graph;

    public List<BaseState> monitorList = new List<BaseState>();
    public FSMGraphRuntime(FSMGraphSo graph, IFSMAgent agent, StateMachine stateMachine)
    {
        this.graph = graph;
        this.agent = agent;
        this.stateMachine = stateMachine;
    }

    public void Init()
    {
        BuildCache();
        InitBlackboard();
        blackboard.OnChanged += OnBlackboardChanged;
        string entryId = string.IsNullOrEmpty(graph.entryNodeId) ? "entryNode" : graph.entryNodeId;
        RequestTransition(entryId);
        Flush();
    }

    void InitBlackboard()
    {
        blackboard = graph.blackboard.Clone();
    }
    void OnBlackboardChanged()
    {
        MonitorState winner = null;
        int highPriority = int.MinValue;
        for (int i = 0; i < monitorList.Count; i++)
        {
            if (monitorList[i] is not MonitorState monitor || !monitor.Poll())
                continue;

            if (monitor.priority > highPriority)
            {
                highPriority = monitor.priority;
                winner = monitor;
            }
        }
        if (winner == null)
            return;

        if (!nextNodeByPort.TryGetValue((winner.nodeId, PortType.Output), out string nextNodeId))
        {
            Debug.LogWarning($"엣지 없음: {winner.nodeId} / {PortType.Output}");
            return;
        }

        if (!RequestMonitor(nextNodeId, winner.priority))
            return;

        winner.Consume();
        if (!inCallback && !flushing)
            Flush();
    }

    void BuildCache()
    {
        foreach (var node in graph.nodes)
        {
            if (node.stateSo == null) continue;

            statesByNodeId[node.id] = node.stateSo.CreateState(agent);
            if (node.nodeType == NodeType.Monitor)
            {
                monitorList.Add(statesByNodeId[node.id]);
                if (statesByNodeId[node.id] is MonitorState monitorState)
                {
                    monitorState.nodeId = node.id;
                }
            }
        }

        foreach (var edge in graph.edges)
        {
            PortType port = ToPortType(edge);
            nextNodeByPort[(edge.outputNodeId, port)] = edge.inputNodeId;
        }
    }

    PortType ToPortType(EdgeData edge)
    {
        return edge.outPortName switch
        {
            "True" => PortType.True,
            "False" => PortType.False,
            "Out" => PortType.Output,
            _ => PortType.Output
        };
    }

    public void Tick()
    {
        inCallback = true;
        try
        {
            stateMachine.Update();
        }
        finally
        {
            inCallback = false;
        }
        Flush();
    }

    public void GoToNode(string nodeId)
    {
        RequestTransition(nodeId);
        if (!inCallback && !flushing)
            Flush();
    }

    const int MaxChain = 32;
    string pendingNodeId;
    bool pendingIsMonitor;
    int pendingMonitorPriority;
    bool inCallback;
    bool flushing;
    readonly HashSet<string> resolveSeen = new();

    void RequestTransition(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("GoToNode: nodeId 비어있음");
            return;
        }
        if (pendingIsMonitor)
            return;

        pendingNodeId = nodeId;
        pendingIsMonitor = false;
    }

    bool RequestMonitor(string nodeId, int priority)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("GoToNode: nodeId 비어있음");
            return false;
        }
        if (pendingIsMonitor && priority <= pendingMonitorPriority)
            return false;

        pendingNodeId = nodeId;
        pendingIsMonitor = true;
        pendingMonitorPriority = priority;
        return true;
    }

    public void Flush()
    {
        if (flushing || inCallback) return;

        flushing = true;
        try
        {
            int depth = 0;
            while (!string.IsNullOrEmpty(pendingNodeId) && depth++ < MaxChain)
            {
                string requested = pendingNodeId;
                pendingNodeId = null;
                pendingIsMonitor = false;
                if (!TryResolveStaying(requested, out string stayId))
                    break;
                if (!statesByNodeId.TryGetValue(stayId, out BaseState nextState))
                {
                    Debug.LogWarning($"State 없음: {stayId}");
                    break;
                }

                CurrentNodeId = stayId;
                
                inCallback = true;
                try
                {
                    BaseState state = statesByNodeId[stayId];
                    stateMachine.ChangeState(state);
                }
                finally
                {
                    inCallback = false;
                }
                FSMGraphRuntimeDebugger.SetActiveNode(agent as UnityEngine.Object, CurrentNodeId);
            }

            if (!string.IsNullOrEmpty(pendingNodeId))
                Debug.LogError($"전이 깊이 초과: {pendingNodeId}");
        }
        finally
        {
            flushing = false;
        }
    }

    bool TryResolveStaying(string nodeId, out string stayId)
    {
        stayId = null;
        resolveSeen.Clear();
        int depth = 0;
        while (!string.IsNullOrEmpty(nodeId) && depth++ < MaxChain)
        {
            if (!resolveSeen.Add(nodeId))
            {
                Debug.LogError($"전이 순환: {nodeId}");
                return false;
            }

            NodeData node = graph.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"노드 없음: {nodeId}");
                return false;
            }

            switch (node.nodeType)
            {
                case NodeType.Reference:
                    if (string.IsNullOrEmpty(node.referenceTargetId))
                    {
                        Debug.LogWarning($"Reference 타겟 없음: {node.title} ({nodeId})");
                        return false;
                    }
                    nodeId = node.referenceTargetId;
                    continue;
                case NodeType.Entry:
                case NodeType.Monitor:
                    if (!TryNext(nodeId, PortType.Output, out nodeId))
                        return false;
                    continue;
                case NodeType.Transition:
                    if (!statesByNodeId.TryGetValue(nodeId, out BaseState state) || state is not CompareState compare)
                    {
                        Debug.LogWarning($"Compare 없음: {nodeId}");
                        return false;
                    }
                    PortType port = compare.Evaluate() ? PortType.True : PortType.False;
                    if (!TryNext(nodeId, port, out nodeId))
                        return false;
                    continue;
                case NodeType.Action:
                    stayId = nodeId;
                    return true;
            }
        }

        Debug.LogError($"전이 깊이 초과: {nodeId}");
        return false;
    }

    bool TryNext(string nodeId, PortType portType, out string nextNodeId)
    {
        if (nextNodeByPort.TryGetValue((nodeId, portType), out nextNodeId))
            return true;

        Debug.LogWarning($"엣지 없음: {nodeId} / {portType}");
        return false;
    }

    public void GoToPort(PortType portType)
    {
        GoToPortFrom(CurrentNodeId, portType);
    }

    public void GoToPortFrom(string nodeId, PortType portType)
    {
        if (!TryNext(nodeId, portType, out string nextNodeId))
            return;

        RequestTransition(nextNodeId);
        if (!inCallback && !flushing)
            Flush();
    }

    public void GoToNextNode() => GoToPort(PortType.Output);
    public void GoToTrueNode() => GoToPort(PortType.True);
    public void GoToFalseNode() => GoToPort(PortType.False);

    public bool TryGetState(string nodeId, out BaseState state)
        => statesByNodeId.TryGetValue(nodeId, out state);

    public void Dispose()
    {
        blackboard.OnChanged -= OnBlackboardChanged;
    }
}
