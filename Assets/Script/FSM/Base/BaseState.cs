using System;
using UnityEngine;

[Serializable]
public abstract class BaseState
{
    [NonSerialized] protected EnemyController enemyController;
    [NonSerialized] protected StateMachine stateMachine;

    public BaseState(EnemyController enemyController, StateMachine stateMachine)
    {
        this.enemyController = enemyController;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
