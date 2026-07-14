using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;

public class FSMGraphRuntime : IFSMNavigator
{
    readonly FSMGraphSo graph;
    readonly EnemyController enemyController;
    readonly StateMachine stateMachine;

    //readonly Dictionary<string, BaseState> stateDic = new Dictionary<string, BaseState>();

    readonly Dictionary<string, BaseState> statesByNodeId = new();
    readonly Dictionary<(string nodeId, PortType port), string> nextNodeByPort = new();
    readonly Dictionary<string, List<EdgeData>> outEdges = new();

    public string CurrentNodeId { get; set; }
    public FSMGraphSo Graph => graph;

    public List<BaseState> monitorList = new List<BaseState>();
    public FSMGraphRuntime(FSMGraphSo graph, EnemyController enemyController, StateMachine stateMachine)
    {
        this.graph = graph;
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }

    public void Init()
    {
        //모든 노드 상태 캐싱
        /*
        foreach (var node in graph.nodes)
        {
            if (node.stateSo != null)
            {
                BaseState nodeState = node.stateSo.CreateState(enemyController, stateMachine);
                stateDic[node.id] = nodeState;
            }
        }
        */
        BuildCache();
        CurrentNodeId = "entryNode";
        GoToPort(PortType.Output);
        //stateMachine.ChangeState(statesByNodeId[CurrentNodeId]);
        //FSMGraphRuntimeDebugger.SetActiveNode(CurrentNodeId);
    }
    void BuildCache()
    {

        foreach (var node in graph.nodes)
        {
            //일단 모든 노드 캐싱하는게?
            //if (node.nodeType != NodeType.Action && node.nodeType != NodeType.Transition) continue;

            if (node.stateSo == null) continue;
            
            statesByNodeId[node.id] = node.stateSo.CreateState(enemyController, stateMachine);
            if (node.nodeType == NodeType.Monitor)
            {
                monitorList.Add(statesByNodeId[node.id]);
                if(statesByNodeId[node.id] is MonitorState monitorState)
                {
                    monitorState.nodeId = node.id;
                }
            }
            Debug.Log(node.id);
        }

        foreach (var edge in graph.edges)
        {
            PortType port = ToPortType(edge);

            nextNodeByPort[(edge.outputNodeId, port)] = edge.inputNodeId;
        }

    }

    PortType ToPortType(EdgeData edge)
    {
        // enum 바꿨으면 edge.outputPortType 그대로
        return edge.outPortName switch
        {
            "True" => PortType.True,
            "False" => PortType.False,
            "Out" => PortType.Output,
            _ => PortType.Output
        };
    }

    public void GoToPort(PortType portType)
    {
        if (!nextNodeByPort.TryGetValue((CurrentNodeId, portType), out string nextNodeId))
        {
            Debug.LogWarning($"엣지 없음: {CurrentNodeId} / {portType}");
            return;
        }
        if (!statesByNodeId.TryGetValue(nextNodeId, out BaseState nextState))
        {
            Debug.LogWarning($"State 없음: {nextNodeId}");
            return;
        }
        CurrentNodeId = nextNodeId;
        stateMachine.ChangeState(nextState);
        FSMGraphRuntimeDebugger.SetActiveNode(CurrentNodeId);
    }

    public void GoToPortFrom(string nodeId, PortType portType)
    {
        if (!nextNodeByPort.TryGetValue((nodeId, portType), out string nextNodeId))
        {
            Debug.LogWarning($"엣지 없음: {nodeId} / {portType}");
            return;
        }
        if (!statesByNodeId.TryGetValue(nextNodeId, out BaseState nextState))
        {
            Debug.LogWarning($"State 없음: {nextNodeId}");
            return;
        }
        CurrentNodeId = nextNodeId;
        stateMachine.ChangeState(nextState);
        FSMGraphRuntimeDebugger.SetActiveNode(CurrentNodeId);
    }

    public void GoToNextNode() => GoToPort(PortType.Output);
    public void GoToTrueNode() => GoToPort(PortType.True);
    public void GoToFalseNode() => GoToPort(PortType.False);

    public bool TryGetState(string nodeId, out BaseState state)
        => statesByNodeId.TryGetValue(nodeId, out state);
}
