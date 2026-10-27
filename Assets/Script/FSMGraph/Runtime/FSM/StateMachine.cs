using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public BaseState curState;
    public string CurrentStateName => curState?.GetType().Name ?? "None";

    public void InitState(BaseState state)
    {
        ChangeState(state);
    }
    public void ChangeState(BaseState nextState)
    {
        curState?.Exit();
        curState = nextState;
        curState?.Enter();
    }

    public void Update()
    {
        curState?.Update();
    }
}
