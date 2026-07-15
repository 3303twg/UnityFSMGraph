using System;
using UnityEngine;

[Serializable]
public class RecoverState : BaseState
{
    public float duration;
    float timer;

    public RecoverState(EnemyController c, StateMachine s, RecoverStateSo d) : base(c, s)
    {
        duration = d.duration;
    }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("[Boss] Recover");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
            enemyController.Navigator.GoToNextNode();
    }
}
