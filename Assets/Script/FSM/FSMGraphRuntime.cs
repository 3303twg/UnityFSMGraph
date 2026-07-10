using System.Collections.Generic;
using UnityEngine;

public class FSMGraphRuntime : IFSMNavigator
{
    readonly FSMGraphSo graph;
    readonly EnemyController enemyController;
    readonly StateMachine stateMachine;

    //readonly Dictionary<string, BaseState> stateDic = new Dictionary<string, BaseState>();

    readonly Dictionary<string, BaseState> statesByNodeId = new();
    readonly Dictionary<string, List<EdgeData>> outEdges = new();

    public string CurrentNodeId { get; set; }

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
        CurrentNodeId = "54969657-bfc2-460f-ba05-bae0df22c352";
        stateMachine.InitState(statesByNodeId[CurrentNodeId]);
        FSMGraphRuntimeDebugger.SetActiveNode(CurrentNodeId);
    }
    void BuildCache()
    {

        foreach (var node in graph.nodes)
        {
            if (node.nodeType != NodeType.Action) continue;
            if (node.stateSo == null) continue;
            statesByNodeId[node.id] = node.stateSo.CreateState(enemyController, stateMachine);
            Debug.Log(node.id);
        }

        foreach (var edge in graph.edges)
        {
            if (!outEdges.TryGetValue(edge.outputNodeId, out var list))
            {
                list = new List<EdgeData>();
                outEdges[edge.outputNodeId] = list;
            }
            list.Add(edge);
        }

    }
    public void GoToNextNode()
    {

        string nextNodeId = outEdges[CurrentNodeId][0].inputNodeId;
        stateMachine.ChangeState(statesByNodeId[nextNodeId]);
        CurrentNodeId = nextNodeId;
        FSMGraphRuntimeDebugger.SetActiveNode(CurrentNodeId);
    }
}
