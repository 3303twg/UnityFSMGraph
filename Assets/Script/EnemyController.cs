using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public FSMGraphSo graphSo; //네이밍 꼬라지

    [SerializeField]
    public BaseStat enemyStat;

    [SerializeField]
    private StateMachine stateMachine;
    FSMGraphRuntime graphRuntime;
    public IFSMNavigator Navigator => graphRuntime;

    public Dictionary<BlackboardKey, object> blackboard = new Dictionary<BlackboardKey, object>();

    public void Init()
    {
        // 여기에 데이터 기반으로 스텟 세팅하는게 있다치고
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

    private void Awake()
    {
        Init();
        stateMachine = new StateMachine();
        graphRuntime = new FSMGraphRuntime(graphSo, this, stateMachine);
        graphRuntime.Init();
    }
    private void Start()
    {
    }

    private void Update()
    {
        stateMachine?.Update();   
    }


    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        enemyStat.hp -= 1f;
        blackboard[BlackboardKey.CurHp] = enemyStat.hp;
    }
}
