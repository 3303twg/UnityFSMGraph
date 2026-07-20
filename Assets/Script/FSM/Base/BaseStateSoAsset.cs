using UnityEngine;

public abstract class BaseStateSoAsset : ScriptableObject
{
    public abstract BaseState CreateState(IFSMAgent agent, StateMachine stateMachine);
}

