using System;
using UnityEngine;

[Serializable]
public class HeavySlashState : BaseState
{
    public float windup;
    public float recover;
    public float damage;
    public float hitRange;
    float timer;
    bool hit;
    float pulseTimer;

    public HeavySlashState(EnemyController c, StateMachine s, HeavySlashStateSo d) : base(c, s)
    {
        windup = d.windup;
        recover = d.recover;
        damage = d.damage;
        hitRange = d.hitRange;
    }

    public override void Enter()
    {
        timer = 0f;
        hit = false;
        pulseTimer = 0f;
        Debug.Log("[Boss] Heavy Slash (telegraph)");
        BossCombatHud.Instance?.SetStateLabel("HEAVY SLASH");
        enemyController.FacePlayer();
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.85f, 0.2f, 0.7f), 8f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        pulseTimer += Time.deltaTime;
        enemyController.FacePlayer();

        if (!hit && pulseTimer >= 0.12f)
        {
            pulseTimer = 0f;
            BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.8f, 0.2f, 0.5f), 0.6f, 1.6f, 0.18f);
        }

        if (!hit && timer >= windup)
        {
            hit = true;
            BossVfx.ClearTelegraph(enemyController.transform);
            BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.9f, 0.3f), 16, 9f);
            BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.7f, 0.1f), 1f, 3.5f, 0.28f);
            BossCombatHud.Instance?.Shake(0.3f);
            CombatCamera.Instance?.Impact(0.5f, 0.28f, -0.55f, 0.18f);

            if (enemyController.GetDistanceToPlayer() <= hitRange)
            {
                enemyController.DamagePlayer(damage);
                Debug.Log("[Boss] Heavy Slash HIT");
            }
            else Debug.Log("[Boss] Heavy Slash MISS");
            timer = 0f;
        }

        if (hit && timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
