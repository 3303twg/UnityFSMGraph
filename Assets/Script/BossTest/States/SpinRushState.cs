using System;
using UnityEngine;

[Serializable]
public class SpinRushState : BaseState
{
    public float duration;
    public float moveSpeed;
    public float spinSpeed;
    public float tickInterval;
    public float damage;
    public float hitRadius;

    float timer;
    float tick;
    float angle;

    public SpinRushState(EnemyController c, StateMachine s, SpinRushStateSo d) : base(c, s)
    {
        duration = d.duration;
        moveSpeed = d.moveSpeed;
        spinSpeed = d.spinSpeed;
        tickInterval = d.tickInterval;
        damage = d.damage;
        hitRadius = d.hitRadius;
    }

    public override void Enter()
    {
        timer = 0f;
        tick = 0f;
        angle = enemyController.transform.eulerAngles.z;
        BossCombatHud.Instance?.SetStateLabel("SPIN RUSH");
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.9f, 0.3f), 12f);
        BossVfx.SpawnSparkBurst(enemyController.transform.position, Color.yellow, 12, 7f);
        CombatCamera.Instance?.Shake(0.12f, duration * 0.9f, 40f);
        Debug.Log("[Boss] Spin Rush");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        tick += Time.deltaTime;
        angle += spinSpeed * Time.deltaTime;
        enemyController.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        enemyController.MoveTowardsPlayer(moveSpeed);
        BossVfx.SpawnAfterimage(enemyController.transform, new Color(1f, 0.85f, 0.2f), 0.15f);

        if (tick >= tickInterval)
        {
            tick = 0f;
            if (enemyController.GetDistanceToPlayer() <= hitRadius)
            {
                enemyController.DamagePlayer(damage);
                BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.8f, 0.2f), 6, 5f);
                CombatCamera.Instance?.Shake(0.2f, 0.1f, 45f);
            }
        }

        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
