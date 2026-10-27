using System;
using UnityEngine;

[Serializable]
public abstract class BaseState
{
    [NonSerialized] protected IFSMAgent agent;
    [NonSerialized] protected StateMachine stateMachine;

    public BaseState(IFSMAgent agent)
    {
        this.agent = agent;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
