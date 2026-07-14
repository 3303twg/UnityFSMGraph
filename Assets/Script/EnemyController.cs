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

    public Dictionary<BlackboardKey, object> blackboard = new Dictionary<BlackboardKey, object>();

    public void Init()
    {
        enemyStat.hp = enemyStat.maxHp;
        InitBlackboard();
    }

    void InitBlackboard()
    {
        blackboard[BlackboardKey.CurHp] = enemyStat.hp;
    }

    public object GetBlackboardValue(BlackboardKey key)
    {
        return blackboard[key];
    }

    public void SetBlackboardValue(BlackboardKey key, object value)
    {
        blackboard[key] = value;
    }

    private void Awake()
    {
        Init();
        stateMachine = new StateMachine();
        graphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        graphRuntime.Init();
        foreach (var monitor in graphRuntime.monitorList)
        {
            if (monitor is IFSMMonitor fsmMonitor)
            {
                fsmMonitor.Init();
            }
        }
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
        blackboard[BlackboardKey.CurHp] = enemyStat.hp;
    }
}
