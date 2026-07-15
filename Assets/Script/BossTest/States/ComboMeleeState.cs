using System;
using UnityEngine;

[Serializable]
public class ComboMeleeState : BaseState
{
    public int hitCount;
    public float windup;
    public float betweenHits;
    public float recover;
    public float damage;
    public float hitRange;

    float timer;
    int hitsDone;
    enum Step { Windup, Strike, Between, Recover }
    Step step;

    public ComboMeleeState(EnemyController c, StateMachine s, ComboMeleeStateSo d) : base(c, s)
    {
        hitCount = Mathf.Max(1, d.hitCount);
        windup = d.windup;
        betweenHits = d.betweenHits;
        recover = d.recover;
        damage = d.damage;
        hitRange = d.hitRange;
    }

    public override void Enter()
    {
        timer = 0f;
        hitsDone = 0;
        step = Step.Windup;
        Debug.Log("[Boss] Combo Melee");
        BossCombatHud.Instance?.SetStateLabel("COMBO");
        enemyController.FacePlayer();
        BossVfx.AttachTelegraph(enemyController.transform, new Color(1f, 0.4f, 0.3f), 9f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.FacePlayer();

        switch (step)
        {
            case Step.Windup:
                if (timer >= windup) Strike();
                break;
            case Step.Between:
                if (timer >= betweenHits) Strike();
                break;
            case Step.Recover:
                if (timer >= recover)
                    enemyController.Navigator.GoToNextNode();
                break;
        }
    }

    void Strike()
    {
        timer = 0f;
        hitsDone++;
        if (enemyController.GetDistanceToPlayer() <= hitRange)
        {
            enemyController.DamagePlayer(damage);
            BossVfx.SpawnSparkBurst(enemyController.transform.position, new Color(1f, 0.5f, 0.3f), 8, 6f);
            CombatCamera.Instance?.Shake(0.22f, 0.12f, 36f);
            Debug.Log($"[Boss] Combo Hit {hitsDone}/{hitCount}");
        }
        else
            Debug.Log($"[Boss] Combo Miss {hitsDone}/{hitCount}");

        if (hitsDone >= hitCount)
        {
            step = Step.Recover;
            BossVfx.ClearTelegraph(enemyController.transform);
        }
        else
            step = Step.Between;
    }

    public override void Exit()
    {
        BossVfx.ClearTelegraph(enemyController.transform);
    }
}

