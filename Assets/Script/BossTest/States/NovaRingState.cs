using System;
using UnityEngine;

[Serializable]
public class NovaRingState : BaseState
{
    public float windup;
    public float expandTime;
    public float maxRadius;
    public float damage;
    public float bandWidth;

    float timer;
    bool expanding;
    bool damaged;
    float radius;

    public NovaRingState(EnemyController c, StateMachine s, NovaRingStateSo d) : base(c, s)
    {
        windup = d.windup;
        expandTime = d.expandTime;
        maxRadius = d.maxRadius;
        damage = d.damage;
        bandWidth = d.bandWidth;
    }

    public override void Enter()
    {
        timer = 0f;
        expanding = false;
        damaged = false;
        radius = 0.5f;
        BossCombatHud.Instance?.SetStateLabel("NOVA");
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.4f, 0.9f), 9f);
        Debug.Log("[Boss] Nova Ring");
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (!expanding)
        {
            if (timer >= windup)
            {
                expanding = true;
                timer = 0f;
                BossVfx.ClearTelegraph(enemyController.transform);
                BossCombatHud.Instance?.Shake(0.3f);
                CombatCamera.Instance?.Impact(0.45f, 0.3f, 0.8f, 0.35f);
                CombatCamera.Instance?.HoldZoomOffset(0.9f, expandTime);
            }
            else if (Mathf.FloorToInt(timer * 10f) != Mathf.FloorToInt((timer - Time.deltaTime) * 10f))
                BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.5f, 0.95f, 0.5f), 0.5f, 1.4f, 0.15f);
            return;
        }

        float t = Mathf.Clamp01(timer / expandTime);
        radius = Mathf.Lerp(0.6f, maxRadius, t);
        if (Mathf.FloorToInt(timer * 20f) != Mathf.FloorToInt((timer - Time.deltaTime) * 20f))
            BossVfx.SpawnPulseRing(enemyController.transform.position, new Color(1f, 0.35f, 0.9f, 0.55f), radius * 0.9f, radius * 1.05f, 0.08f);

        if (!damaged)
        {
            float dist = enemyController.GetDistanceToPlayer();
            if (Mathf.Abs(dist - radius) <= bandWidth && PlayerController.Instance != null)
            {
                damaged = true;
                enemyController.DamagePlayer(damage);
                BossVfx.SpawnSparkBurst(PlayerController.Instance.transform.position, new Color(1f, 0.4f, 1f), 12, 7f);
                CombatCamera.Instance?.Impact(0.4f, 0.2f, -0.5f, 0.15f);
            }
        }

        if (timer >= expandTime)
            enemyController.Navigator.GoToNextNode();
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
        CombatCamera.Instance?.ClearHoldZoom();
    }
}
