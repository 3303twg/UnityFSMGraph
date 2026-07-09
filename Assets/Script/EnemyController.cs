using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public FSMGraphSo graphSo; //네이밍 꼬라지
    StateMachine stateMachine;

    public Dictionary<string, BaseState> stateDic = new Dictionary<string, BaseState>();
    private void Awake()
    {
        stateMachine = new StateMachine();

        foreach(var node in graphSo.nodes)
        {
            if (node.stateSo != null)
            {
                stateDic.Add(node.id, node.stateSo.CreateState(this, stateMachine));
            }
        }
    }
    private void Start()
    {
        stateMachine.ChangeState(stateDic["idle"]);
    }

    private void Update()
    {
        stateMachine?.Update();   
    }
}
