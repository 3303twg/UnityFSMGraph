using System;
using UnityEngine;

[Serializable]
public class StalkState : BaseState
{
    public float duration;
    public float orbitSpeed;
    public float approachSpeed;
    public float preferDistance;
    public float forceDecideDistance;
    float timer;

    public StalkState(EnemyController c, StateMachine s, StalkStateSo d) : base(c, s)
    {
        duration = d.duration;
        orbitSpeed = d.orbitSpeed;
        approachSpeed = d.approachSpeed;
        preferDistance = d.preferDistance;
        forceDecideDistance = d.forceDecideDistance;
    }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("[Boss] Stalk");
        BossCombatHud.Instance?.SetStateLabel("STALK");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.StalkPlayer(orbitSpeed, approachSpeed, preferDistance);

        if (timer >= duration || enemyController.GetDistanceToPlayer() <= forceDecideDistance)
            enemyController.Navigator.GoToNextNode();
    }
}
