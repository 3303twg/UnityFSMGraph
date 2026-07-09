using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateSo<T> : ScriptableObject where T : BaseState
{
    public T CreateState(EnemyController controller, StateMachine stateMachine)
    { 
        return (T)Activator.CreateInstance(typeof(T), controller, stateMachine);
    }
}
