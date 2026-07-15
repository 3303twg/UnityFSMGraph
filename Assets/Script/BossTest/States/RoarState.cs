using System;
using UnityEngine;

[Serializable]
public class RoarState : BaseState
{
    public float duration;
    float timer;

    public RoarState(EnemyController c, StateMachine s, RoarStateSo d) : base(c, s)
    {
        duration = d.duration;
    }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("[Boss] ROAR!");
        BossCombatHud.Instance?.SetStateLabel("ROAR");
        enemyController.FacePlayer();
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.6f, 0.1f), 5f);
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.7f, 0.2f), 0.8f, 3.5f, 0.5f);
        CombatCamera.Instance?.Shake(0.4f, duration * 0.85f, 22f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.FacePlayer();
        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.75f, 0.2f), 14, 7f);
        CombatCamera.Instance?.Impact(0.35f, 0.2f, 0.6f, 0.2f);
    }
}
