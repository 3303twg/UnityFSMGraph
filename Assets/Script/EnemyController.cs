using System.Collections.Generic;
using UnityEngine;

public partial class EnemyController : MonoBehaviour
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
        blackboard[BlackboardKey.DetectionDistance] = enemyStat.detectionDistance;
        blackboard[BlackboardKey.DistToPlayer] = 0f;
        blackboard[BlackboardKey.BossPhase] = 1f;
        blackboard[BlackboardKey.HpRatio] = 100f;
        MoveSpeedMul = 1f;
        DamageMul = 1f;
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
    }

    private void Update()
    {
        SyncCombatBlackboard();
        stateMachine?.Update();
        foreach (var monitor in graphRuntime.monitorList)
        {
            monitor?.Update();
        }
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        TakeDamage(20f);
    }

    public void TakeDamage(float amount)
    {
        enemyStat.hp = Mathf.Max(0f, enemyStat.hp - amount);
        blackboard[BlackboardKey.CurHp] = enemyStat.hp;
        var sr = GetComponent<SpriteRenderer>();
        BossVfx.HitFlash(sr, Color.white);
        BossVfx.SpawnSparkBurst(transform.position, new Color(1f, 0.5f, 0.2f), 6, 4.5f);
        BossCombatHud.Instance?.Shake(0.18f);
        Debug.Log($"[Boss] HP {enemyStat.hp}/{enemyStat.maxHp}");
    }

    void LateUpdate()
    {
        // 테스트: F로 보스에 데미지 → 페이즈 전환 확인
        if (Input.GetKeyDown(KeyCode.F))
            TakeDamage(25f);
    }
}
