using System;
using UnityEngine;

[Serializable]
public class ApproachState : BaseState
{
    public float engageDistance;
    public float moveSpeed;

    public ApproachState(EnemyController enemyController, StateMachine stateMachine, ApproachStateSo data)
        : base(enemyController, stateMachine)
    {
        engageDistance = data.engageDistance;
        moveSpeed = data.moveSpeed > 0f ? data.moveSpeed : enemyController.enemyStat.moveSpeed;
    }

    public override void Enter()
    {
        Debug.Log("[Boss] Approach");
    }

    public override void Update()
    {
        enemyController.MoveTowardsPlayer(moveSpeed);

        if (enemyController.GetDistanceToPlayer() <= engageDistance)
            enemyController.Navigator.GoToNextNode();
    }
}
