using System;
using UnityEngine;

[Serializable]
public class SlamState : BaseState
{
    public float windup;
    public float recover;
    public float damage;
    public float radius;
    float timer;
    bool slammed;
    float pulseTimer;

    public SlamState(EnemyController c, StateMachine s, SlamStateSo d) : base(c, s)
    {
        windup = d.windup;
        recover = d.recover;
        damage = d.damage;
        radius = d.radius;
    }

    public override void Enter()
    {
        timer = 0f;
        slammed = false;
        pulseTimer = 0f;
        Debug.Log("[Boss] Slam telegraph");
        BossCombatHud.Instance?.SetStateLabel("SLAM");
        enemyController.FacePlayer();
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.25f, 0.55f, 0.75f), 7f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        pulseTimer += Time.deltaTime;

        if (!slammed && pulseTimer >= 0.1f)
        {
            pulseTimer = 0f;
            float t = Mathf.Clamp01(timer / windup);
            BossVfx.SpawnPulseRing(
                enemyController.transform.position,
                new Color(1f, 0.3f, 0.6f, 0.45f),
                0.5f + t,
                radius * (0.4f + t * 0.6f),
                0.2f);
        }

        if (!slammed && timer >= windup)
        {
            slammed = true;
            timer = 0f;
            BossVfx.ClearTelegraph(enemyController.transform);
            BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.2f, 0.5f), radius * 0.5f, radius * 1.2f, 0.35f);
            BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.4f, 0.7f), 20, 10f);
            BossCombatHud.Instance?.Shake(0.45f);
            CombatCamera.Instance?.Impact(0.7f, 0.35f, -0.9f, 0.2f);
            CombatCamera.Instance?.Kick(Vector3.down, 0.45f);

            if (enemyController.GetDistanceToPlayer() <= radius)
            {
                enemyController.DamagePlayer(damage);
                Debug.Log("[Boss] SLAM HIT");
            }
            else Debug.Log("[Boss] Slam whiff");
        }

        if (slammed && timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}
