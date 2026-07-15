using System;
using UnityEngine;

[Serializable]
public class EnrageState : BaseState
{
    public float duration;
    public float speedMul;
    public float damageMul;
    float timer;

    public EnrageState(EnemyController c, StateMachine s, EnrageStateSo d) : base(c, s)
    {
        duration = d.duration;
        speedMul = d.speedMul;
        damageMul = d.damageMul;
    }

    public override void Enter()
    {
        timer = 0f;
        enemyController.MoveSpeedMul = speedMul;
        enemyController.DamageMul = damageMul;
        enemyController.FlashColor(new Color(1f, 0.15f, 0.05f));
        BossCombatHud.Instance?.SetStateLabel("ENRAGE");
        BossCombatHud.Instance?.Shake(0.5f);
        BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.1f, 0.05f), 1f, 5f, 0.55f);
        BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.2f, 0.05f), 24, 11f);
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.15f, 0.05f), 10f);
        CombatCamera.Instance?.Impact(0.85f, 0.45f, 1.2f, 0.4f);
        CombatCamera.Instance?.HoldZoomOffset(1.4f, duration);
        Debug.Log("[Boss] ENRAGE!");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        CombatCamera.Instance?.ClearHoldZoom();
    }
}
