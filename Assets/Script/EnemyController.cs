using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    public FSMGraphSo graphSo;

    [SerializeField]
    public BaseStat enemyStat;

    [SerializeField]
    private StateMachine stateMachine;

    [SerializeField]
    FSMGraphRuntime graphRuntime;

    [SerializeReference]
    BaseState runtimeDebugState;

    public IFSMNavigator Navigator => graphRuntime;
    public FSMGraphRuntime GraphRuntime => graphRuntime;
    public BaseState RuntimeDebugState
    {
        get => runtimeDebugState;
        set => runtimeDebugState = value;
    }


    public void Init()
    {
        enemyStat.hp = enemyStat.maxHp;
        graphRuntime.blackboard.Set("Hp", enemyStat.hp);
    }


    private void Awake()
    {
        stateMachine = new StateMachine();
        graphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        graphRuntime.Init();
        Init();
    }

    private void Update()
    {
        stateMachine?.Update();
        foreach(var monitor in graphRuntime.monitorList)
        {
            monitor?.Update();
        }
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        enemyStat.hp -= 1f;
        graphRuntime.blackboard.Set("Hp", enemyStat.hp);
    }
}
