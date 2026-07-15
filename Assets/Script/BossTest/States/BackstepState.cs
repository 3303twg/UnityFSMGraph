using System;
using UnityEngine;

[Serializable]
public class BackstepState : BaseState
{
    public float duration;
    public float speed;
    Vector3 dir;
    float timer;

    public BackstepState(EnemyController c, StateMachine s, BackstepStateSo d) : base(c, s)
    {
        duration = d.duration;
        speed = d.speed;
    }

    public override void Enter()
    {
        timer = 0f;
        if (enemyController.PlayerTransform != null)
        {
            dir = enemyController.transform.position - enemyController.PlayerTransform.position;
            dir.z = 0f;
            if (dir.sqrMagnitude < 0.001f)
                dir = -enemyController.FacingDirection2D();
            dir.Normalize();
        }
        else dir = -enemyController.FacingDirection2D();

        Debug.Log("[Boss] Backstep");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        enemyController.MoveInDirection(dir, speed);
        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }
}
