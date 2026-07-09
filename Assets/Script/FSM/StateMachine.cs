using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public BaseState curState;

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
