using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public BaseState curState;
    public string CurrentStateName => curState?.GetType().Name ?? "None";

    public void InitState(BaseState state)
    {
        curState = state;
        curState.Enter();
    }
    public void ChangeState(BaseState state)
    {
        curState.Exit();
        curState = state;
        curState.Enter();
    }

    public void Update()
    {
        curState?.Update();
    }
}
