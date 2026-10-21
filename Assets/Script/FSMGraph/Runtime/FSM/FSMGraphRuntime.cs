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
        CurrentNodeId = "entryNode";
        GoToPort(PortType.Output);
    }

    void InitBlackboard()
    {
        blackboard = graph.blackboard.Clone();
    }
    void OnBlackboardChanged()
    {
        for (int i = 0; i < monitorList.Count; i++)
        {
            if (monitorList[i] is MonitorState monitor)
                monitor.Compare();
        }
    }

    void BuildCache()
    {
        foreach (var node in graph.nodes)
        {
            if (node.stateSo == null) continue;

            statesByNodeId[node.id] = node.stateSo.CreateState(agent, stateMachine);
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

    public void GoToNode(string nodeId)
    {
        GoToNodeInternal(nodeId);
    }


    //하나의 호출 스택중 전이순환 거르는용도
    const int MaxChain = 32;
    int chainDepth;
    readonly List<string> chain = new();
    readonly HashSet<string> chainSet = new();

    void GoToNodeInternal(string nodeId)
    {

        if (!chainSet.Add(nodeId))
        {
            Debug.LogError($"전이 순환: {string.Join(" → ", chain)} → {nodeId}");
            return;
        }
        if (chain.Count >= MaxChain)
        {
            chainSet.Remove(nodeId);
            Debug.LogError($"전이 깊이 초과: {string.Join(" → ", chain)}");
            return;
        }
        chain.Add(nodeId);
        chainDepth++;

        try
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                Debug.LogWarning("GoToNode: nodeId 비어있음");
                return;
            }

            NodeData node = graph.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"노드 없음: {nodeId}");
                return;
            }

            // Ref는 상태가 아님 → 타겟으로 워프
            if (node.nodeType == NodeType.Reference)
            {
                if (string.IsNullOrEmpty(node.referenceTargetId))
                {
                    Debug.LogWarning($"Reference 타겟 없음: {node.title} ({nodeId})");
                    return;
                }

                GoToNodeInternal(node.referenceTargetId);
                return;
            }

            if (!statesByNodeId.TryGetValue(nodeId, out BaseState nextState))
            {
                Debug.LogWarning($"State 없음: {nodeId}");
                return;
            }

            CurrentNodeId = nodeId;
            stateMachine.ChangeState(nextState);
            FSMGraphRuntimeDebugger.SetActiveNode(agent as UnityEngine.Object, CurrentNodeId);
        }

        finally
        {
            chainDepth--;
            if (chainDepth == 0)
            {
                chain.Clear();
                chainSet.Clear();
            }
        }
    }

    public void GoToPort(PortType portType)
    {
        if (!nextNodeByPort.TryGetValue((CurrentNodeId, portType), out string nextNodeId))
        {
            Debug.LogWarning($"엣지 없음: {CurrentNodeId} / {portType}");
            return;
        }

        GoToNode(nextNodeId);
    }

    public void GoToPortFrom(string nodeId, PortType portType)
    {
        if (!nextNodeByPort.TryGetValue((nodeId, portType), out string nextNodeId))
        {
            Debug.LogWarning($"엣지 없음: {nodeId} / {portType}");
            return;
        }

        GoToNode(nextNodeId);
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
