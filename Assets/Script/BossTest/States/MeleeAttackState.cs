using System;
using UnityEngine;

[Serializable]
public class MeleeAttackState : BaseState
{
    public float windup;
    public float recover;
    public float damage;
    public float hitRange;

    float timer;
    bool didHit;
    bool finishedWindup;

    public MeleeAttackState(EnemyController enemyController, StateMachine stateMachine, MeleeAttackStateSo data)
        : base(enemyController, stateMachine)
    {
        windup = data.windup;
        recover = data.recover;
        damage = data.damage;
        hitRange = data.hitRange;
    }

    public override void Enter()
    {
        Debug.Log("[Boss] Melee");
        timer = 0f;
        didHit = false;
        finishedWindup = false;
        enemyController.FacePlayer();
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.FacePlayer();

        if (!finishedWindup)
        {
            if (timer >= windup)
            {
                finishedWindup = true;
                timer = 0f;
                if (enemyController.GetDistanceToPlayer() <= hitRange)
                {
                    enemyController.DamagePlayer(damage);
                    didHit = true;
                    Debug.Log($"[Boss] Melee Hit ({didHit})");
                }
                else
                {
                    Debug.Log("[Boss] Melee Miss");
                }
            }
            return;
        }

        if (timer >= recover)
            enemyController.Navigator.GoToNextNode();
    }
}
